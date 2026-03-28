using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Common.UnionIds;
using EncosyTower.SourceGen.Generators.EnumExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    [Generator]
    internal class UnionIdGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.UnionIds";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string UNION_ID_ATTRIBUTE_METADATA = $"{NAMESPACE}.UnionIdAttribute";
        public const string KIND_FOR_UNION_ID_ATTRIBUTE_METADATA = $"{NAMESPACE}.KindForUnionIdAttribute";
        public const string GENERATOR_NAME = nameof(UnionIdGenerator);

        private const string UNION_ID_ATTRIBUTE_FULL = $"global::{UNION_ID_ATTRIBUTE_METADATA}";
        private const string UNION_ID_KIND_ATTRIBUTE_FULL = $"global::{NAMESPACE}.UnionIdKindAttribute";
        private const string ENUM_EXTENSIONS_ATTRIBUTE_FULL = "global::EncosyTower.EnumExtensions.EnumExtensionsAttribute";
        private const string FLAGS_ATTRIBUTE_FULL = "global::System.FlagsAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var idProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  UNION_ID_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax
                , GetUnionIdInfo
            ).Where(static x => x.IsValid);

            var kindProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  KIND_FOR_UNION_ID_ATTRIBUTE_METADATA
                , static (node, _) => node is BaseTypeDeclarationSyntax typeSyntax
                    && typeSyntax.Kind() is SyntaxKind.EnumDeclaration
                        or SyntaxKind.StructDeclaration
                        or SyntaxKind.RecordStructDeclaration
                , GetKindForUnionIdInfo
            ).Where(static x => string.IsNullOrEmpty(x.kindFullName) == false
                             && string.IsNullOrEmpty(x.idFullName) == false);

            var combined = idProvider
                .Combine(kindProvider.Collect())
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left.Left
                    , source.Left.Left.Right
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static IdDeclaration GetUnionIdInfo(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol
                || symbol.IsUnmanagedType == false
                || symbol.IsUnboundGenericType
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attrib = context.Attributes[0];
            var info = new IdDeclaration {
                separator = '-',
            };

            var args = attrib.NamedArguments;

            foreach (var arg in args)
            {
                switch (arg.Key)
                {
                    case "Size":
                    {
                        if (arg.Value.Value is byte val)
                            info.size = (UnionIdSize)val;
                        break;
                    }

                    case "DisplayNameForId":
                    {
                        if (arg.Value.Value is string val)
                            info.displayNameForId = val;
                        break;
                    }

                    case "DisplayNameForKind":
                    {
                        if (arg.Value.Value is string val)
                            info.displayNameForKind = val;
                        break;
                    }

                    case "Separator":
                    {
                        if (arg.Value.Value is char val)
                            info.separator = val;
                        break;
                    }

                    case "KindSettings":
                    {
                        if (arg.Value.Value is byte val)
                            info.kindSettings = (UnionIdKindSettings)val;
                        break;
                    }

                    case "ConverterSettings":
                    {
                        if (arg.Value.Value is byte val)
                            info.converterSettings = (ParsableStructConverterSettings)val;
                        break;
                    }

                    case "FixedStringBytes":
                    {
                        if (arg.Value.Value is ushort val && val > 0)
                        {
                            info.fixedStringBytes = val switch {
                                <= 32 => 32 - 3,
                                <= 64 => 64 - 3,
                                <= 128 => 128 - 3,
                                <= 512 => 512 - 3,
                                _ => 4096 - 3,
                            };
                        }
                        break;
                    }
                }
            }

            var ns = symbol.ContainingNamespace;
            info.fullName = symbol.ToFullName();
            info.simpleName = symbol.Name;
            info.fileHintName = symbol.ToFileName();
            info.namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;
            info.accessibility = symbol.DeclaredAccessibility;
            info.location = LocationInfo.From(context.TargetNode.GetLocation());

            // Determine if parent is a namespace (for EnumExtensionsDeclaration)
            info.parentIsNamespace = context.TargetNode.Parent is BaseNamespaceDeclarationSyntax;

            info.containingTypes = symbol.GetContainingTypes();

            // Inline kinds via [UnionIdKind] attributes on this struct
            info.inlineKinds = GetInlineKinds(symbol, info.fullName, token);

            return info;
        }

        private static KindDeclaration GetKindForUnionIdInfo(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol kindSymbol
                || kindSymbol.IsUnmanagedType == false
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attrib = context.Attributes[0];

            if (attrib.ConstructorArguments.Length < 1)
                return default;

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type
                || typeArg.Value is not INamedTypeSymbol idSymbol
                || idSymbol.IsUnmanagedType == false
                || idSymbol.IsUnboundGenericType
                || idSymbol.HasAttribute(UNION_ID_ATTRIBUTE_FULL) == false
            )
            {
                return default;
            }

            var ctorArgs = attrib.ConstructorArguments;
            ulong order = 0;
            string name = null;
            string displayName = null;
            bool signed = false;
            var toStringMethods = ToStringMethods.Default;
            var tryParseMethodType = TryParseMethodType.None;

            for (var i = 1; i < ctorArgs.Length; i++)
            {
                var arg = ctorArgs[i];

                if (i == 1 && arg.Value is ulong ulongVal) order = ulongVal;
                else if (i == 2 && arg.Value is string stringVal1) name = stringVal1;
                else if (i == 3 && arg.Value is string stringVal2) displayName = stringVal2;
                else if (i == 4 && arg.Value is bool boolVal) signed = boolVal;
                else if (i == 5 && arg.Value is byte byteVal1) toStringMethods = (ToStringMethods)byteVal1;
                else if (i == 6 && arg.Value is byte byteVal2) tryParseMethodType = (TryParseMethodType)byteVal2;
            }

            return BuildKindInfo(
                  kindSymbol
                , idSymbol.ToFullName()
                , order
                , name
                , displayName
                , signed
                , toStringMethods
                , tryParseMethodType
                , LocationInfo.From(context.TargetNode.GetLocation())
                , token
            );
        }

        private static EquatableArray<KindDeclaration> GetInlineKinds(
              INamedTypeSymbol idSymbol
            , string idFullName
            , CancellationToken token
        )
        {
            var attributes = idSymbol.GetAttributes(UNION_ID_KIND_ATTRIBUTE_FULL);
            using var builder = ImmutableArrayBuilder<KindDeclaration>.Rent();

            foreach (var attrib in attributes)
            {
                if (attrib == null)
                    continue;

                var args = attrib.ConstructorArguments;

                if (args.Length < 1)
                    continue;

                var typeArg = args[0];

                if (typeArg.Kind != TypedConstantKind.Type
                    || typeArg.Value is not INamedTypeSymbol kindSymbol
                    || kindSymbol.IsUnmanagedType == false
                )
                {
                    continue;
                }

                ulong order = 0;
                string name = null;
                string displayName = null;
                bool signed = false;
                var toStringMethods = ToStringMethods.Default;
                var tryParseMethodType = TryParseMethodType.None;

                for (var i = 1; i < args.Length; i++)
                {
                    var arg = args[i];

                    if (i == 1 && arg.Value is ulong ulongVal) order = ulongVal;
                    else if (i == 2 && arg.Value is string stringVal1) name = stringVal1;
                    else if (i == 3 && arg.Value is string stringVal2) displayName = stringVal2;
                    else if (i == 4 && arg.Value is bool boolVal) signed = boolVal;
                    else if (i == 5 && arg.Value is byte byteVal1) toStringMethods = (ToStringMethods)byteVal1;
                    else if (i == 6 && arg.Value is byte byteVal2) tryParseMethodType = (TryParseMethodType)byteVal2;
                }

                var kindInfo = BuildKindInfo(kindSymbol, idFullName, order, name, displayName, signed, toStringMethods, tryParseMethodType,
                    default, token);

                if (string.IsNullOrEmpty(kindInfo.kindFullName) == false)
                    builder.Add(kindInfo);
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private static KindDeclaration BuildKindInfo(
              INamedTypeSymbol kindSymbol
            , string idFullName
            , ulong order
            , string name
            , string displayName
            , bool signed
            , ToStringMethods toStringMethods
            , TryParseMethodType tryParseMethodType
            , LocationInfo location
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var info = new KindDeclaration {
                kindFullName = kindSymbol.ToFullName(),
                kindSimpleName = kindSymbol.Name,
                idFullName = idFullName,
                order = order,
                name = name ?? string.Empty,
                displayName = displayName ?? string.Empty,
                signed = signed,
                toStringMethods = toStringMethods,
                location = location,
            };

            var size = 0;
            kindSymbol.GetUnmanagedSize(ref size);
            info.kindUnmanagedSize = size;

            var isEnum = kindSymbol.TypeKind == TypeKind.Enum;
            info.isEnum = isEnum;
            info.isKindAlsoUnionId = kindSymbol.HasAttribute(UNION_ID_ATTRIBUTE_FULL);

            if (isEnum)
            {
                info.kindEnumHasFlags = kindSymbol.HasAttribute(FLAGS_ATTRIBUTE_FULL);
                info.kindEnumUnderlyingTypeName = kindSymbol.EnumUnderlyingType?.ToDisplayString() ?? "int";
                info.hasExternalEnumExtensions = kindSymbol.TryGetAttribute(ENUM_EXTENSIONS_ATTRIBUTE_FULL, out _);

                if (info.hasExternalEnumExtensions)
                {
                    info.externalEnumExtensionsFullName = $"{kindSymbol.ToFullName()}Extensions";
                }

                // Collect enum members
                var enumMembers = kindSymbol.GetMembers();
                using var memberBuilder = ImmutableArrayBuilder<EnumMemberDeclaration>.Rent();
                var maxBytes = 0;
                var hasDisplayAttr = false;

                foreach (var member in enumMembers)
                {
                    if (member is not IFieldSymbol field || field.ConstantValue is null)
                        continue;

                    string memberDisplayName = null;

                    foreach (var attr in field.GetAttributes())
                    {
                        var attrName = attr.AttributeClass?.Name ?? string.Empty;

                        switch (attrName)
                        {
                            case nameof(ObsoleteAttribute):
                                goto CONTINUE;

                            case "LabelAttribute":
                            case "DescriptionAttribute":
                            case "DisplayAttribute":
                            case "DisplayNameAttribute":
                            case "InspectorNameAttribute":
                            {
                                if (attr.ConstructorArguments.Length > 0)
                                {
                                    var a = attr.ConstructorArguments[0];

                                    if (a.Kind == TypedConstantKind.Primitive && a.Value?.ToString() is string dn)
                                    {
                                        memberDisplayName = dn;
                                        goto ADD;
                                    }
                                }
                                else if (attr.NamedArguments.Length > 0)
                                {
                                    foreach (var na in attr.NamedArguments)
                                    {
                                        if (na.Key is "Name" or "DisplayName"
                                            && na.Value.Kind == TypedConstantKind.Primitive
                                            && na.Value.Value?.ToString() is string dn
                                        )
                                        {
                                            memberDisplayName = dn;
                                            goto ADD;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }

                    ADD:
                    {
                        if (string.IsNullOrEmpty(memberDisplayName) == false) hasDisplayAttr = true;

                        var nb = member.Name.GetByteCount();
                        var db = memberDisplayName.GetByteCount();
                        maxBytes = Math.Max(maxBytes, Math.Max(nb, db));
                        memberBuilder.Add(new EnumMemberDeclaration { name = member.Name, displayName = memberDisplayName });
                        continue;
                    }

                    CONTINUE:
                    {
                        continue;
                    }
                }

                info.kindEnumValues = memberBuilder.ToImmutable().AsEquatableArray();
                info.kindEnumFixedStringBytes = maxBytes;
                info.kindEnumIsDisplayAttributeUsed = hasDisplayAttr;

                info.toStringMethods |= ToStringMethods.ToDisplayString;
            }
            else if (info.isKindAlsoUnionId)
            {
                info.tryParseSpan = new MemberExistence(true, false, false, 4);
                info.equality = new Equality(EqualityStrategy.Equals, false, false);
                info.toStringMethods |= ToStringMethods.All;
            }
            else
            {
                info.tryParseSpan = tryParseMethodType switch {
                    TryParseMethodType.Instance => new MemberExistence(true, false, false, 4),
                    TryParseMethodType.Static => new MemberExistence(true, true, false, 4),
                    _ => kindSymbol.FindTryParseSpan().DefaultIfNullableIs(true),
                };

                info.equality = kindSymbol.DetermineEquality();

                if (info.equality.IsNullable)
                    info.kindFullNameFromNullable = kindSymbol.GetTypeFromNullable().ToFullName();

                if (CheckToDisplayFixedString(kindSymbol))
                {
                    info.hasToDisplayString = true;
                    info.toStringMethods |= ToStringMethods.ToDisplayString;
                }

                if (CheckToFixedString(kindSymbol, "ToFixedString", out var fixedBytes))
                {
                    info.hasToFixedString = true;
                    info.toFixedStringBytes = fixedBytes;
                }

                if (CheckToFixedString(kindSymbol, "ToDisplayFixedString", out var displayFixedBytes))
                {
                    info.hasToDisplayFixedString = true;
                    info.toDisplayFixedStringBytes = displayFixedBytes;
                }
            }

            return info;
        }

        private static bool CheckToFixedString(INamedTypeSymbol symbol, string name, out int byteCount)
        {
            var members = symbol.GetMembers(name);

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic
                    || method.Parameters.Length != 0
                    || method.ReturnsVoid
                )
                {
                    continue;
                }

                var returnTypeName = method.ReturnType.ToFullName();

                if (returnTypeName.StartsWith("global::Unity.Collections.FixedString", StringComparison.Ordinal) == false)
                    continue;

                var indexOfBytes = returnTypeName.IndexOf("Bytes", 36);

                if (indexOfBytes < 39)
                    continue;

                if (int.TryParse(returnTypeName.Substring(37, indexOfBytes - 37), out byteCount))
                    return true;
            }

            byteCount = 0;
            return false;
        }

        private static bool CheckToDisplayFixedString(INamedTypeSymbol symbol)
        {
            var members = symbol.GetMembers("ToDisplayString");

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic
                    || method.Parameters.Length != 0
                    || method.ReturnType.SpecialType != SpecialType.System_String
                )
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , IdDeclaration idInfo
            , ImmutableArray<KindDeclaration> kindInfos
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (idInfo.IsValid == false)
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;
;
                var assemblyName = compilation.assemblyName;
                var declaration = new UnionIdDeclaration(
                      idInfo
                    , kindInfos
                    , compilation.references
                );

                if (declaration.IsInvalid)
                    return;

                var hintName = $"{GENERATOR_NAME}__{idInfo.fileHintName}.g.cs";
                var sourceFilePath = BuildSourceFilePath(assemblyName, hintName, projectPath);
                var source = declaration.WriteCode();

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , idInfo.location.ToLocation()
                        , sourceFilePath
                        , sourceText
                        , projectPath
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , idInfo.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static string BuildSourceFilePath(string assemblyName, string hintName, string projectPath = null)
        {
            if (projectPath is not null)
            {
                var dir = $"{projectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var dir = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            return $"Temp/GeneratedCode/{assemblyName}/{hintName}";
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_UNION_ID_01"
                , "UnionId Generator Error"
                , "This error indicates a bug in the UnionId source generators. Error message: '{0}'."
                , "EncosyTower.UnionIdAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
