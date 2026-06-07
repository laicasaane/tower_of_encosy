using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    [Generator]
    public sealed class DatabaseAuthoringGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseAuthoringGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationSpec.GetCompilation);

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      "EncosyTower.Databases.Authoring.AuthorDatabaseAttribute"
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , DatabaseSpec.Extract
                )
                .Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) =>
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right        // AuthoringCompilationSpec
                    , source.Left.Left         // DatabaseSpec
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                )
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , DatabaseSpec model
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var databaseAuthoring = compilation.databaseAuthoring;
                var bakingSheet = compilation.bakingSheet;
                var assemblyName = compilation.compilation.assemblyName;
                var printer = Printer.DefaultLarge;

                // SheetContainer
                {
                    printer.Clear();
                    printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                    printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                    var hintName = model.containerHintName;
                    var filePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                    context.OutputSource(
                          outputSourceGenFiles
                        , model.openingSource
                        , model.WriteContainer()
                        , model.closingSource
                        , model.containerHintName
                        , filePath
                        , projectPath
                        , printer
                    );
                }

                foreach (var sheet in model.sheets)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    printer.Clear();
                    printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                    printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                    var hintName = sheet.hintName;
                    var filePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                    context.OutputSource(
                          outputSourceGenFiles
                        , model.openingSource
                        , model.WriteSheet(in sheet)
                        , model.closingSource
                        , sheet.hintName
                        , filePath
                        , projectPath
                        , printer
                    );
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , model.location.ToLocation()
                    , ex.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_AUTHOR_DATABASE_UNKNOWN_0001"
                , "Data Authoring Generator Error"
                , "This error indicates a bug in the Data Authoring source generators. Error message: '{0}'."
                , "DataAuthoringGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
