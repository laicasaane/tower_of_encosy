using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.RuntimeTypeCache.SourceGen
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [Generator]
    internal class RuntimeTypeCacheGenerator : IIncrementalGenerator
    {
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.Types.Caches.SkipSourceGenForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(RuntimeTypeCacheGenerator);

        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.RuntimeTypeCache.SourceGen.RuntimeTypeCacheGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string GENERATED_RUNTIME_TYPE_CACHE = "[global::EncosyTower.Modules.Types.Caches.SourceGen.GeneratedRuntimeTypeCache]";
        public const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: GetMatchedType
            ).Where(static t => t.type is { } && t.cacheAttributeType != CacheAttributeType.None);

            var combined = typeProvider.Collect()
                .Combine(compilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is MemberAccessExpressionSyntax syntax
                && syntax.Expression is IdentifierNameSyntax { Identifier: { Text: "RuntimeTypeCache" } }
                && syntax.Name is GenericNameSyntax { TypeArgumentList: { Arguments: { Count: 1 } } } member
                && IsMemberSupported(member.Identifier.ValueText);
        }

        private static bool IsMemberSupported(string memberName)
        {
            return memberName switch {
                "GetTypesDerivedFrom" => true,
                "GetTypesWithAttribute" => true,
                "GetFieldsWithAttribute" => true,
                "GetMethodsWithAttribute" => true,
                _ => false,
            };
        }

        private static bool IsTypeSupported(INamedTypeSymbol type)
        {
            return type.DeclaredAccessibility is Accessibility.Private
                or Accessibility.ProtectedAndInternal
                or Accessibility.Protected
                ;
        }

        private static Candidate GetMatchedType(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(SKIP_ATTRIBUTE) == false
                || context.Node is not MemberAccessExpressionSyntax syntax
                || syntax.Name is not GenericNameSyntax member
                || member.TypeArgumentList is not TypeArgumentListSyntax typeArgList
                || typeArgList.Arguments.Count != 1
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var typeInfo = semanticModel.GetTypeInfo(typeArgList.Arguments[0], token);

            if (typeInfo.Type is not INamedTypeSymbol typeSymbol
                || typeSymbol.TypeKind is not (TypeKind.Class or TypeKind.Interface)
                || IsTypeSupported(typeSymbol) == false
            )
            {
                return default;
            }

            var cacheAttributeType = member.Identifier.ValueText switch {
                "GetTypesDerivedFrom" => CacheAttributeType.CacheTypesDerivedFrom,
                "GetTypesWithAttribute" => CacheAttributeType.CacheTypesWithAttribute,
                "GetFieldsWithAttribute" => CacheAttributeType.CacheFieldsWithAttribute,
                "GetMethodsWithAttribute" => CacheAttributeType.CacheMethodsWithAttribute,
                _ => CacheAttributeType.None,
            };

            if (cacheAttributeType == CacheAttributeType.None)
            {
                return default;
            }

            var candidate = new Candidate {
                type = typeSymbol,
                cacheAttributeType = cacheAttributeType,
            };

            if (syntax.Parent is InvocationExpressionSyntax { ArgumentList: { Arguments: { Count: 1 } arguments } }
                && arguments[0].Expression is LiteralExpressionSyntax literal
            )
            {
                candidate.assemblyName = literal.Token.ValueText;
            }

            return candidate;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , ImmutableArray<Candidate> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (compilationCandidate.compilation == null
                || candidates.Length < 1
            )
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var syntax = CompilationUnit().NormalizeWhitespace(eol: "\n");

            try
            {
                var cacheMap = MakeCacheMap(candidates);

                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = syntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;
                var source = WriteStaticClass(cacheMap, assemblyName);
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME);

                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , syntax
                    , source
                    , context.CancellationToken
                );

                var fileName = $"RuntimeTypeCache_{assemblyName}";

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, fileName, syntax)
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , syntax.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_RUNTIME_TYPE_CACHE_01"
                , "Runtime Type Cache Generator Error"
                , "This error indicates a bug in the Runtime Type Cache source generators. Error message: '{0}'."
                , "RuntimeTypeCache"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static CacheMap MakeCacheMap(ImmutableArray<Candidate> candidates)
        {
            var map = new CacheMap();

            foreach (var candidate in candidates)
            {
                if (candidate.cacheAttributeType == CacheAttributeType.None)
                {
                    continue;
                }

                if (map.TryGetValue(candidate.cacheAttributeType, out var cache) == false)
                {
                    cache = new Dictionary<INamedTypeSymbol, HashSet<string>>(SymbolEqualityComparer.Default);
                    map[candidate.cacheAttributeType] = cache;
                }

                if (cache.TryGetValue(candidate.type, out var assemblies) == false)
                {
                    assemblies = new HashSet<string>();
                    cache[candidate.type] = assemblies;
                }

                if (string.IsNullOrEmpty(candidate.assemblyName) == false)
                {
                    assemblies.Add(candidate.assemblyName);
                }
            }

            return map;
        }

        private static string WriteStaticClass(CacheMap cacheMap, string assemblyName)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace EncosyTower.Modules.Types.__Caches.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Provides information about the types, fields and methods to be cached.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_RUNTIME_TYPE_CACHE).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);

                foreach (var mapKvp in cacheMap)
                {
                    var cacheType = mapKvp.Key;
                    var map = mapKvp.Value;

                    foreach (var kvp in map)
                    {
                        var type = kvp.Key;
                        var assemblies = kvp.Value;

                        p.PrintBeginLine("[global::EncosyTower.Modules.Types.Caches.");

                        switch (cacheType)
                        {
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

                        p.Print("(typeof(").Print(type.ToFullName()).Print(")");

                        foreach (var assembly in assemblies)
                        {
                            p.Print(", \"").Print(assembly).Print("\"");
                        }

                        p.PrintEndLine(")]");
                    }
                }

                p.PrintLine("internal struct RuntimeTypeCaches { }");
            }
            p.CloseScope();

            return p.Result;
        }

        private enum CacheAttributeType
        {
            None,
            CacheTypesDerivedFrom,
            CacheTypesWithAttribute,
            CacheFieldsWithAttribute,
            CacheMethodsWithAttribute,
        }

        private struct Candidate
        {
            public INamedTypeSymbol type;
            public string assemblyName;
            public CacheAttributeType cacheAttributeType;
        }

        private class Cache
        {
            public INamedTypeSymbol type;
            public HashSet<string> assemblyNames;
        }

        private class CacheMap : Dictionary<CacheAttributeType, Dictionary<INamedTypeSymbol, HashSet<string>>>
        {
        }
    }
}
