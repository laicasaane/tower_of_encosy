using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    [Generator]
    public sealed class DatabaseGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, DATABASES_NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      "EncosyTower.Databases.DatabaseAttribute"
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , DatabaseSpec.Extract
                )
                .Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.isValid)
                .Combine(projectPathProvider);

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

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , DatabaseSpec model
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var hintName = model.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);
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
                    , sourceFilePath
                    , projectPath
                    , printer
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
                    , model.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_DATABASE_UNKNOWN_0001"
                , "Database Generator Error"
                , "This error indicates a bug in the Database source generators. Error message: '{0}'."
                , "DatabaseGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
