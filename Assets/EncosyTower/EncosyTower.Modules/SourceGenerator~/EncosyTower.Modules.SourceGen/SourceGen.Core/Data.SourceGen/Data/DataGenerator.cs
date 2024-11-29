using System;
using System.Linq;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Data.SourceGen
{
    using static EncosyTower.Modules.Data.SourceGen.Helpers;

    [Generator]
    public class DataGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var dataRefProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsStructOrClassSyntaxMatch,
                transform: GetSemanticMatch
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var combined = dataRefProvider
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.IsValidCompilation(SKIP_ATTRIBUTE));

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

        public static bool IsStructOrClassSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is TypeDeclarationSyntax typeSyntax
                && typeSyntax.Kind() is (SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration)
                && typeSyntax.BaseList != null
                && typeSyntax.BaseList.Types.Count > 0
                && typeSyntax.BaseList.Types.Any(
                    static x => x.Type.IsTypeNameCandidate("EncosyTower.Modules.Data", "IData")
                );
        }

        public static (TypeDeclarationSyntax syntax, INamedTypeSymbol symbol) GetSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not TypeDeclarationSyntax syntax
                || syntax.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
                || syntax.Kind() is not (SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration)
                || syntax.BaseList == null
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol is null)
            {
                return default;
            }

            return (syntax, symbol);
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , (TypeDeclarationSyntax syntax, INamedTypeSymbol symbol) candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            var (syntax, symbol) = candidate;

            if (syntax == null || symbol == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = syntax.SyntaxTree;
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var declaration = new DataDeclaration(context, syntax, symbol, semanticModel);

                if (declaration.IsValid == false)
                {
                    return;
                }

                if (declaration.FieldRefs.Length > 0 || declaration.PropRefs.Length > 0)
                {
                    var assemblyName = compilation.Assembly.Name;

                    OutputSource(
                          context
                        , outputSourceGenFiles
                        , declaration.Syntax
                        , declaration.WriteCode()
                        , syntaxTree.GetGeneratedSourceFileName(DATA_GENERATOR_NAME, declaration.Syntax, declaration.Symbol.ToValidIdentifier())
                        , syntaxTree.GetGeneratedSourceFilePath(assemblyName, DATA_GENERATOR_NAME)
                    );
                }

                if (declaration.Diagnostics.Length > 0)
                {
                    foreach (var diagnostic in declaration.Diagnostics)
                    {
                        context.ReportDiagnostic(diagnostic.ToDiagnostic());
                    }
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

        private static void OutputSource(
              SourceProductionContext context
            , bool outputSourceGenFiles
            , SyntaxNode syntax
            , string source
            , string hintName
            , string sourceFilePath
        )
        {
            var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                  sourceFilePath
                , syntax
                , source
                , context.CancellationToken
            );

            context.AddSource(hintName, outputSource);

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

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("DATA_UNKNOWN_0001"
                , "Data Generator Error"
                , "This error indicates a bug in the Data source generators. Error message: '{0}'."
                , "DataGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

    }
}
