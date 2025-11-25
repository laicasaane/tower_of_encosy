using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    [Generator]
    internal class StatSystemGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatGeneratorAPI.SKIP_ATTRIBUTE;
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";
        private const string GENERATOR_NAME = nameof(StatDataGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidStructSyntax,
                transform: GetSemanticSymbolMatch
            ).Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

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

        private static bool IsValidStructSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is TypeDeclarationSyntax syntax
                && syntax.AttributeLists.Count > 0
                && syntax.HasAttributeCandidate(NAMESPACE, "StatSystem")
                ;
        }

        private static StatSystemDefinition GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not TypeDeclarationSyntax syntax
                || syntax.GetAttribute(NAMESPACE, "StatSystem") is not AttributeSyntax attributeSyntax
                || attributeSyntax.ArgumentList is not AttributeArgumentListSyntax argumentList
                || argumentList.Arguments.Count < 1
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var typeSymbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (typeSymbol is not INamedTypeSymbol symbol)
            {
                return default;
            }

            var attribute = typeSymbol.GetAttribute(STAT_SYSTEM_ATTRIBUTE);

            if (attribute == null
                || attribute.ConstructorArguments.Length < 1
                || attribute.ConstructorArguments[0].Value is not byte maxDataSize
            )
            {
                return default;
            }

            return new StatSystemDefinition {
                typeName = symbol.Name,
                typeNamespace = symbol.ContainingNamespace.ToDisplayString(),
                syntax = syntax,
                typeIdentifier = symbol.ToValidIdentifier(),
                maxDataSize = maxDataSize,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
            , StatSystemDefinition candidate
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
                SourceGenHelpers.ProjectPath = projectPath;

                var syntax = candidate.syntax;
                var syntaxTree = syntax.SyntaxTree;

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , candidate.WriteCode(compilation.references)
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, candidate.typeIdentifier)
                    , syntaxTree.GetGeneratedSourceFilePath(compilation.assemblyName, GENERATOR_NAME)
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
                    , candidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_ENTITIES_STATS_02"
                , "Stat System Generator Error"
                , "This error indicates a bug in the Stat System source generators. Error message: '{0}'."
                , $"{NAMESPACE}.StatSystemAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
