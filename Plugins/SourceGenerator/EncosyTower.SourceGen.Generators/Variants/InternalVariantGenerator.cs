using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Variants
{
    [Generator]
    internal class InternalVariantGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Variants";
        private const string NAMESPACE_PREFIX = $"global::{NAMESPACE}";
        private const string SKIP_ATTRIBUTE = $"{NAMESPACE_PREFIX}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string VARIANT_T = $"{NAMESPACE_PREFIX}.Variant<";
        private const string CONVERTER_T = $"{NAMESPACE_PREFIX}.Converters.CachedVariantConverter<";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: GetSemanticMatch
            ).Where(static x => x.IsValid);

            var combined = typeProvider.Collect()
                .Combine(compilationProvider)
                .Combine(projectPathProvider);

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

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is not MemberAccessExpressionSyntax syntax
                || syntax.Expression is not GenericNameSyntax typeSyntax
                || syntax.Name is not IdentifierNameSyntax memberSyntax
                || typeSyntax.TypeArgumentList is not TypeArgumentListSyntax argListSyntax
                || argListSyntax.Arguments.Count != 1
            )
            {
                return false;
            }

            var typeName = typeSyntax.Identifier.ValueText;
            var memberName = memberSyntax.Identifier.ValueText;

            return (typeName is "Variant" && memberName is "GetConverter")
                || (typeName is "CachedVariantConverter" && memberName is "Default");
        }

        private static InternalVariantDeclaration GetSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            // Note: IsValidCompilation check deliberately omitted here.
            // Checking compilation validity requires accessing the full Compilation object inside
            // the transform step, which prevents incremental cache reuse when unrelated files change.
            // Validity is deferred to GenerateOutput via CompilationCandidateSlim.isValid.

            var semanticModel = context.SemanticModel;
            var syntax = context.Node as MemberAccessExpressionSyntax;
            var typeSyntax = syntax.Expression as GenericNameSyntax;
            var typeInfo = semanticModel.GetTypeInfo(typeSyntax, token).Type;

            if (typeInfo is not INamedTypeSymbol type
                || type.TypeArguments.Length != 1
            )
            {
                return default;
            }

            if (type.TypeArguments[0] is not INamedTypeSymbol typeArg
                || typeArg.IsUnboundGenericType
                || (typeArg.IsGenericType && typeArg.TypeParameters.Length != 0)
            )
            {
                return default;
            }

            if (type.HasFullNamePrefix(VARIANT_T) == false
                && type.HasFullNamePrefix(CONVERTER_T) == false
            )
            {
                return default;
            }

            return BuildDeclaration(typeArg, LocationInfo.From(context.Node.GetLocation()), token);
        }

        internal static InternalVariantDeclaration BuildDeclaration(
              INamedTypeSymbol typeArg
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

            var identifier = typeArg.ToValidIdentifier();
            var isValueType = typeArg.IsUnmanagedType;
            int? unmanagedSize = null;

            if (isValueType)
            {
                var size = 0;
                typeArg.GetUnmanagedSize(ref size);
                unmanagedSize = size;
            }

            return new InternalVariantDeclaration {
                location = location,
                fullTypeName = fullTypeName,
                simpleTypeName = typeArg.ToSimpleName(),
                structName = $"Variant__{identifier}",
                converterDefault = $"Variant__{identifier}.Converter.Default",
                fileHintName = typeArg.ToFileName(),
                unmanagedSize = unmanagedSize,
                isValueType = isValueType,
                hasImplicitFromStructToType = isValueType || typeArg.TypeKind != TypeKind.Interface,
                isValid = true,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , ImmutableArray<InternalVariantDeclaration> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidates.Length < 1 || compilation.isValid == false)
            {
                return;
            }

            try
            {
                using var valueTypeBuilder = ImmutableArrayBuilder<InternalVariantDeclaration>.Rent();
                using var refTypeBuilder = ImmutableArrayBuilder<InternalVariantDeclaration>.Rent();
                var seenTypeNames = new HashSet<string>(StringComparer.Ordinal);
                var assemblyName = compilation.assemblyName;

                foreach (var candidate in candidates)
                {
                    if (seenTypeNames.Add(candidate.fullTypeName) == false)
                    {
                        continue;
                    }

                    InternalVariantWriteCode.WriteVariantCode(
                          ref context
                        , in candidate
                        , assemblyName
                        , outputSourceGenFiles
                        , s_errorDescriptor
                        , projectPath
                    );

                    if (candidate.isValueType)
                    {
                        valueTypeBuilder.Add(candidate);
                    }
                    else
                    {
                        refTypeBuilder.Add(candidate);
                    }
                }

                InternalVariantWriteCode.WriteStaticClass(
                      ref context
                    , valueTypeBuilder.ToImmutable()
                    , refTypeBuilder.ToImmutable()
                    , assemblyName
                    , outputSourceGenFiles
                    , s_errorDescriptor
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
                    , null
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_VARIANT_INTERNAL_01"
                , "Internal Variant Generator Error"
                , "This error indicates a bug in the internal variant source generators. Error message: '{0}'."
                , "EncosyTower.Variants.Variant<T>"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
