using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalVariants
{
    using InternalVariantDeclaration = Variants.InternalVariantDeclaration;

    [Generator]
    public class InternalVariantGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string OBSERVABLE_PROPERTY_ATTRIBUTE = $"{NAMESPACE}.ComponentModel.ObservablePropertyAttribute";
        private const string NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE = $"{NAMESPACE}.ComponentModel.NotifyPropertyChangedForAttribute";
        private const string RELAY_COMMAND_ATTRIBUTE = $"{NAMESPACE}.Input.RelayCommandAttribute";
        private const string BINDING_PROPERTY_ATTRIBUTE = $"{NAMESPACE}.ViewBinding.BindingPropertyAttribute";
        private const string BINDING_COMMAND_ATTRIBUTE = $"{NAMESPACE}.ViewBinding.BindingCommandAttribute";
        private const string VARIANT_ATTRIBUTE = "EncosyTower.Variants.VariantAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            // Fields or properties annotated with [ObservableProperty].
            var obsProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      OBSERVABLE_PROPERTY_ATTRIBUTE
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetSemanticMatch_ObservableProperty(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Methods annotated with [RelayCommand] that have exactly one parameter.
            var relayCommandProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      RELAY_COMMAND_ATTRIBUTE
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetSemanticMatch_Method(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Methods annotated with [BindingProperty] that have exactly one parameter.
            var bindingPropertyProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      BINDING_PROPERTY_ATTRIBUTE
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetSemanticMatch_Method(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Methods annotated with [BindingCommand] that have exactly one parameter.
            var bindingCommandProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      BINDING_COMMAND_ATTRIBUTE
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetSemanticMatch_Method(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Fields or properties annotated with [NotifyPropertyChangedFor].
            var npcfProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetSemanticMatch_NotifyPropertyChangedFor(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Structs annotated with [Variant] — used to suppress duplicate generation.
            var typeNamesToIgnoreProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      VARIANT_ATTRIBUTE
                    , static (node, _) => node is StructDeclarationSyntax
                    , static (ctx, token) => GetTypeNameFromVariantAttribute(ctx, token)
                )
                .Where(static x => string.IsNullOrEmpty(x) == false);

            // Merge all five candidate providers into one flat array.
            var allCandidatesFlat = obsProvider.Collect()
                .Combine(relayCommandProvider.Collect())
                .Combine(bindingPropertyProvider.Collect())
                .Combine(bindingCommandProvider.Collect())
                .Combine(npcfProvider.Collect())
                .Select(static (data, _) => {
                    using var b = ImmutableArrayBuilder<InternalVariantDeclaration>.Rent();
                    foreach (var x in data.Left.Left.Left.Left)  b.Add(x); // [ObservableProperty]
                    foreach (var x in data.Left.Left.Left.Right) b.Add(x); // [RelayCommand]
                    foreach (var x in data.Left.Left.Right)      b.Add(x); // [BindingProperty]
                    foreach (var x in data.Left.Right)           b.Add(x); // [BindingCommand]
                    foreach (var x in data.Right)                b.Add(x); // [NotifyPropertyChangedFor]
                    return b.ToImmutable();
                });

            var combined = allCandidatesFlat
                .Combine(typeNamesToIgnoreProvider.Collect())
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
                    , source.Left.Left.Left
                    , source.Left.Left.Right
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        // -------------------------------------------------------------------------
        // Candidate transforms
        // -------------------------------------------------------------------------

        private static InternalVariantDeclaration GetSemanticMatch_ObservableProperty(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            // Note: IsValidCompilation check deliberately omitted here.
            // Checking compilation validity inside the transform step prevents incremental cache reuse
            // when unrelated files change. Validity is deferred to GenerateOutput via CompilationCandidateSlim.isValid.

            ITypeSymbol typeSymbol;

            if (context.TargetSymbol is IFieldSymbol fieldSymbol)
            {
                typeSymbol = fieldSymbol.Type;
            }
            else if (context.TargetSymbol is IPropertySymbol propertySymbol)
            {
                typeSymbol = propertySymbol.Type;
            }
            else
            {
                return default;
            }

            return BuildDeclaration(typeSymbol, context.TargetNode.GetLocation(), token);
        }

        private static InternalVariantDeclaration GetSemanticMatch_Method(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not IMethodSymbol methodSymbol
                || methodSymbol.Parameters.Length != 1
            )
            {
                return default;
            }

            return BuildDeclaration(methodSymbol.Parameters[0].Type, context.TargetNode.GetLocation(), token);
        }

        private static InternalVariantDeclaration GetSemanticMatch_NotifyPropertyChangedFor(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            INamedTypeSymbol containingType;

            if (context.TargetSymbol is IFieldSymbol fieldSymbol
                && fieldSymbol.ContainingType is INamedTypeSymbol fieldContainingType
            )
            {
                containingType = fieldContainingType;
            }
            else if (context.TargetSymbol is IPropertySymbol propertySymbol
                && propertySymbol.ContainingType is INamedTypeSymbol propContainingType
            )
            {
                containingType = propContainingType;
            }
            else
            {
                return default;
            }

            // Only process members on types that implement IObservableObject.
            var implementsIObservableObject = false;

            foreach (var iface in containingType.AllInterfaces)
            {
                if (iface.Name == "IObservableObject"
                    && iface.ContainingNamespace is { Name: "ComponentModel" } ns1
                    && ns1.ContainingNamespace is { Name: "Mvvm" } ns2
                    && ns2.ContainingNamespace is { Name: "EncosyTower" } ns3
                    && ns3.ContainingNamespace.IsGlobalNamespace
                )
                {
                    implementsIObservableObject = true;
                    break;
                }
            }

            if (implementsIObservableObject == false)
            {
                return default;
            }

            // Each [NotifyPropertyChangedFor] attribute names the property whose type to observe.
            // Only the first valid attribute is processed.
            foreach (var attribute in context.Attributes)
            {
                if (attribute.ConstructorArguments.Length < 1
                    || attribute.ConstructorArguments[0].Value is not string propertyName
                    || string.IsNullOrEmpty(propertyName)
                )
                {
                    continue;
                }

                foreach (var member in containingType.GetMembers(propertyName))
                {
                    if (member is not IPropertySymbol targetProperty)
                    {
                        continue;
                    }

                    return BuildDeclaration(targetProperty.Type, context.TargetNode.GetLocation(), token);
                }
            }

            return default;
        }

        // -------------------------------------------------------------------------
        // [Variant] exclusion-list transform
        // -------------------------------------------------------------------------

        private static string GetTypeNameFromVariantAttribute(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Attributes.Length < 1)
            {
                return string.Empty;
            }

            var attributeData = context.Attributes[0];

            if (attributeData.ConstructorArguments.Length < 1
                || attributeData.ConstructorArguments[0].Value is not ITypeSymbol typeArg
            )
            {
                return string.Empty;
            }

            return typeArg.ToFullName();
        }

        // -------------------------------------------------------------------------
        // Shared helper
        // -------------------------------------------------------------------------

        private static InternalVariantDeclaration BuildDeclaration(
              ITypeSymbol typeSymbol
            , Location location
            , CancellationToken token
        )
        {
            if (typeSymbol is not INamedTypeSymbol namedType
                || namedType.IsUnboundGenericType
                || (namedType.IsGenericType && namedType.TypeParameters.Length != 0)
            )
            {
                return default;
            }

            return Generators.Variants.InternalVariantGenerator.BuildDeclaration(
                  namedType
                , LocationInfo.From(location)
                , token
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilationCandidate
            , ImmutableArray<InternalVariantDeclaration> candidates
            , ImmutableArray<string> typeNamesToIgnore
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
                // Pre-seed the set with ignored names so a single Add() covers both the ignore-list
                // check and deduplication in one pass.
                var seenTypeNames = new HashSet<string>(typeNamesToIgnore, StringComparer.Ordinal);
                using var valueTypeBuilder = ImmutableArrayBuilder<InternalVariantDeclaration>.Rent();
                using var refTypeBuilder = ImmutableArrayBuilder<InternalVariantDeclaration>.Rent();
                var assemblyName = compilationCandidate.assemblyName;

                foreach (var candidate in candidates)
                {
                    if (seenTypeNames.Add(candidate.fullTypeName) == false)
                    {
                        continue;
                    }

                    MvvmInternalVariantWriteCode.WriteVariantCode(
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

                MvvmInternalVariantWriteCode.WriteStaticClass(
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
            = new("SG_MVVM_INTERNAL_VARIANTS_01"
                , "Mvvm Internal Variant Generator Error"
                , "This error indicates a bug in the Mvvm Internal Variant source generators. Error message: '{0}'."
                , $"{NAMESPACE}.IObservableObject"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}