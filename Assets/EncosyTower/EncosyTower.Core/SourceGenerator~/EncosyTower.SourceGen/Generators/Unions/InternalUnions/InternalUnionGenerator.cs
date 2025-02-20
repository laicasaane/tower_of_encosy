using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Unions.InternalUnions
{
    [Generator]
    internal class InternalUnionGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Unions";
        private const string NAMESPACE_PREFIX = $"global::{NAMESPACE}";
        private const string SKIP_ATTRIBUTE = $"{NAMESPACE_PREFIX}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string UNION_T = $"{NAMESPACE_PREFIX}.Union<";
        private const string CONVERTER_T = $"{NAMESPACE_PREFIX}.Converters.CachedUnionConverter<";

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

            static bool ValidateCandiate(TypeRef candidate)
            {
                return candidate is not null
                    && candidate.Syntax is not null
                    && candidate.Symbol is not null
                    ;
            }
        }

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is not MemberAccessExpressionSyntax syntax
                || syntax.Expression is not GenericNameSyntax typeSyntax
                || syntax.Name is not IdentifierNameSyntax memberSyntax
                || typeSyntax.TypeArgumentList is not TypeArgumentListSyntax argListSyntax
                || argListSyntax.Arguments.Count != 1
            )
            {
                return false;
            }

            var typeName = typeSyntax.Identifier.ValueText;
            var memberName = memberSyntax.Identifier.ValueText;

            return (typeName is "Union" && memberName is "GetConverter")
                || (typeName is "CachedUnionConverter" && memberName is "Default");
        }

        private static TypeRef GetMatchedType(
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
            var typeSyntax = syntax.Expression as GenericNameSyntax;
            var typeInfo = semanticModel.GetTypeInfo(typeSyntax, token).Type;

            if (typeInfo is not INamedTypeSymbol type
                || type.TypeArguments.Length != 1
            )
            {
                return default;
            }

            if (type.TypeArguments[0] is not INamedTypeSymbol typeArg
                || typeArg.IsUnboundGenericType
                || (typeArg.IsGenericType && typeArg.TypeParameters.Length != 0)
            )
            {
                return default;
            }

            var typeFullName = type.ToFullName();

            if (typeFullName.StartsWith(UNION_T) == false
                && typeFullName.StartsWith(CONVERTER_T) == false
            )
            {
                return default;
            }

            var candidate = new TypeRef {
                Syntax = typeSyntax.TypeArgumentList.Arguments[0],
                Symbol = typeArg,
            };

            return candidate;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , ImmutableArray<TypeRef> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidates.Length < 1)
            {
                return;
            }

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var compilation = compilationCandidate.compilation;

                var declaration = new InternalUnionDeclaration(
                      FilterByTypeArgument(candidates)
                    , ImmutableArray<ITypeSymbol>.Empty
                );

                declaration.GenerateUnionForValueTypes(
                      context
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                );

                declaration.GenerateUnionForRefTypes(
                      context
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                );

                declaration.GenerateStaticClass(
                      context
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                );
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , null
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_INTERNAL_UNIONS_01"
                , "Internal Union Generator Error"
                , "This error indicates a bug in the Internal Union source generators. Error message: '{0}'."
                , "EncosyTower.Unions.Union<T>"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static ImmutableArray<TypeRef> FilterByTypeArgument(ImmutableArray<TypeRef> candidates)
        {
            var unique = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            using var resultBuilder = ImmutableArrayBuilder<TypeRef>.Rent();

            foreach (var candidate in candidates)
            {
                var symbol = candidate.Symbol;

                if (unique.Contains(symbol))
                {
                    continue;
                }

                unique.Add(symbol);
                resultBuilder.Add(candidate);
            }

            return resultBuilder.ToImmutable();
        }
    }
}
