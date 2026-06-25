using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.PolyEnumFactories
{
    [Generator]
    internal sealed class PolyEnumFactoryGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string POLY_ENUM_FACTORY_FOR_ATTRIBUTE_METADATA = $"{NAMESPACE}.PolyEnumFactoryForAttribute";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumStructAttribute";
        private const string ENUM_CASE_IGNORE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseIgnoreAttribute";
        private const string UNDEFINED_NAME = "Undefined";
        private const string DEFAULT_FIELD_NAME_PREFIX = "_enumStruct_";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  POLY_ENUM_FACTORY_FOR_ATTRIBUTE_METADATA
                , static (node, _) => IsCandidateNode(node)
                , ExtractSpec
            ).Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsCandidateNode(SyntaxNode node)
        {
            if (node is RecordDeclarationSyntax record)
            {
                return record.TypeParameterList == null;
            }

            if (node is ClassDeclarationSyntax classDecl)
            {
                return classDecl.TypeParameterList == null;
            }

            if (node is StructDeclarationSyntax structDecl)
            {
                return structDecl.TypeParameterList == null;
            }

            return false;
        }

        private static PolyEnumFactorySpec ExtractSpec(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax wrapperSyntax
                || context.TargetSymbol is not INamedTypeSymbol wrapperSymbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            if (wrapperSymbol.TypeParameters.Length > 0)
            {
                return default;
            }

            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1
                || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol enumStructSymbol
                || enumStructSymbol.IsUnboundGenericType
                || enumStructSymbol.IsGenericType
                || enumStructSymbol.HasAttribute(POLY_ENUM_STRUCT_ATTRIBUTE, token) == false
            )
            {
                return default;
            }

            var canAccessMembers = wrapperSymbol.CanAccessMembersOf(enumStructSymbol, token);

            if (canAccessMembers == CanAccessMembersResult.NoAccess)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = wrapperSyntax.SyntaxTree;
            var hintName = syntaxTree.GetHintName(wrapperSyntax, wrapperSymbol.ToFileName());

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  wrapperSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new PolyEnumFactorySpec {
                location = LocationInfo.From(wrapperSyntax.GetLocation()),
                wrapperTypeName = wrapperSymbol.Name,
                wrapperTypeNamespace = wrapperSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                wrapperKindKeyword = GetWrapperKindKeyword(wrapperSyntax),
                wrapperPreModifiers = GetWrapperPreModifiers(wrapperSyntax, token),
                wrapperAccessibility = GetAccessibilityKeyword(wrapperSymbol.DeclaredAccessibility),
                enumStructTypeName = SelectEnumStructTypeName(enumStructSymbol, canAccessMembers),
                enumStructNamespace = enumStructSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                enumStructIsReadOnly = enumStructSymbol.IsReadOnly,
                enumStructSize = 0,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                parentIsNamespace = wrapperSyntax.Parent is BaseNamespaceDeclarationSyntax,
                isStruct = IsStruct(wrapperSyntax),
            };

            ResolveBackingField(wrapperSyntax, wrapperSymbol, enumStructSymbol, ref result, token);
            AggregateCases(enumStructSymbol, canAccessMembers, ref result, token);
            ResolveExplicitUndefinedMethod(ref result, token);

            return result;
        }

        private static void ResolveBackingField(
              TypeDeclarationSyntax wrapperSyntax
            , INamedTypeSymbol wrapperSymbol
            , INamedTypeSymbol enumStructSymbol
            , ref PolyEnumFactorySpec result
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (wrapperSyntax is RecordDeclarationSyntax recordSyntax
                && recordSyntax.ParameterList is ParameterListSyntax paramListSyntax
                && paramListSyntax.Parameters.Count > 0
            )
            {
                var param = paramListSyntax.Parameters[0];

                if (param.Type is TypeSyntax type
                    && type.IsTypeNameCandidate(result.enumStructTypeName, token)
                )
                {
                    result.emitBackingField = false;
                    result.fieldName = param.Identifier.Text;
                    return;
                }
            }

            var comparer = SymbolEqualityComparer.Default;

            foreach (var ctor in wrapperSymbol.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                if (ctor.Parameters.Length < 1)
                {
                    continue;
                }

                var paramType = ctor.Parameters[0].Type;

                if (comparer.Equals(paramType, enumStructSymbol))
                {
                    result.emitBackingField = false;
                    result.fieldName = GetUserDefinedBackingField(wrapperSymbol, enumStructSymbol, token);
                    return;
                }
            }

            result.emitBackingField = true;

            token.ThrowIfCancellationRequested();

            var baseName = $"{DEFAULT_FIELD_NAME_PREFIX}{enumStructSymbol.Name}";
            var fieldName = baseName;
            var suffix = 1;

            while (HasInstanceField(wrapperSymbol, fieldName, token))
            {
                token.ThrowIfCancellationRequested();

                fieldName = $"{baseName}{suffix}";
                suffix++;
            }

            result.fieldName = fieldName;
            return;

            static string GetUserDefinedBackingField(
                  INamedTypeSymbol wrapperSymbol
                , INamedTypeSymbol enumStructSymbol
                , CancellationToken token
            )
            {
                var comparer = SymbolEqualityComparer.Default;

                foreach (var member in wrapperSymbol.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IFieldSymbol field
                        && field.IsStatic == false
                        && comparer.Equals(field.Type, enumStructSymbol)
                    )
                    {
                        return field.Name;
                    }
                }

                return string.Empty;
            }
        }

        private static bool HasInstanceField(INamedTypeSymbol typeSymbol, string name, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var member in typeSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is IFieldSymbol field
                    && field.IsStatic == false
                    && string.Equals(field.Name, name, StringComparison.Ordinal)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static void AggregateCases(
              INamedTypeSymbol enumStructSymbol
            , CanAccessMembersResult access
            , ref PolyEnumFactorySpec result
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var casesBuilder = ImmutableArrayBuilder<PolyEnumFactorySpec.CaseSpec>.Rent();

            var verboseUndefined = $"{enumStructSymbol.Name}_Undefined";

            foreach (var nested in enumStructSymbol.GetTypeMembers())
            {
                token.ThrowIfCancellationRequested();

                if (nested.TypeKind != TypeKind.Struct)
                {
                    continue;
                }

                if (nested.IsGenericType)
                {
                    continue;
                }

                if (nested.HasAttribute(ENUM_CASE_IGNORE_ATTRIBUTE, token))
                {
                    continue;
                }

                if (IsSupportedAccessibility(nested.DeclaredAccessibility) == false)
                {
                    continue;
                }

                var caseSpec = BuildCaseSpec(enumStructSymbol, nested, access, verboseUndefined, token);

                if (caseSpec.IsValid == false)
                {
                    continue;
                }

                casesBuilder.Add(caseSpec);
            }

            result.cases = casesBuilder.ToImmutable().AsEquatableArray();
        }

        private static PolyEnumFactorySpec.CaseSpec BuildCaseSpec(
              INamedTypeSymbol enumStructSymbol
            , INamedTypeSymbol caseSymbol
            , CanAccessMembersResult access
            , string verboseUndefined
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var isUndefined = string.Equals(caseSymbol.Name, UNDEFINED_NAME, StringComparison.Ordinal)
                || string.Equals(caseSymbol.Name, verboseUndefined, StringComparison.Ordinal);

            var spec = new PolyEnumFactorySpec.CaseSpec {
                name = caseSymbol.Name,
                identifier = caseSymbol.Name.ToValidIdentifier(),
                qualifiedName = SelectCaseStructTypeName(enumStructSymbol, caseSymbol, access),
                isUndefined = isUndefined,
                isReadOnly = caseSymbol.IsReadOnly,
                size = 0,
                strategy = PolyEnumFactorySpec.ConstructionStrategy.Default,
                ctors = default,
                initMembers = default,
            };

            if (TryBuildCtorList(caseSymbol, token, out var ctors))
            {
                spec.strategy = PolyEnumFactorySpec.ConstructionStrategy.Ctors;
                spec.ctors = ctors;

                if (TryBuildMemberInitList(caseSymbol, token, out var settableMembers))
                {
                    var maxCtorParamCount = 0;

                    for (var i = 0; i < ctors.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        var count = ctors[i].parameters.Count;

                        if (count > maxCtorParamCount)
                        {
                            maxCtorParamCount = count;
                        }
                    }

                    if (maxCtorParamCount < settableMembers.Count)
                    {
                        spec.initMembers = settableMembers;
                        spec.emitMemberInitOverload = true;
                    }
                }

                return spec;
            }

            if (TryBuildMemberInitList(caseSymbol, token, out var members))
            {
                spec.strategy = PolyEnumFactorySpec.ConstructionStrategy.MemberInit;
                spec.initMembers = members;
                return spec;
            }

            spec.strategy = PolyEnumFactorySpec.ConstructionStrategy.Default;
            return spec;
        }

        private static bool TryBuildCtorList(
              INamedTypeSymbol caseSymbol
            , CancellationToken token
            , out EquatableArray<PolyEnumFactorySpec.CtorSpec> result
        )
        {
            using var builder = ImmutableArrayBuilder<PolyEnumFactorySpec.CtorSpec>.Rent();

            foreach (var ctor in caseSymbol.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                if (IsSupportedAccessibility(ctor.DeclaredAccessibility) == false)
                {
                    continue;
                }

                if (ctor.IsImplicitlyDeclared)
                {
                    if (ctor.Parameters.Length < 1)
                    {
                        continue;
                    }
                }

                if (HasOutParameter(ctor, token))
                {
                    continue;
                }

                var paramArray = BuildParamSpecs(ctor, token);

                builder.Add(new PolyEnumFactorySpec.CtorSpec {
                    parameters = paramArray,
                    isParameterless = ctor.Parameters.Length == 0,
                });
            }

            if (builder.Count < 1)
            {
                result = default;
                return false;
            }

            result = builder.ToImmutable().AsEquatableArray();
            return true;
        }

        private static bool HasOutParameter(IMethodSymbol ctor, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var p in ctor.Parameters)
            {
                token.ThrowIfCancellationRequested();

                if (p.RefKind == RefKind.Out)
                {
                    return true;
                }
            }

            return false;
        }

        private static EquatableArray<PolyEnumFactorySpec.ParamSpec> BuildParamSpecs(
              IMethodSymbol ctor
            , CancellationToken token
        )
        {
            using var builder = ImmutableArrayBuilder<PolyEnumFactorySpec.ParamSpec>.Rent();

            foreach (var p in ctor.Parameters)
            {
                token.ThrowIfCancellationRequested();

                var hasDefault = p.HasExplicitDefaultValue && p.RefKind == RefKind.None;
                var literal = hasDefault ? FormatDefaultValue(p) : null;
                var paramName = NameCasing.Camel.ConvertName(p.Name);

                builder.Add(new PolyEnumFactorySpec.ParamSpec {
                    name = paramName,
                    typeFullyQualifiedName = p.Type.ToFullName(),
                    refKind = p.RefKind,
                    isParams = p.IsParams,
                    hasExplicitDefaultValue = hasDefault,
                    defaultValueLiteral = literal,
                });
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private static bool TryBuildMemberInitList(
              INamedTypeSymbol caseSymbol
            , CancellationToken token
            , out EquatableArray<PolyEnumFactorySpec.MemberSpec> result
        )
        {
            using var builder = ImmutableArrayBuilder<PolyEnumFactorySpec.MemberSpec>.Rent();
            var seenNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in caseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member.IsStatic || member.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (IsSupportedAccessibility(member.DeclaredAccessibility) == false)
                {
                    continue;
                }

                if (member is IFieldSymbol field)
                {
                    if (field.IsConst || field.IsReadOnly)
                    {
                        continue;
                    }

                    if (seenNames.Add(field.Name) == false)
                    {
                        continue;
                    }

                    builder.Add(new PolyEnumFactorySpec.MemberSpec {
                        name = field.Name,
                        parameterName = NameCasing.Camel.ConvertName(field.Name),
                        typeFullyQualifiedName = field.Type.ToFullName(),
                        isProperty = false,
                    });
                }
                else if (member is IPropertySymbol property)
                {
                    if (property.IsIndexer)
                    {
                        continue;
                    }

                    var setter = property.SetMethod;

                    if (setter is null || IsSupportedAccessibility(setter.DeclaredAccessibility) == false)
                    {
                        continue;
                    }

                    if (seenNames.Add(property.Name) == false)
                    {
                        continue;
                    }

                    builder.Add(new PolyEnumFactorySpec.MemberSpec {
                        name = property.Name,
                        parameterName = NameCasing.Camel.ConvertName(property.Name),
                        typeFullyQualifiedName = property.Type.ToFullName(),
                        isProperty = true,
                    });
                }
            }

            if (builder.Count < 1)
            {
                result = default;
                return false;
            }

            result = builder.ToImmutable().AsEquatableArray();
            return true;
        }

        private static void ResolveExplicitUndefinedMethod(ref PolyEnumFactorySpec result, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var cases = result.cases;
            var hasParameterlessUndefined = false;

            for (var i = 0; i < cases.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var c = cases[i];

                if (c.isUndefined == false)
                {
                    continue;
                }

                switch (c.strategy)
                {
                    case PolyEnumFactorySpec.ConstructionStrategy.Default:
                        hasParameterlessUndefined = true;
                        break;

                    case PolyEnumFactorySpec.ConstructionStrategy.Ctors:
                    {
                        var ctors = c.ctors;
                        for (var j = 0; j < ctors.Count; j++)
                        {
                            if (ctors[j].isParameterless)
                            {
                                hasParameterlessUndefined = true;
                                break;
                            }
                        }

                        break;
                    }
                }

                if (hasParameterlessUndefined)
                {
                    break;
                }
            }

            result.emitExplicitUndefinedMethod = hasParameterlessUndefined == false;
        }

        private static bool IsStruct(TypeDeclarationSyntax typeSyntax)
        {
            return typeSyntax is RecordDeclarationSyntax record
                ? record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                : typeSyntax is StructDeclarationSyntax;
        }

        private static bool IsSupportedAccessibility(Accessibility accessibility)
        {
            return accessibility >= Accessibility.Internal;
        }

        private static string GetWrapperKindKeyword(TypeDeclarationSyntax typeSyntax)
        {
            if (typeSyntax is RecordDeclarationSyntax record)
            {
                return record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                    ? "record struct"
                    : "record class";
            }

            if (typeSyntax is StructDeclarationSyntax)
            {
                return "struct";
            }

            if (typeSyntax is ClassDeclarationSyntax)
            {
                return "class";
            }

            return string.Empty;
        }

        private static string GetWrapperPreModifiers(TypeDeclarationSyntax typeSyntax, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var sb = StringExtensions.RentSb();

            foreach (var modifier in typeSyntax.Modifiers)
            {
                token.ThrowIfCancellationRequested();

                if (modifier.IsKind(SyntaxKind.PartialKeyword))
                {
                    continue;
                }

                if (IsAccessibilityModifier(modifier))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(modifier.ValueText);
            }

            var result = sb.ToString();
            StringExtensions.ReturnSb(sb);
            return result;
        }

        private static string SelectEnumStructTypeName(INamedTypeSymbol type, CanAccessMembersResult access)
        {
            switch (access)
            {
                case CanAccessMembersResult.EnclosingType:
                case CanAccessMembersResult.SameAssemblyInheritance:
                case CanAccessMembersResult.CrossAssemblyInheritance:
                    return type.Name;

                default:
                    return type.ToFullName();
            }
        }

        private static string SelectCaseStructTypeName(
              INamedTypeSymbol enumStructType
            , INamedTypeSymbol caseStructType
            , CanAccessMembersResult access
        )
        {
            switch (access)
            {
                case CanAccessMembersResult.EnclosingType:
                case CanAccessMembersResult.SameAssemblyInheritance:
                case CanAccessMembersResult.CrossAssemblyInheritance:
                    return $"{enumStructType.Name}.{caseStructType.Name}";

                default:
                    return caseStructType.ToFullName();
            }
        }

        private static bool IsAccessibilityModifier(SyntaxToken token)
            => token.IsKind(SyntaxKind.PublicKeyword)
            || token.IsKind(SyntaxKind.PrivateKeyword)
            || token.IsKind(SyntaxKind.ProtectedKeyword)
            || token.IsKind(SyntaxKind.InternalKeyword)
            ;

        private static string GetAccessibilityKeyword(Accessibility accessibility)
        {
            return accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => string.Empty,
            };
        }

        private static string FormatDefaultValue(IParameterSymbol parameter)
        {
            var value = parameter.ExplicitDefaultValue;
            var type = parameter.Type;

            if (value is null)
            {
                if (type.IsReferenceType || type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return "null";
                }

                return $"default({type.ToFullName()})";
            }

            if (type.TypeKind == TypeKind.Enum && type is INamedTypeSymbol enumType)
            {
                return $"({enumType.ToFullName()})({FormatPrimitive(value)})";
            }

            return FormatPrimitive(value);
        }

        private static string FormatPrimitive(object value)
        {
            return value switch {
                null => "null",
                bool b => b ? "true" : "false",
                string s => $"\"{StringExtensions.EscapeStringLiteral(s)}\"",
                char c => FormatCharLiteral(c),
                float f => $"{f.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}f",
                double d => $"{d.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}d",
                decimal m => $"{m.ToString(System.Globalization.CultureInfo.InvariantCulture)}m",
                long l => $"{l.ToString(System.Globalization.CultureInfo.InvariantCulture)}L",
                ulong ul => $"{ul.ToString(System.Globalization.CultureInfo.InvariantCulture)}UL",
                uint ui => $"{ui.ToString(System.Globalization.CultureInfo.InvariantCulture)}U",
                int i => i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                short sh => $"(short){sh.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                ushort ush => $"(ushort){ush.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                byte by => $"(byte){by.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                sbyte sb => $"(sbyte){sb.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                _ => value.ToString(),
            };
        }

        private static string FormatCharLiteral(char c)
        {
            switch (c)
            {
                case '\\': return "'\\\\'";
                case '\'': return "'\\''";
                case '\0': return "'\\0'";
                case '\a': return "'\\a'";
                case '\b': return "'\\b'";
                case '\f': return "'\\f'";
                case '\n': return "'\\n'";
                case '\r': return "'\\r'";
                case '\t': return "'\\t'";
                case '\v': return "'\\v'";
                default:
                    if (char.IsControl(c) || c > 127)
                    {
                        return $"'\\u{((int)c).ToString("x4", System.Globalization.CultureInfo.InvariantCulture)}'";
                    }

                    return $"'{c}'";
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , PolyEnumFactorySpec candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var hintName = candidate.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , candidate.WriteCode(compilation)
                    , candidate.closingSource
                    , candidate.hintName
                    , sourceFilePath
                    , projectPath
                );
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , candidate.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_POLY_ENUM_FACTORY_UNKNOWN_0001"
                , "Poly Enum Factory Generator Error"
                , "This error indicates a bug in the Poly Enum Factory source generators. Error message: '{0}'."
                , NAMESPACE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using ETCol = global::EncosyTower.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
