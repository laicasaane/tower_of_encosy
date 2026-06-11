using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    [Generator]
    internal sealed class UserDataGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(UserDataGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      USER_DATA_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                        or StructDeclarationSyntax
                        or RecordDeclarationSyntax
                    , UserDataSpec.Extract
                )
                .Where(static x => x.IsValid);

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
            , UserDataSpec spec
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (spec.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var hintName = spec.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , spec.openingSource
                    , spec.WriteCode()
                    , spec.closingSource
                    , spec.hintName
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
                    , spec.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor = new(
              id: "SG_USER_DATA_UNKNOWN_0001"
            , title: "User Data Generator Error"
            , messageFormat: "This error indicates a bug in the User Data source generator. Error message: '{0}'."
            , category: USER_DATA_ATTRIBUTE
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: ""
        );
    }
}
