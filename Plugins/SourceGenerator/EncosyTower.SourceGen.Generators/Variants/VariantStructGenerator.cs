using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Variants
{
    [Generator]
    public class VariantStructGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Variants";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string VARIANT_ATTRIBUTE = $"{NAMESPACE}.VariantAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  VARIANT_ATTRIBUTE
                , static (node, _) => node is StructDeclarationSyntax
                , GetSemanticMatch
            ).Where(static x => x.IsValid);

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

            return BuildDeclaration(structSymbol, typeArg, LocationInfo.From(context.TargetNode.GetLocation()), token);
        }

        internal static VariantDeclaration BuildDeclaration(
              INamedTypeSymbol structSymbol
            , ITypeSymbol typeArg
            , LocationInfo location
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var fullTypeName = typeArg.ToFullName();

            if (fullTypeName.ToUnionType().IsNativeUnionType())
            {
                return default;
            }

            var isValueType = typeArg.IsUnmanagedType;
            int? unmanagedSize = null;

            if (isValueType)
            {
                var size = 0;
                typeArg.GetUnmanagedSize(ref size);
                unmanagedSize = size;
            }

            var structFullName = structSymbol.ToFullName();
            var converterDefault = $"{structFullName}.Converter.Default";
            var fileHintName = structSymbol.ToFileName();

            var structNs = structSymbol.ContainingNamespace;
            var namespaceName = structNs is { IsGlobalNamespace: false }
                ? structNs.ToDisplayString()
                : string.Empty;

            return new VariantDeclaration {
                location = location,
                fullTypeName = fullTypeName,
                typeName = typeArg.ToSimpleName(),
                converterDefault = converterDefault,
                unmanagedSize = unmanagedSize,
                isValueType = isValueType,
                // Struct symbol is never an interface, so implicit conversion from T → struct is always safe.
                hasImplicitFromStructToType = isValueType || typeArg.TypeKind != TypeKind.Interface,
                structName = structSymbol.Name,
                structFullName = structFullName,
                fileHintName = fileHintName,
                namespaceName = namespaceName,
                containingTypes = structSymbol.GetContainingTypes(),
                isValid = true,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
            , VariantDeclaration declaration
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (declaration.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                declaration.WriteVariantCode(ref context, compilation, outputSourceGenFiles, s_errorDescriptor, projectPath);
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
            = new("SG_VARIANT_STRUCT_01"
                , "Variant Struct Generator Error"
                , "This error indicates a bug in the Variant Struct source generators. Error message: '{0}'."
                , "EncosyTower.Variants.VariantAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
