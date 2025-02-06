using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Types.Caches.SourceGen
{
    [Generator]
    internal class RuntimeTypeCachesGenerator : IIncrementalGenerator
    {
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.Types.Caches.SkipSourceGenForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(RuntimeTypeCachesGenerator);

        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Types.Caches.SourceGen.RuntimeTypeCachesGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string RUNTIME_TYPE_CACHE = "global::EncosyTower.Modules.Types.RuntimeTypeCache";
        private const string CACHES_NAMESPACE = "global::EncosyTower.Modules.Types.Caches";
        private const string GENERATED_RUNTIME_TYPE_CACHES = "[global::EncosyTower.Modules.Types.Caches.SourceGen.GeneratedRuntimeTypeCaches]";
        private const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";
        private const string EDITOR_BROWSABLE_NEVER = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]";
        private const string METHOD_GET_INFO = "GetInfo";
        private const string METHOD_GET_TYPES_DERIVED_FROM = "GetTypesDerivedFrom";
        private const string METHOD_GET_TYPES_WITH_ATTRIBUTE = "GetTypesWithAttribute";
        private const string METHOD_GET_FIELDS_WITH_ATTRIBUTE = "GetFieldsWithAttribute";
        private const string METHOD_GET_METHODS_WITH_ATTRIBUTE = "GetMethodsWithAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: GetMatchedType
            ).Where(ValidateCandiate);

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

            static bool ValidateCandiate(Candidate candidate)
            {
                return candidate.cacheAttributeType is not CacheAttributeType.None
                    && string.IsNullOrEmpty(candidate.methodName) == false
                    && candidate.containingSyntax is not null
                    && candidate.containingType is not null
                    && candidate.typeSyntax is not null
                    ;
            }
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
                METHOD_GET_INFO => true,
                METHOD_GET_TYPES_DERIVED_FROM => true,
                METHOD_GET_TYPES_WITH_ATTRIBUTE => true,
                METHOD_GET_FIELDS_WITH_ATTRIBUTE => true,
                METHOD_GET_METHODS_WITH_ATTRIBUTE => true,
                _ => false,
            };
        }

        private static Candidate GetMatchedType(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(SKIP_ATTRIBUTE) == false)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var syntax = context.Node as MemberAccessExpressionSyntax;
            var identifier = syntax.Expression as IdentifierNameSyntax;
            var identifierType = semanticModel.GetTypeInfo(identifier, token).Type;

            if (identifierType.ToFullName() is not RUNTIME_TYPE_CACHE)
            {
                return default;
            }

            var member = syntax.Name as GenericNameSyntax;
            var typeArgList = member.TypeArgumentList;
            var containingSyntax = GetContainingType(syntax);

            var containingType = semanticModel.GetDeclaredSymbol(containingSyntax, token);
            var typeInfo = semanticModel.GetTypeInfo(typeArgList.Arguments[0], token);

            var cacheAttributeType = member.Identifier.ValueText switch {
                METHOD_GET_INFO => CacheAttributeType.CacheType,
                METHOD_GET_TYPES_DERIVED_FROM => CacheAttributeType.CacheTypesDerivedFrom,
                METHOD_GET_TYPES_WITH_ATTRIBUTE => CacheAttributeType.CacheTypesWithAttribute,
                METHOD_GET_FIELDS_WITH_ATTRIBUTE => CacheAttributeType.CacheFieldsWithAttribute,
                METHOD_GET_METHODS_WITH_ATTRIBUTE => CacheAttributeType.CacheMethodsWithAttribute,
                _ => CacheAttributeType.None,
            };

            if (cacheAttributeType == CacheAttributeType.None)
            {
                return default;
            }

            var candidate = new Candidate {
                containingSyntax = containingSyntax,
                containingType = containingType,
                typeSyntax = typeArgList.Arguments[0],
                type = typeInfo.Type,
                cacheAttributeType = cacheAttributeType,
                methodName = member.Identifier.ValueText,
            };

            if (syntax.Parent is InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments })
            {
                var constValueOpt = semanticModel.GetConstantValue(arguments[0].Expression, token);

                if (constValueOpt is { HasValue: true, Value: string assemblyName })
                {
                    candidate.assemblyName = string.IsNullOrWhiteSpace(assemblyName) ? string.Empty : assemblyName;
                }
                else
                {
                    candidate.invalidAssemblyNameSyntax = arguments[0].Expression;
                }
            }

            return candidate;
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
            , CompilationCandidate compilationCandidate
            , ImmutableArray<Candidate> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidates.Length < 1)
            {
                return;
            }

            SourceGenHelpers.ProjectPath = projectPath;

            var cacheMap = MakeCacheMap(context, candidates);
            var compilation = compilationCandidate.compilation;
            var assemblyName = compilation.Assembly.Name;

            foreach (var kvp in cacheMap)
            {
                var containingType = kvp.Key;
                var list = kvp.Value;

                if (list.Count < 1)
                {
                    continue;
                }

                var containingSyntax = list[0].containingSyntax;
                var syntaxTree = containingSyntax.SyntaxTree;

                try
                {
                    context.OutputSource(
                          outputSourceGenFiles
                        , containingSyntax
                        , WriteCode(containingSyntax, containingType, list)
                        , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, containingSyntax, containingType.ToValidIdentifier())
                        , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
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
                        , containingSyntax.GetLocation()
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_RUNTIME_TYPE_CACHE_01"
                , "Runtime Type Cache Generator Error"
                , "This error indicates a bug in the Runtime Type Caches source generators. Error message: '{0}'."
                , "RuntimeTypeCache"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static CacheMap MakeCacheMap(
              SourceProductionContext context
            , ImmutableArray<Candidate> candidates
        )
        {
            var map = new CacheMap(SymbolEqualityComparer.Default);

            foreach (var candidate in candidates)
            {
                if (candidate.type is not INamedTypeSymbol type)
                {
                    context.ReportDiagnostic(
                          DiagnosticDescriptors.TypeParameterIsNotApplicable
                        , candidate.typeSyntax
                        , (candidate.typeSyntax as IdentifierNameSyntax)?.Identifier.ValueText ?? "T"
                        , candidate.methodName
                    );
                    continue;
                }

                if (candidate.invalidAssemblyNameSyntax is ExpressionSyntax invalidAssemblyNameSyntax)
                {
                    context.ReportDiagnostic(
                          DiagnosticDescriptors.AssemblyNameMustBeStringLiteralOrConstant
                        , invalidAssemblyNameSyntax
                    );
                    continue;
                }

                if (candidate.cacheAttributeType == CacheAttributeType.CacheType)
                {
                    if (type.IsStatic)
                    {
                        context.ReportDiagnostic(
                              DiagnosticDescriptors.StaticClassIsNotApplicable
                            , candidate.typeSyntax
                            , type.ToFullName()
                        );
                        continue;
                    }
                }
                else if (candidate.cacheAttributeType == CacheAttributeType.CacheTypesDerivedFrom)
                {
                    if (type.TypeKind is not (TypeKind.Class or TypeKind.Interface))
                    {
                        context.ReportDiagnostic(
                              DiagnosticDescriptors.OnlyClassOrInterfaceIsApplicable
                            , candidate.typeSyntax
                            , type.ToFullName()
                        );
                        continue;
                    }

                    if (type.IsStatic)
                    {
                        context.ReportDiagnostic(
                              DiagnosticDescriptors.StaticClassIsNotApplicable
                            , candidate.typeSyntax
                            , type.ToFullName()
                        );
                        continue;
                    }

                    if (type.IsSealed)
                    {
                        context.ReportDiagnostic(
                              DiagnosticDescriptors.SealedClassIsNotApplicable
                            , candidate.typeSyntax
                            , type.ToFullName()
                        );
                        continue;
                    }
                }
                else if (candidate.cacheAttributeType != CacheAttributeType.CacheTypesDerivedFrom)
                {
                    if (type.ToFullName().StartsWith(CACHES_NAMESPACE))
                    {
                        context.ReportDiagnostic(
                              DiagnosticDescriptors.TypesFromCachesAreProhibited
                            , candidate.typeSyntax
                            , type.ToFullName()
                        );
                        continue;
                    }
                }

                if (map.TryGetValue(candidate.containingType, out var list) == false)
                {
                    list = new List<Candidate>();
                    map.Add(candidate.containingType, list);
                }

                list.Add(candidate);
            }

            return map;
        }

        private static string WriteCode(
              TypeDeclarationSyntax containingSyntax
            , INamedTypeSymbol containingType
            , List<Candidate> candidates
        )
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, containingSyntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var isStruct = containingType.TypeKind == TypeKind.Struct;
            var isRefStruct = isStruct && containingSyntax.Modifiers.Any(SyntaxKind.RefKeyword);
            var isRecord = containingSyntax.Modifiers.Any(SyntaxKind.RecordKeyword);

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(isRecord, "record ")
                    .PrintIf(isStruct, "struct ", "class ")
                    .Print(containingSyntax.Identifier.ValueText)
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

                    foreach (var cdd in candidates)
                    {
                        p.PrintBeginLine("[global::EncosyTower.Modules.Types.Caches.");

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

                        p.Print("(typeof(").Print(cdd.type.ToFullName()).Print(")");

                        if (string.IsNullOrEmpty(cdd.assemblyName) == false)
                        {
                            p.Print(", \"").Print(cdd.assemblyName).Print("\"");
                        }

                        p.PrintEndLine(")]");
                    }

                    p.PrintBeginLine("private struct ")
                        .Print(containingSyntax.Identifier.ValueText)
                        .Print("_RuntimeTypeCaches_")
                        .Print(containingSyntax.SyntaxTree.GetStableHashCode().ToString())
                        .PrintEndLine(" { }");
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private enum CacheAttributeType
        {
            None,
            CacheType,
            CacheTypesDerivedFrom,
            CacheTypesWithAttribute,
            CacheFieldsWithAttribute,
            CacheMethodsWithAttribute,
        }

        private struct Candidate
        {
            public TypeDeclarationSyntax containingSyntax;
            public INamedTypeSymbol containingType;
            public TypeSyntax typeSyntax;
            public ITypeSymbol type;
            public string assemblyName;
            public string methodName;
            public ExpressionSyntax invalidAssemblyNameSyntax;
            public CacheAttributeType cacheAttributeType;
        }

        private class CacheMap : Dictionary<INamedTypeSymbol, List<Candidate>>
        {
            public CacheMap(IEqualityComparer<ISymbol> comparer) : base(comparer) { }
        }
    }
}
