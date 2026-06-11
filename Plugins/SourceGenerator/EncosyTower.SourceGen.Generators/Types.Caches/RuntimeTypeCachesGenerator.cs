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

            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: GetMatchedType
            ).Where(static c => c.IsValid);

            var combined = typeProvider
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

            var keysProvider = typeProvider.Select(static (x, _) => ContainingTypeKey.From(x))
                .Collect()
                .Combine(projectPathProvider)
                .Combine(compilationProvider);

            context.RegisterSourceOutput(keysProvider, static (sourceProductionContext, source) => {
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

        private static TypeCacheCallSite GetMatchedType(
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

            if (semanticModel.GetDeclaredSymbol(containingSyntax, token) is not INamedTypeSymbol containingType)
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

            return new TypeCacheCallSite {
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
                syntaxTreeStableHash = containingSyntax.SyntaxTree.GetStableHashCode()
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
            , TypeCacheCallSite candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var fileName = candidate.containingTypeFileName;
                var stableHash = candidate.syntaxTreeStableHash;
                var hintName = SourceGenHelpers.BuildHintName(assemblyName, fileName, stableHash, 0);
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , WriteCode(candidate)
                    , candidate.closingSource
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
            , ImmutableArray<ContainingTypeKey> keys
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var seen = new HashSet<ContainingTypeKey>();

            foreach (var key in keys)
            {
                if (seen.Add(key) == false)
                {
                    continue;
                }

                context.CancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var assemblyName = compilation.assemblyName;
                    var fileTypeName = key.containingTypeFileName;
                    var hintName = $"{fileTypeName}_{key.syntaxTreeStableHash}_header.g.cs";
                    var filePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                    context.OutputSource(
                          outputSourceGenFiles
                        , key.openingSource
                        , WriteHeaderCode(key)
                        , key.closingSource
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

        private static string WriteCode(in TypeCacheCallSite callSite)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(callSite.isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(callSite.isRecord, "record ")
                    .PrintIf(callSite.isStruct, "struct ", "class ")
                    .Print(callSite.containingTypeIdentifier)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintBeginLine("[ETTC.");

                    switch (callSite.cacheAttributeType)
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

                    p.Print("(typeof(").Print(callSite.typeFullName).Print(")");

                    if (string.IsNullOrEmpty(callSite.assemblyName) == false)
                    {
                        p.Print(", \"").Print(callSite.assemblyName).Print("\"");
                    }

                    p.PrintEndLine(")]");

                    p.PrintBeginLine("private partial struct ")
                        .Print(callSite.containingTypeIdentifier)
                        .Print("_RuntimeTypeCaches_")
                        .Print(callSite.syntaxTreeStableHash)
                        .PrintEndLine(" { }");
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private static string WriteHeaderCode(in ContainingTypeKey key)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(key.isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(key.isRecord, "record ")
                    .PrintIf(key.isStruct, "struct ", "class ")
                    .Print(key.containingTypeIdentifier)
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
                        .Print(key.containingTypeIdentifier)
                        .Print("_RuntimeTypeCaches_")
                        .Print(key.syntaxTreeStableHash.ToString())
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

        private struct TypeCacheCallSite : IEquatable<TypeCacheCallSite>
        {
            public string containingTypeFullName;
            public string containingTypeFileName;
            public string containingTypeIdentifier;
            public string typeFullName;
            public string assemblyName;
            public string openingSource;
            public string closingSource;
            public int syntaxTreeStableHash;
            public CacheAttributeType cacheAttributeType;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            public readonly bool IsValid => containingTypeFullName is not null;

            public readonly bool Equals(TypeCacheCallSite other)
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
                => obj is TypeCacheCallSite other && Equals(other);

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

        private struct ContainingTypeKey : IEquatable<ContainingTypeKey>
        {
            public string containingTypeFullName;
            public string containingTypeFileName;
            public string containingTypeIdentifier;
            public string assemblyName;
            public string openingSource;
            public string closingSource;
            public int syntaxTreeStableHash;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            public static ContainingTypeKey From(TypeCacheCallSite c) => new() {
                containingTypeFullName = c.containingTypeFullName,
                containingTypeFileName = c.containingTypeFileName,
                containingTypeIdentifier = c.containingTypeIdentifier,
                assemblyName = c.assemblyName,
                openingSource = c.openingSource,
                closingSource = c.closingSource,
                syntaxTreeStableHash = c.syntaxTreeStableHash,
                isStruct = c.isStruct,
                isRefStruct = c.isRefStruct,
                isRecord = c.isRecord,
            };

            public readonly bool Equals(ContainingTypeKey other)
            {
                return string.Equals(containingTypeFullName, other.containingTypeFullName, StringComparison.Ordinal)
                    && string.Equals(containingTypeFileName, other.containingTypeFileName, StringComparison.Ordinal)
                    && string.Equals(containingTypeIdentifier, other.containingTypeIdentifier, StringComparison.Ordinal)
                    && string.Equals(assemblyName, other.assemblyName, StringComparison.Ordinal)
                    && isStruct == other.isStruct
                    && isRefStruct == other.isRefStruct
                    && isRecord == other.isRecord
                    ;
            }

            public readonly override bool Equals(object obj)
                => obj is ContainingTypeKey other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      containingTypeFullName
                    , containingTypeFileName
                    , containingTypeIdentifier
                    , assemblyName
                    , isStruct
                    , isRefStruct
                    , isRecord
                );
        }
    }
}

