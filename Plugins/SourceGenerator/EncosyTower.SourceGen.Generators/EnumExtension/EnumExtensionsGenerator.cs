using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    [Generator]
    public class EnumExtensionsGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string ENUM_EXTENSIONS_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsAttribute";
        private const string ENUM_EXTENSIONS_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumExtensionsAttribute";
        public const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";
        public const string GENERATOR_NAME = nameof(EnumExtensionsGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_EXTENSIONS_ATTRIBUTE_METADATA
                    , static (node, _) => node is EnumDeclarationSyntax
                    , ExtractCandidate
                )
                .Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static EnumExtensionCandidate ExtractCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol enumSymbol)
            {
                return default;
            }

            var syntax = context.TargetNode;
            var location = LocationInfo.From(syntax.GetLocation());

            var containingTypes = enumSymbol.GetContainingTypes();

            var ns = enumSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;

            return EnumExtensionCandidate.Extract(
                  enumSymbol
                , syntax.Parent is BaseNamespaceDeclarationSyntax
                , EnumExtensionsDeclaration.GetNameExtensionsClass(enumSymbol.Name)
                , enumSymbol.DeclaredAccessibility
                , location
                , namespaceName
                , containingTypes
                , token
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , EnumExtensionCandidate candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var declaration = new EnumExtensionsDeclaration(candidate, compilation.references.unityCollections);

                var hintName = $"{GENERATOR_NAME}__{candidate.fileHintName}.g.cs";
                var sourceFilePath = BuildSourceFilePath(compilation.assemblyName, hintName, projectPath);
                var source = declaration.WriteCode();

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , candidate.location.ToLocation()
                        , sourceFilePath
                        , sourceText
                        , projectPath
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                // Generator bugs are silently swallowed — do not emit diagnostics from the
                // generator. User-facing validation is handled by EnumExtensionsAnalyzer.
            }
        }

        private static string BuildSourceFilePath(string assemblyName, string hintName, string projectPath)
        {
            if (projectPath is not null)
            {
                var dir = $"{projectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var dir = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            return $"Temp/GeneratedCode/{assemblyName}/{hintName}";
        }
    }
}
