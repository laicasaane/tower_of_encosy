using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    [Generator]
    public class DatabaseAuthoringGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseAuthoringGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(AuthoringCompilationInfo.GetCompilation);

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      "EncosyTower.Databases.Authoring.AuthorDatabaseAttribute"
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , DatabaseModel.Extract
                )
                .Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) =>
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right        // AuthoringCompilationInfo
                    , source.Left.Left         // DatabaseModel
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                )
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , AuthoringCompilationInfo compilation
            , DatabaseModel model
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var databaseAuthoring = compilation.databaseAuthoring;
                var bakingSheet = compilation.bakingSheet;
                var assemblyName = compilation.compilation.assemblyName;
                var printer = Printer.DefaultLarge;

                // SheetContainer
                {
                    printer.Clear();
                    printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                    printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                    var containerFilePath = GetSourceFilePath(model.containerHintName, assemblyName);

                    context.OutputSource(
                          outputSourceGenFiles
                        , model.openingSource
                        , model.WriteContainer()
                        , model.closingSource
                        , model.containerHintName
                        , containerFilePath
                        , model.location.ToLocation()
                        , projectPath
                        , printer
                    );
                }

                foreach (var sheet in model.sheets)
                {
                    printer.Clear();
                    printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                    printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                    var sheetFilePath = GetSourceFilePath(sheet.hintName, assemblyName);

                    context.OutputSource(
                          outputSourceGenFiles
                        , model.openingSource
                        , model.WriteSheet(in sheet)
                        , model.closingSource
                        , sheet.hintName
                        , sheetFilePath
                        , model.location.ToLocation()
                        , projectPath
                        , printer
                    );
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , model.location.ToLocation()
                    , ex.ToUnityPrintableString()
                ));
            }
        }

        private static string GetSourceFilePath(string fileName, string assemblyName)
        {
            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var saveToDirectory = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(saveToDirectory);
                return saveToDirectory + fileName;
            }

            return $"Temp/GeneratedCode/{assemblyName}/{fileName}";
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("AUTHOR_DATABASE_UNKNOWN_0001"
                , "Data Authoring Generator Error"
                , "This error indicates a bug in the Data Authoring source generators. Error message: '{0}'."
                , "DataAuthoringGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
