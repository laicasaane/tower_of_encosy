using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    [Generator]
    public class MonoBinderGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(MonoBinderGenerator);
        public const string NAMESPACE      = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string MONO_BINDER_ATTRIBUTE_METADATA =
            "EncosyTower.Mvvm.ViewBinding.Components.MonoBinderAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      MONO_BINDER_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                    , MonoBinderDeclaration.Extract
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
            , MonoBinderDeclaration declaration
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                context.OutputSource(
                      outputSourceGenFiles
                    , declaration.openingSource
                    , declaration.WriteCode()
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
            }
        }
    }
}
