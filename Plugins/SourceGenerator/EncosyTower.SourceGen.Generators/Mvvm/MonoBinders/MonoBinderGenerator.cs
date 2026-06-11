using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    [Generator]
    public sealed class MonoBinderGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(MonoBinderGenerator);
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string MONO_BINDER_ATTRIBUTE_METADATA =
            "EncosyTower.Mvvm.ViewBinding.Components.MonoBinderAttribute";

        private static readonly DiagnosticDescriptor s_errorDescriptor = new(
              id: "SG_MONO_BINDER_UNKNOWN_0001"
            , title: "Mono Binder Generator Error"
            , messageFormat: "This error indicates a bug in the Mono Binder source generators. Error message: '{0}'."
            , category: "EncosyTower.Mvvm"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: ""
        );

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      MONO_BINDER_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                    , MonoBinderSpec.Extract
                )
                .Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

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
            , MonoBinderSpec declaration
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var hintName = declaration.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , declaration.openingSource
                    , declaration.WriteCode()
                    , declaration.closingSource
                    , declaration.hintName
                    , sourceFilePath
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
                    , Location.None
                    , e.ToUnityPrintableString()
                ));
            }
        }
    }
}
