using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Variants
{
    [Generator]
    public sealed class VariantStructGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Variants";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string VARIANT_ATTRIBUTE = $"{NAMESPACE}.VariantAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

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

        public static VariantSpec GetSemanticMatch(
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

            var decl = BuildDeclaration(structSymbol, typeArg, LocationInfo.From(context.TargetNode.GetLocation()), token);

            if (decl.IsValid)
            {
                TypeCreationHelpers.GenerateOpeningAndClosingSource(
                      context.TargetNode
                    , token
                    , out decl.openingSource
                    , out decl.closingSource
                    , printAdditionalUsings: PrintAdditionalUsings
                );
            }

            return decl;
        }

        internal static VariantSpec BuildDeclaration(
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
                typeArg.GetUnmanagedSize(ref size, token);
                unmanagedSize = size;
            }

            var structFullName = structSymbol.ToFullName();
            var converterDefault = $"{structFullName}.Converter.Default";
            var fileHintName = structSymbol.ToFileName();

            var structNs = structSymbol.ContainingNamespace;
            var namespaceName = structNs is { IsGlobalNamespace: false }
                ? structNs.ToDisplayString()
                : string.Empty;

            return new VariantSpec {
                location = location,
                fullTypeName = fullTypeName,
                typeName = typeArg.ToFullNameNoGlobal(),
                converterDefault = converterDefault,
                unmanagedSize = unmanagedSize,
                isValueType = isValueType,
                hasImplicitFromStructToType = isValueType || typeArg.TypeKind != TypeKind.Interface,
                structName = structSymbol.Name,
                structFullName = structFullName,
                fileHintName = fileHintName,
                namespaceName = namespaceName,
                containingTypes = structSymbol.GetContainingTypes(token),
                isValid = true,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , VariantSpec declaration
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

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ETT = global::EncosyTower.Types;");
            p.PrintLine("using ETV = global::EncosyTower.Variants;");
            p.PrintLine("using ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintLine("using ETVSG = global::EncosyTower.Variants.SourceGen;");
            p.PrintLine("using UE = global::UnityEngine;");
            p.PrintLine("using UES = global::UnityEngine.Scripting;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_VARIANT_STRUCT_UNKNOWN_0001"
                , "Variant Struct Generator Error"
                , "This error indicates a bug in the Variant Struct source generators. Error message: '{0}'."
                , "EncosyTower.Variants.VariantAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
