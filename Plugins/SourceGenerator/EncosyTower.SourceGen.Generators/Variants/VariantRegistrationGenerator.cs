using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Variants
{
    [Generator]
    public class VariantRegistrationGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Variants";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string VARIANT_ATTRIBUTE = $"{NAMESPACE}.VariantAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  VARIANT_ATTRIBUTE
                , static (node, _) => node is StructDeclarationSyntax
                , GetSemanticMatch
            ).Where(static x => x.IsValid);

            var combined = candidateProvider.Collect()
                .Combine(compilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                if (source.Left.Right.isValid == false)
                {
                    return;
                }

                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        public static VariantDeclaration GetSemanticMatch(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol structSymbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attributeData = context.Attributes[0];

            if (attributeData.ConstructorArguments.Length < 1
                || attributeData.ConstructorArguments[0].Value is not ITypeSymbol typeArg
            )
            {
                return default;
            }

            return VariantStructGenerator.BuildDeclaration(
                  structSymbol
                , typeArg
                , LocationInfo.From(context.TargetNode.GetLocation())
                , token
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , ImmutableArray<VariantDeclaration> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidates.Length < 1)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var uniqueBuilder = ImmutableArrayBuilder<VariantDeclaration>.Rent();
                using var redundants = ImmutableArrayBuilder<VariantDeclaration>.Rent();
                var seenTypeNames = new HashSet<string>(StringComparer.Ordinal);

                foreach (var candidate in candidates)
                {
                    if (seenTypeNames.Add(candidate.fullTypeName))
                    {
                        uniqueBuilder.Add(candidate);
                    }
                    else
                    {
                        redundants.Add(candidate);
                    }
                }

                VariantDeclarationWriteCode.WriteStaticRegistrationClass(
                      ref context
                    , uniqueBuilder.ToImmutable()
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                    , projectPath
                );

                foreach (var redundant in redundants.ToImmutable())
                {
                    VariantDeclarationWriteCode.WriteRedundantTypeMarker(
                          ref context
                        , in redundant
                        , compilation
                        , outputSourceGenFiles
                        , s_errorDescriptor
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

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , null
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_VARIANT_REG_01"
                , "Variant Registration Generator Error"
                , "This error indicates a bug in the Variant Registration source generators. Error message: '{0}'."
                , "EncosyTower.Variants.VariantAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
