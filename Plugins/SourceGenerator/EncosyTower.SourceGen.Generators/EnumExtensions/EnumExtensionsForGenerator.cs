using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    [Generator]
    public sealed class EnumExtensionsForGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string ENUM_EXTENSIONS_FOR_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsForAttribute";
        private const string ENUM_EXTENSIONS_FOR_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumExtensionsForAttribute";
        public const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";
        public const string GENERATOR_NAME = nameof(EnumExtensionsGenerator);

        private static readonly DiagnosticDescriptor s_errorDescriptor = new(
              id: "SG_ENUM_EXTENSIONS_FOR_UNKNOWN_0001"
            , title: "Enum Extensions For Generator Error"
            , messageFormat: "This error indicates a bug in the Enum Extensions For source generators. Error message: '{0}'."
            , category: "EncosyTower.EnumExtensions"
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
                      ENUM_EXTENSIONS_FOR_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax cls
                        && cls.HasModifier(SyntaxKind.StaticKeyword)
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

            if (context.TargetSymbol is not INamedTypeSymbol classSymbol
                || classSymbol.IsStatic == false
            )
            {
                return default;
            }

            if (context.Attributes.Length < 1)
            {
                return default;
            }

            var attribData = context.Attributes[0];

            if (attribData.ConstructorArguments.Length < 1)
            {
                return default;
            }


            if (attribData.ConstructorArguments[0].Value is not INamedTypeSymbol enumSymbol
                || enumSymbol.TypeKind != TypeKind.Enum
            )
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

            var containingTypes = classSymbol.GetContainingTypes(token);
            var ns = classSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;
            var candidate = EnumExtensionSpec.Extract(
                  enumSymbol
                , syntax.Parent is BaseNamespaceDeclarationSyntax
                , classSymbol.Name
                , classSymbol.DeclaredAccessibility
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
                var declaration = new EnumExtensionsDeclaration(candidate, compilation.references.unityCollections);
                var assemblyName = compilation.assemblyName;
                var hintName = $"{candidate.fileHintName}.g.cs";
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , declaration.WriteCode()
                    , candidate.closingSource
                    , hintName
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
                    , candidate.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
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
            p.PrintLine("using ETCol = global::EncosyTower.Collections;");
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
