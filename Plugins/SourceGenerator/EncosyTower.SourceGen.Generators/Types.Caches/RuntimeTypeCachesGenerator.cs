using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using EncosyTower.SourceGen.Common.Types.Caches;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.Types.Caches
{
    [Generator]
    internal class RuntimeTypeCachesGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Types.Caches";
        public const string NAMESPACE_PREFIX = $"global::{NAMESPACE}";
        public const string SKIP_ATTRIBUTE = $"{NAMESPACE_PREFIX}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(RuntimeTypeCachesGenerator);
        public const string RUNTIME_TYPE_CACHE = "global::EncosyTower.Types.RuntimeTypeCache";

        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Types.Caches.RuntimeTypeCachesGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string GENERATED_RUNTIME_TYPE_CACHES = $"[{NAMESPACE_PREFIX}.SourceGen.GeneratedRuntimeTypeCaches]";
        private const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";
        private const string EDITOR_BROWSABLE_NEVER = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            // Each matched call site is processed independently — no Collect() — so that
            // editing one call site only re-runs that single output node.
            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: GetMatchedType
            ).Where(static c => c.IsValid);

            // Call-site branch: one file per call site, [CacheXxx] attr only.
            context.RegisterSourceOutput(
                  typeProvider.Combine(projectPathProvider)
                , static (sourceProductionContext, source) => {
                    GenerateOutput(
                          sourceProductionContext
                        , source.Left
                        , source.Right.projectPath
                        , source.Right.outputSourceGenFiles
                    );
                }
            );

            // Header branch: one file per containing type, 5 decorating attrs + empty partial struct.
            // Collect() on ContainingTypeKey is acceptable — the struct is small and equatable,
            // so it only re-runs when containing type shape changes, not on call-site edits.
            var typeKeyProvider = typeProvider.Select(static (c, _) => ContainingTypeKey.From(c));

            context.RegisterSourceOutput(
                  typeKeyProvider.Collect().Combine(projectPathProvider)
                , static (sourceProductionContext, source) => {
                    GenerateHeaderOutput(
                          sourceProductionContext
                        , source.Left
                        , source.Right.projectPath
                        , source.Right.outputSourceGenFiles
                    );
                }
            );
        }

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is MemberAccessExpressionSyntax syntax
                && syntax.Expression is IdentifierNameSyntax { Identifier.ValueText: "RuntimeTypeCache" }
                && syntax.Name is GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } member
                && IsMemberSupported(member.Identifier.ValueText)
                && GetContainingType(syntax) is not null;
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

        /// <summary>
        /// Extracts an equatable <see cref="TypeCacheCallSite"/> value from the semantic model.
        /// All ISymbol and SyntaxNode data is converted to primitive/string representations so
        /// the incremental pipeline can correctly compare successive runs and skip unchanged steps.
        /// Invalid call sites (wrong type, forbidden namespace, non-constant assembly name, etc.)
        /// are returned as <c>default</c> and filtered by <see cref="TypeCacheCallSite.IsValid"/>;
        /// the <see cref="RuntimeTypeCachesAnalyzer"/> is responsible for reporting those errors.
        /// </summary>
        private static TypeCacheCallSite GetMatchedType(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE) == false)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var syntax = context.Node as MemberAccessExpressionSyntax;
            var identifier = syntax.Expression as IdentifierNameSyntax;
            var identifierType = semanticModel.GetTypeInfo(identifier, token).Type;

            if (identifierType.HasFullName(RUNTIME_TYPE_CACHE) == false)
            {
                return default;
            }

            var member = syntax.Name as GenericNameSyntax;
            var typeArgList = member.TypeArgumentList;
            var typeInfo = semanticModel.GetTypeInfo(typeArgList.Arguments[0], token);

            // Type parameters are not applicable; the analyzer will report the diagnostic.
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

            // Validate type constraints; the analyzer will report the corresponding diagnostics.
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
            else
            {
                // CacheTypesWithAttribute, CacheFieldsWithAttribute, CacheMethodsWithAttribute
                if (typeFullName.StartsWith(NAMESPACE_PREFIX))
                {
                    return default;
                }
            }

            // Validate the optional assembly-name argument is a compile-time constant.
            // A non-constant argument is silently filtered; the analyzer reports the diagnostic.
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

            var containingSyntax = GetContainingType(syntax);

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
            );

            return new TypeCacheCallSite {
                  compilationAssemblyName = semanticModel.Compilation.Assembly.Name
                , containingTypeFullName = containingType.ToFullName()
                , containingTypeFileName = containingType.ToFileName()
                , containingTypeIdentifier = containingSyntax.Identifier.ValueText
                , isStruct = isStruct
                , isRefStruct = isStruct && containingSyntax.Modifiers.Any(SyntaxKind.RefKeyword)
                , isRecord = containingSyntax.Modifiers.Any(SyntaxKind.RecordKeyword)
                , syntaxTreeStableHash = containingSyntax.SyntaxTree.GetStableHashCode()
                , sourceFileBaseName = Path.GetFileNameWithoutExtension(containingSyntax.SyntaxTree.FilePath)
                , callSiteLineNumber = syntax.GetLineNumber()
                , scopeOpening = scopeOpening
                , scopeClosing = scopeClosing
                , typeFullName = typeFullName
                , cacheAttributeType = cacheAttributeType
                , assemblyName = assemblyName
            };
        }

        private static TypeDeclarationSyntax GetContainingType(SyntaxNode node)
        {
            var parent = node.Parent;

            while (parent is not null && parent is not TypeDeclarationSyntax)
            {
                parent = parent.Parent;
            }

            return parent as TypeDeclarationSyntax;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , TypeCacheCallSite candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;

            var compilationAssemblyName = candidate.compilationAssemblyName;
            var fileTypeName = candidate.containingTypeFileName;
            var hintName = $"{candidate.sourceFileBaseName}__{fileTypeName}_{candidate.syntaxTreeStableHash}_{candidate.callSiteLineNumber}.g.cs";
            var filePathName = $"{candidate.sourceFileBaseName}__{fileTypeName}_{candidate.syntaxTreeStableHash}_{candidate.callSiteLineNumber}.g.cs";
            string filePath;

            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var dir = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{compilationAssemblyName}/";
                Directory.CreateDirectory(dir);
                filePath = $"{dir}{filePathName}";
            }
            else
            {
                filePath = $"Temp/GeneratedCode/{compilationAssemblyName}/{filePathName}";
            }

            try
            {
                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.scopeOpening
                    , WriteCode(candidate)
                    , candidate.scopeClosing
                    , hintName
                    , filePath
                    , Location.None
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
            , ImmutableArray<ContainingTypeKey> keys
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;

            var seen = new HashSet<ContainingTypeKey>();

            foreach (var key in keys)
            {
                if (seen.Add(key) == false)
                {
                    continue;
                }

                context.CancellationToken.ThrowIfCancellationRequested();

                var compilationAssemblyName = key.compilationAssemblyName;
                var fileTypeName = key.containingTypeFileName;
                var hintName = $"{key.sourceFileBaseName}__{fileTypeName}_{key.syntaxTreeStableHash}_header.g.cs";
                var filePathName = $"{key.sourceFileBaseName}__{fileTypeName}_{key.syntaxTreeStableHash}_header.g.cs";
                string filePath;

                if (SourceGenHelpers.CanWriteToProjectPath)
                {
                    var dir = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{compilationAssemblyName}/";
                    Directory.CreateDirectory(dir);
                    filePath = $"{dir}{filePathName}";
                }
                else
                {
                    filePath = $"Temp/GeneratedCode/{compilationAssemblyName}/{filePathName}";
                }

                try
                {
                    context.OutputSource(
                          outputSourceGenFiles
                        , key.scopeOpening
                        , WriteHeaderCode(key)
                        , key.scopeClosing
                        , hintName
                        , filePath
                        , Location.None
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
                  "SG_RUNTIME_TYPE_CACHE_01"
                , "Runtime Type Cache Generator Error"
                , "This error indicates a bug in the Runtime Type Caches source generators. Error message: '{0}'."
                , "RuntimeTypeCache"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static string WriteCode(in TypeCacheCallSite cdd)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(cdd.isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(cdd.isRecord, "record ")
                    .PrintIf(cdd.isStruct, "struct ", "class ")
                    .Print(cdd.containingTypeIdentifier)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintBeginLine("[").Print(NAMESPACE_PREFIX).Print(".");

                    switch (cdd.cacheAttributeType)
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

                    p.Print("(typeof(").Print(cdd.typeFullName).Print(")");

                    if (string.IsNullOrEmpty(cdd.assemblyName) == false)
                    {
                        p.Print(", \"").Print(cdd.assemblyName).Print("\"");
                    }

                    p.PrintEndLine(")]");

                    p.PrintBeginLine("private partial struct ")
                        .Print(cdd.containingTypeIdentifier)
                        .Print("_RuntimeTypeCaches_")
                        .Print(cdd.syntaxTreeStableHash.ToString())
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

        /// <summary>
        /// Represents a single <c>RuntimeTypeCache.GetXxx&lt;T&gt;()</c> call site.
        /// All fields are primitive or string values so the incremental pipeline can correctly
        /// compare successive runs and avoid re-running unchanged downstream steps.
        /// </summary>
        private struct TypeCacheCallSite : IEquatable<TypeCacheCallSite>
        {
            // ── Compilation ───────────────────────────────────────────────────────────────
            public string compilationAssemblyName;

            // ── Equatable data extracted from INamedTypeSymbol ───────────────────────────
            public string containingTypeFullName;
            public string containingTypeFileName;
            public string containingTypeIdentifier;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            // ── Equatable data extracted from TypeDeclarationSyntax / SyntaxTree ─────────
            public int syntaxTreeStableHash;
            public string sourceFileBaseName;
            public string scopeOpening;
            public string scopeClosing;

            // ── Equatable data for this individual call site ─────────────────────────────
            public int callSiteLineNumber;
            public string typeFullName;
            public CacheAttributeType cacheAttributeType;
            public string assemblyName;

            public readonly bool IsValid => containingTypeFullName is not null;

            public readonly bool Equals(TypeCacheCallSite other)
            {
                return compilationAssemblyName == other.compilationAssemblyName
                    && containingTypeFullName == other.containingTypeFullName
                    && containingTypeFileName == other.containingTypeFileName
                    && containingTypeIdentifier == other.containingTypeIdentifier
                    && isStruct == other.isStruct
                    && isRefStruct == other.isRefStruct
                    && isRecord == other.isRecord
                    && syntaxTreeStableHash == other.syntaxTreeStableHash
                    && sourceFileBaseName == other.sourceFileBaseName
                    && scopeOpening == other.scopeOpening
                    && scopeClosing == other.scopeClosing
                    && callSiteLineNumber == other.callSiteLineNumber
                    && typeFullName == other.typeFullName
                    && cacheAttributeType == other.cacheAttributeType
                    && assemblyName == other.assemblyName;
            }

            public readonly override bool Equals(object obj)
                => obj is TypeCacheCallSite other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(compilationAssemblyName)
                    .Add(containingTypeFullName)
                    .Add(containingTypeIdentifier)
                    .Add(sourceFileBaseName)
                    .Add(scopeOpening)
                    .Add(typeFullName)
                    .Add(assemblyName)
                    .Add((int)cacheAttributeType)
                    .Add(syntaxTreeStableHash)
                    .Add(callSiteLineNumber)
                    .Add(isStruct)
                    .Add(isRefStruct)
                    .Add(isRecord);
        }

        /// <summary>
        /// Identifies a containing type by its shape — used to deduplicate header output
        /// across multiple call sites inside the same type.
        /// All fields are primitive or string values for stable incremental pipeline comparison.
        /// </summary>
        private struct ContainingTypeKey : IEquatable<ContainingTypeKey>
        {
            // ── Compilation ───────────────────────────────────────────────────────────────
            public string compilationAssemblyName;

            // ── Equatable data extracted from INamedTypeSymbol ───────────────────────────
            public string containingTypeFullName;
            public string containingTypeFileName;
            public string containingTypeIdentifier;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            // ── Equatable data extracted from TypeDeclarationSyntax / SyntaxTree ─────────
            public int syntaxTreeStableHash;
            public string sourceFileBaseName;
            public string scopeOpening;
            public string scopeClosing;

            public static ContainingTypeKey From(TypeCacheCallSite c) => new() {
                  compilationAssemblyName = c.compilationAssemblyName
                , containingTypeFullName = c.containingTypeFullName
                , containingTypeFileName = c.containingTypeFileName
                , containingTypeIdentifier = c.containingTypeIdentifier
                , isStruct = c.isStruct
                , isRefStruct = c.isRefStruct
                , isRecord = c.isRecord
                , syntaxTreeStableHash = c.syntaxTreeStableHash
                , sourceFileBaseName = c.sourceFileBaseName
                , scopeOpening = c.scopeOpening
                , scopeClosing = c.scopeClosing
            };

            public readonly bool Equals(ContainingTypeKey other)
            {
                return compilationAssemblyName == other.compilationAssemblyName
                    && containingTypeFullName == other.containingTypeFullName
                    && containingTypeFileName == other.containingTypeFileName
                    && containingTypeIdentifier == other.containingTypeIdentifier
                    && isStruct == other.isStruct
                    && isRefStruct == other.isRefStruct
                    && isRecord == other.isRecord
                    && syntaxTreeStableHash == other.syntaxTreeStableHash
                    && sourceFileBaseName == other.sourceFileBaseName
                    && scopeOpening == other.scopeOpening
                    && scopeClosing == other.scopeClosing;
            }

            public readonly override bool Equals(object obj)
                => obj is ContainingTypeKey other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(compilationAssemblyName)
                    .Add(containingTypeFullName)
                    .Add(containingTypeIdentifier)
                    .Add(sourceFileBaseName)
                    .Add(scopeOpening)
                    .Add(syntaxTreeStableHash)
                    .Add(isStruct)
                    .Add(isRefStruct)
                    .Add(isRecord);
        }
    }
}
