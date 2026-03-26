using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.ObservableProperties
{
    [Generator]
    public class ObservablePropertyGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(ObservablePropertyGenerator);
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ObservablePropertyDeclaration.OBSERVABLE_OBJECT_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                    , ObservablePropertyDeclaration.Extract
                )
                .Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

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
            , ObservablePropertyDeclaration declaration
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var source = (declaration.fieldRefs.Count > 0 || declaration.propRefs.Count > 0)
                    ? declaration.WriteCode()
                    : declaration.WriteCodeWithoutMember();

                context.OutputSource(
                      outputSourceGenFiles
                    , declaration.openingSource
                    , source
                    , declaration.closingSource
                    , declaration.hintName
                    , declaration.sourceFilePath
                    , declaration.location.ToLocation()
                    , projectPath
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
                    , declaration.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_OBSERVABLE_PROPERTY_01"
                , "Observable Property Generator Error"
                , "This error indicates a bug in the Observable Property source generators. Error message: '{0}'."
                , $"{NAMESPACE}.ObservablePropertyAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
