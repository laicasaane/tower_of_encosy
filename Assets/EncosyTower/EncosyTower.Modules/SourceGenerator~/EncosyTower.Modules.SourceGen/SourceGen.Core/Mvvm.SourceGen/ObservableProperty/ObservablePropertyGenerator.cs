using System;
using System.Linq;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm.ObservablePropertySourceGen
{
    [Generator]
    public class ObservablePropertyGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(ObservablePropertyGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsClassSyntaxMatch,
                transform: GetClassSemanticMatch
            ).Where(static t => t is { });

            var combined = candidateProvider
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.IsValidCompilation(MvvmGeneratorHelpers.SKIP_ATTRIBUTE));

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

        public static bool IsClassSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is ClassDeclarationSyntax classSyntax
                && classSyntax.BaseList != null
                && classSyntax.BaseList.Types.Count > 0
                && classSyntax.BaseList.Types.Any(
                    static x => x.Type.IsTypeNameCandidate("EncosyTower.Modules.Mvvm.ComponentModel", "IObservableObject")
                );
        }

        public static ClassDeclarationSyntax GetClassSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not ClassDeclarationSyntax classSyntax
                || classSyntax.BaseList == null
                || classSyntax.BaseList.Types.Count < 1
            )
            {
                return null;
            }

            return classSyntax;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ClassDeclarationSyntax candidate
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
                var declaration = new ObservablePropertyDeclaration(candidate, semanticModel, context.CancellationToken);

                string source;

                if (declaration.FieldRefs.Length > 0 || declaration.PropRefs.Length > 0)
                {
                    source = declaration.WriteCode();
                }
                else
                {
                    source = declaration.WriteCodeWithoutMember();
                }

                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, GENERATOR_NAME);
                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , candidate
                    , source
                    , context.CancellationToken
                );

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, candidate, declaration.Symbol.ToValidIdentifier())
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , candidate.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , candidate.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_OBSERVABLE_PROPERTY_01"
                , "Observable Property Generator Error"
                , "This error indicates a bug in the Observable Property source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Mvvm.ObservablePropertyAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}