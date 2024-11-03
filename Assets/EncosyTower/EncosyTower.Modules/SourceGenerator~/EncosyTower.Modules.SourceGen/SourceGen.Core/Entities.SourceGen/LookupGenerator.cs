using System;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Entities.SourceGen
{
    internal abstract class LookupGenerator : IIncrementalGenerator
    {
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.Entities.SkipSourceGenForAssemblyAttribute";

        protected abstract string Interface { get; }

        protected abstract LookupCodeWriter CodeWriter { get; }

        protected abstract DiagnosticDescriptor ErrorDescriptor { get; }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, token) => IsStructSyntaxMatch(node, token),
                transform: (syntaxContext, token) => GeneratorHelpers.GetStructSemanticMatch(syntaxContext, token, Interface)
            ).Where(static t => t is { });

            var combined = candidateProvider
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.IsValidCompilation(SKIP_ATTRIBUTE));

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

        private static bool IsStructSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is StructDeclarationSyntax structSyntax
                && structSyntax.BaseList != null
                && structSyntax.BaseList.Types.Count > 0
                && structSyntax.TypeParameterList == null
                && structSyntax.HasAttributeCandidate("EncosyTower.Modules.Entities", "Lookup");
        }

        protected abstract LookupDeclaration GetDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        );

        private void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , StructDeclarationSyntax candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = candidate.SyntaxTree;
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var declaration = GetDeclaration(context, candidate, semanticModel);

                if (declaration.TypeRefs.Length <= 0)
                {
                    return;
                }

                var assemblyName = compilation.Assembly.Name;
                var generatorName = GetType().Name;

                context.OutputSource(
                      outputSourceGenFiles
                    , declaration.Syntax
                    , CodeWriter.Write(declaration)
                    , syntaxTree.GetGeneratedSourceFileName(generatorName, declaration.Syntax, declaration.Symbol.ToValidIdentifier())
                    , syntaxTree.GetGeneratedSourceFilePath(assemblyName, generatorName)
                );
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      ErrorDescriptor
                    , candidate.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }
    }
}