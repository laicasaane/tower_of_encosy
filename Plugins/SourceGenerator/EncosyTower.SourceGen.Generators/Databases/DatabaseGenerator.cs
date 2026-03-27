using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    [Generator]
    public class DatabaseGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      "EncosyTower.Databases.DatabaseAttribute"
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , DatabaseModel.Extract
                )
                .Where(static t => t.IsValid);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, DATABASES_NAMESPACE, SKIP_ATTRIBUTE));

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.isValid)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , DatabaseModel model
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;

            try
            {
                var printer = Printer.DefaultLarge;

                {
                    printer.PrintEndLine();
                    printer.Print("#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS").PrintEndLine();
                    printer.Print("#define __ENCOSY_NO_VALIDATION__").PrintEndLine();
                    printer.Print("#else").PrintEndLine();
                    printer.Print("#define __ENCOSY_VALIDATION__").PrintEndLine();
                    printer.Print("#endif").PrintEndLine();
                    printer.PrintEndLine();
                }

                context.OutputSource(
                      outputSourceGenFiles
                    , model.openingSource
                    , model.WriteCode()
                    , model.closingSource
                    , model.hintName
                    , model.sourceFilePath
                    , model.location.ToLocation()
                    , projectPath
                    , printer
                );
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , model.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("DATABASE_UNKNOWN_0001"
                , "Database Generator Error"
                , "This error indicates a bug in the Database source generators. Error message: '{0}'."
                , "DatabaseGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
