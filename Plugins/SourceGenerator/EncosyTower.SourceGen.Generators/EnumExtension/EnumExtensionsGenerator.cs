using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        private static EnumExtensionSpec ExtractCandidate(
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

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var containingTypes = enumSymbol.GetContainingTypes();

            var ns = enumSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;

            var candidate = EnumExtensionSpec.Extract(
                  enumSymbol
                , syntax.Parent is BaseNamespaceDeclarationSyntax
                , EnumExtensionsDeclaration.GetNameExtensionsClass(enumSymbol.Name)
                , enumSymbol.DeclaredAccessibility
                , location
                , namespaceName
                , containingTypes
                , token
            );

            candidate.openingSource = openingSource;
            candidate.closingSource = closingSource;

            return candidate;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , EnumExtensionSpec candidate
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
                var sourceFilePath = GeneratorHelpers.BuildSourceFilePath(compilation.assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , declaration.WriteCode()
                    , candidate.closingSource
                    , hintName
                    , sourceFilePath
                    , candidate.location.ToLocation()
                    , projectPath
                );
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

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SC = global::System.Collections;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ETCon = global::EncosyTower.Conversion;");
            p.PrintLine("using ETEE = global::EncosyTower.EnumExtensions;");
            p.PrintLine("using ETEESG = global::EncosyTower.EnumExtensions.SourceGen;");
            p.PrintLine("using UC = global::Unity.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
