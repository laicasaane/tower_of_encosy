using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.RelayCommands
{
    [Generator]
    public class RelayCommandGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public const string ATTRIBUTE = "RelayCommand";
        public const string INPUT_NAMESPACE = $"{NAMESPACE}.Input";
        public const string GENERATOR_NAME = nameof(RelayCommandGenerator);

        private const string OBSERVABLE_OBJECT_ATTRIBUTE_METADATA = "EncosyTower.Mvvm.ComponentModel.ObservableObjectAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      OBSERVABLE_OBJECT_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax cls && HasAnyRelayCommandMethod(cls)
                    , RelayCommandDeclaration.Extract
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

        private static bool HasAnyRelayCommandMethod(ClassDeclarationSyntax cls)
        {
            foreach (var member in cls.Members)
            {
                if (member is MethodDeclarationSyntax method
                    && method.HasAttributeCandidate(INPUT_NAMESPACE, ATTRIBUTE))
                {
                    return true;
                }
            }

            return false;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , RelayCommandDeclaration declaration
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

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , declaration.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_RELAY_COMMAND_01"
                , "Relay Command Generator Error"
                , "This error indicates a bug in the RelayCommand source generators. Error message: '{0}'."
                , $"{NAMESPACE}.ComponentModel.ObservableObject"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}

