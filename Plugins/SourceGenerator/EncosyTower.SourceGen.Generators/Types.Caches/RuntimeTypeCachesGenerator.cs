using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Types.Caches;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Types.Caches
{
    [Generator]
    internal sealed class RuntimeTypeCachesGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Types.Caches";
        public const string NAMESPACE_PREFIX = $"global::{NAMESPACE}";
        public const string SKIP_ATTRIBUTE = $"{NAMESPACE_PREFIX}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(RuntimeTypeCachesGenerator);
        public const string RUNTIME_TYPE_CACHE = "global::EncosyTower.Types.RuntimeTypeCache";

        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Types.Caches.RuntimeTypeCachesGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_RUNTIME_TYPE_CACHES = "[ETTCSG.GeneratedRuntimeTypeCaches]";
        private const string PRESERVE = "[UES.Preserve]";
        private const string EDITOR_BROWSABLE_NEVER = "[SCM.EditorBrowsable(SCM.EditorBrowsableState.Never)]";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var partialTypeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: ExtractPartialType
            ).Where(static c => c.IsValid);

            var combined = partialTypeProvider
                .Combine(projectPathProvider)
                .Combine(compilationProvider)
                .Where(static t => t.Right.isValid);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Right
                    , source.Left.Left
                    , source.Left.Right.projectPath
                    , source.Left.Right.outputSourceGenFiles
                );
            });

            var typesProvider = partialTypeProvider.Select(static (x, _) => TypeSpec.From(x))
                .Collect()
                .Combine(projectPathProvider)
                .Combine(compilationProvider);

            context.RegisterSourceOutput(typesProvider, static (sourceProductionContext, source) => {
                GenerateHeaderOutput(
                      sourceProductionContext
                    , source.Right
                    , source.Left.Left
                    , source.Left.Right.projectPath
                    , source.Left.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is MemberAccessExpressionSyntax syntax
                && syntax.Expression is IdentifierNameSyntax { Identifier.ValueText: "RuntimeTypeCache" }
                && syntax.Name is GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } member
                && IsMemberSupported(member.Identifier.ValueText)
                && GetContainingType(syntax, token) is not null;
        }

        private static bool IsMemberSupported(string memberName)
        {
            return memberName switch {
                RuntimeTypeCacheMethods.GET_INFO => true,
                RuntimeTypeCacheMethods.GET_TYPES_DERIVED_FROM => true,
                RuntimeTypeCacheMethods.GET_TYPES_WITH_ATTRIBUTE => true,
                RuntimeTypeCacheMethods.GET_FIELDS_WITH_ATTRIBUTE => true,
                RuntimeTypeCacheMethods.GET_METHODS_WITH_ATTRIBUTE => true,
                _ => false,
            };
        }

        private static PartialTypeSpec ExtractPartialType(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not MemberAccessExpressionSyntax syntax
                || syntax.Expression is not IdentifierNameSyntax identifier
                || syntax.Name is not GenericNameSyntax member
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var identifierType = semanticModel.GetTypeInfo(identifier, token).Type;

            if (identifierType.HasFullName(RUNTIME_TYPE_CACHE, token) == false)
            {
                return default;
            }

            var typeArgList = member.TypeArgumentList;
            var typeInfo = semanticModel.GetTypeInfo(typeArgList.Arguments[0], token);

            if (typeInfo.Type is not INamedTypeSymbol type)
            {
                return default;
            }

            var cacheAttributeType = member.Identifier.ValueText switch {
                RuntimeTypeCacheMethods.GET_INFO => CacheAttributeType.CacheType,
                RuntimeTypeCacheMethods.GET_TYPES_DERIVED_FROM => CacheAttributeType.CacheTypesDerivedFrom,
                RuntimeTypeCacheMethods.GET_TYPES_WITH_ATTRIBUTE => CacheAttributeType.CacheTypesWithAttribute,
                RuntimeTypeCacheMethods.GET_FIELDS_WITH_ATTRIBUTE => CacheAttributeType.CacheFieldsWithAttribute,
                RuntimeTypeCacheMethods.GET_METHODS_WITH_ATTRIBUTE => CacheAttributeType.CacheMethodsWithAttribute,
                _ => CacheAttributeType.None,
            };

            if (cacheAttributeType == CacheAttributeType.None)
            {
                return default;
            }

            var typeFullName = type.ToFullName();

            if (cacheAttributeType == CacheAttributeType.CacheType)
            {
                if (type.IsStatic)
                {
                    return default;
                }
            }
            else if (cacheAttributeType == CacheAttributeType.CacheTypesDerivedFrom)
            {
                if (type.TypeKind is not (TypeKind.Class or TypeKind.Interface)
                    || type.IsStatic
                    || type.IsSealed)
                {
                    return default;
                }
            }
            else if (typeFullName.StartsWith(NAMESPACE_PREFIX))
            {
                return default;
            }

            var assemblyName = string.Empty;

            if (syntax.Parent is InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments })
            {
                var constValueOpt = semanticModel.GetConstantValue(arguments[0].Expression, token);

                if (constValueOpt is { HasValue: true, Value: string asmName })
                {
                    assemblyName = string.IsNullOrWhiteSpace(asmName) ? string.Empty : asmName;
                }
                else
                {
                    return default;
                }
            }

            var containingSyntax = GetContainingType(syntax, token);

            if (containingSyntax is null)
            {
                return default;
            }

            var synmbolInfo = semanticModel.GetSymbolInfo(containingSyntax, token);

            if (synmbolInfo.Symbol is not ITypeSymbol containingType)
            {
                return default;
            }

            var isStruct = containingType.TypeKind == TypeKind.Struct;

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  containingSyntax
                , token
                , out var scopeOpening
                , out var scopeClosing
                , printAdditionalUsings: PrintAdditionalUsings
            );

            return new PartialTypeSpec {
                containingTypeFullName = containingType.ToFullName(),
                containingTypeFileName = containingType.ToFileName(),
                containingTypeIdentifier = containingSyntax.Identifier.ValueText,
                isStruct = isStruct,
                isRefStruct = isStruct && containingSyntax.Modifiers.Any(SyntaxKind.RefKeyword),
                isRecord = containingSyntax.Modifiers.Any(SyntaxKind.RecordKeyword),
                openingSource = scopeOpening,
                closingSource = scopeClosing,
                typeFullName = typeFullName,
                cacheAttributeType = cacheAttributeType,
                assemblyName = assemblyName,
                syntaxTreeStableHash = containingSyntax.SyntaxTree.GetStableHashCode(),
                syntaxLineNumber = syntax.GetLineNumber(),
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SCM = global::System.ComponentModel;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ETTC = global::EncosyTower.Types.Caches;");
            p.PrintLine("using ETTCSG = global::EncosyTower.Types.Caches.SourceGen;");
            p.PrintLine("using UES = global::UnityEngine.Scripting;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static TypeDeclarationSyntax GetContainingType(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var parent = node.Parent;

            while (parent is not null && parent is not TypeDeclarationSyntax)
            {
                token.ThrowIfCancellationRequested();

                parent = parent.Parent;
            }

            return parent as TypeDeclarationSyntax;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , PartialTypeSpec type
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var fileName = type.containingTypeFileName;
                var stableHash = type.syntaxTreeStableHash;
                var lineNumber = type.syntaxLineNumber;
                var hintName = SourceGenHelpers.BuildHintName(assemblyName, fileName, stableHash, lineNumber);
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , type.openingSource
                    , WriteCode(type)
                    , type.closingSource
                    , hintName
                    , sourceFilePath
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
                    , Location.None
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static void GenerateHeaderOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , ImmutableArray<TypeSpec> types
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var type in types)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var assemblyName = compilation.assemblyName;
                    var fileName = type.containingTypeFileName;
                    var stableHash = type.syntaxTreeStableHash;
                    var hintName = SourceGenHelpers.BuildHintName(assemblyName, fileName, stableHash, 0);

                    if (seen.Add(hintName) == false)
                    {
                        continue;
                    }

                    var filePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                    context.OutputSource(
                          outputSourceGenFiles
                        , type.openingSource
                        , WriteCode(type)
                        , type.closingSource
                        , hintName
                        , filePath
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
                        , Location.None
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new(
                  "SG_RUNTIME_TYPE_CACHES_UNKNOWN_0001"
                , "Runtime Type Cache Generator Error"
                , "This error indicates a bug in the Runtime Type Caches source generators. Error message: '{0}'."
                , "RuntimeTypeCache"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static string WriteCode(in PartialTypeSpec type)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(type.isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(type.isRecord, "record ")
                    .PrintIf(type.isStruct, "struct ", "class ")
                    .Print(type.containingTypeIdentifier)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintBeginLine("[ETTC.");

                    switch (type.cacheAttributeType)
                    {
                        case CacheAttributeType.CacheType:
                            p.Print(nameof(CacheAttributeType.CacheType));
                            break;

                        case CacheAttributeType.CacheTypesDerivedFrom:
                            p.Print(nameof(CacheAttributeType.CacheTypesDerivedFrom));
                            break;

                        case CacheAttributeType.CacheTypesWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheTypesWithAttribute));
                            break;

                        case CacheAttributeType.CacheFieldsWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheFieldsWithAttribute));
                            break;

                        case CacheAttributeType.CacheMethodsWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheMethodsWithAttribute));
                            break;
                    }

                    p.Print("(typeof(").Print(type.typeFullName).Print(")");

                    if (string.IsNullOrEmpty(type.assemblyName) == false)
                    {
                        p.Print(", \"").Print(type.assemblyName).Print("\"");
                    }

                    p.PrintEndLine(")]");

                    p.PrintBeginLine("partial struct ")
                        .Print(type.containingTypeIdentifier)
                        .Print("_RuntimeTypeCaches_")
                        .Print(type.syntaxTreeStableHash)
                        .PrintEndLine(" { }");
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private static string WriteCode(in TypeSpec type)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(type.isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(type.isRecord, "record ")
                    .PrintIf(type.isStruct, "struct ", "class ")
                    .Print(type.containingTypeIdentifier)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Provides information about the types, fields and methods to be cached.");
                    p.PrintLine("/// </summary>");
                    p.PrintLine(GENERATED_RUNTIME_TYPE_CACHES)
                        .PrintLine(GENERATED_CODE)
                        .PrintLine(EXCLUDE_COVERAGE)
                        .PrintLine(EDITOR_BROWSABLE_NEVER)
                        .PrintLine(PRESERVE);

                    p.PrintBeginLine("private partial struct ")
                        .Print(type.containingTypeIdentifier)
                        .Print("_RuntimeTypeCaches_")
                        .Print(type.syntaxTreeStableHash)
                        .PrintEndLine(" { }");
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        internal enum CacheAttributeType
        {
            None,
            CacheType,
            CacheTypesDerivedFrom,
            CacheTypesWithAttribute,
            CacheFieldsWithAttribute,
            CacheMethodsWithAttribute,
        }

        private struct PartialTypeSpec : IEquatable<PartialTypeSpec>
        {
            public string containingTypeFullName;
            public string containingTypeFileName;
            public string containingTypeIdentifier;
            public string typeFullName;
            public string assemblyName;
            public string openingSource;
            public string closingSource;
            public int syntaxTreeStableHash;
            public int syntaxLineNumber;
            public CacheAttributeType cacheAttributeType;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            public readonly bool IsValid => containingTypeFullName is not null;

            public readonly bool Equals(PartialTypeSpec other)
            {
                return string.Equals(containingTypeFullName, other.containingTypeFullName, StringComparison.Ordinal)
                    && string.Equals(containingTypeFileName, other.containingTypeFileName, StringComparison.Ordinal)
                    && string.Equals(containingTypeIdentifier, other.containingTypeIdentifier, StringComparison.Ordinal)
                    && string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
                    && string.Equals(assemblyName, other.assemblyName, StringComparison.Ordinal)
                    && cacheAttributeType == other.cacheAttributeType
                    && isStruct == other.isStruct
                    && isRefStruct == other.isRefStruct
                    && isRecord == other.isRecord
                    ;
            }

            public readonly override bool Equals(object obj)
                => obj is PartialTypeSpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      containingTypeFullName
                    , containingTypeFileName
                    , containingTypeIdentifier
                    , typeFullName
                    , assemblyName
                    , cacheAttributeType
                    , isStruct
                )
                .Add(isRefStruct)
                .Add(isRecord)
                ;
        }

        private struct TypeSpec : IEquatable<TypeSpec>
        {
            public string containingTypeFullName;
            public string containingTypeFileName;
            public string containingTypeIdentifier;
            public string openingSource;
            public string closingSource;
            public int syntaxTreeStableHash;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            public static TypeSpec From(PartialTypeSpec c) => new() {
                containingTypeFullName = c.containingTypeFullName,
                containingTypeFileName = c.containingTypeFileName,
                containingTypeIdentifier = c.containingTypeIdentifier,
                openingSource = c.openingSource,
                closingSource = c.closingSource,
                syntaxTreeStableHash = c.syntaxTreeStableHash,
                isStruct = c.isStruct,
                isRefStruct = c.isRefStruct,
                isRecord = c.isRecord,
            };

            public readonly bool Equals(TypeSpec other)
            {
                return string.Equals(containingTypeFullName, other.containingTypeFullName, StringComparison.Ordinal)
                    && string.Equals(containingTypeFileName, other.containingTypeFileName, StringComparison.Ordinal)
                    && isStruct == other.isStruct
                    && isRefStruct == other.isRefStruct
                    && isRecord == other.isRecord
                    ;
            }

            public readonly override bool Equals(object obj)
                => obj is TypeSpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      containingTypeFullName
                    , containingTypeFileName
                    , isStruct
                    , isRefStruct
                    , isRecord
                );
        }
    }
}

