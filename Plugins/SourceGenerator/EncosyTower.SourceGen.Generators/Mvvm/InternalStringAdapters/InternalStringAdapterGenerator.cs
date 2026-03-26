using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters
{
    [Generator]
    public class InternalStringAdapterGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = MvvmGeneratorHelpers.NAMESPACE;
        public const string SKIP_ATTRIBUTE = MvvmGeneratorHelpers.SKIP_ATTRIBUTE;

        private const string OBSERVABLE_PROPERTY_ATTRIBUTE_FULL = $"{NAMESPACE}.ComponentModel.ObservablePropertyAttribute";
        private const string NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE_FULL = $"{NAMESPACE}.ComponentModel.NotifyPropertyChangedForAttribute";
        private const string RELAY_COMMAND_ATTRIBUTE_FULL = $"{NAMESPACE}.Input.RelayCommandAttribute";
        private const string BINDING_PROPERTY_ATTRIBUTE_FULL = $"{NAMESPACE}.ViewBinding.BindingPropertyAttribute";
        private const string ADAPTER_ATTRIBUTE_FULL = $"{NAMESPACE}.ViewBinding.AdapterAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            // --- candidate providers ---

            // Fields or properties annotated with [ObservableProperty].
            var obsCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      OBSERVABLE_PROPERTY_ATTRIBUTE_FULL
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetCandidate_ObservableProperty(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Methods annotated with [RelayCommand] that have exactly one parameter.
            var methodRelayCommandCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      RELAY_COMMAND_ATTRIBUTE_FULL
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetCandidate_Method(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Methods annotated with [Binding] that have exactly one parameter.
            var methodBindingCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      BINDING_PROPERTY_ATTRIBUTE_FULL
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetCandidate_Method(ctx, token)
                )
                .Where(static x => x.IsValid);

            // Fields or properties on IObservableObject classes annotated with [NotifyPropertyChangedFor].
            var npcfCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE_FULL
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetCandidate_NotifyPropertyChangedFor(ctx, token)
                )
                .Where(static x => x.IsValid);

            // --- existing adapters (exclusion list) ---
            // Types for which a [Adapter(from, typeof(string))] adapter already exists in user code.
            var existingAdaptersProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ADAPTER_ATTRIBUTE_FULL
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , static (ctx, token) => GetExistingAdapterSourceTypeName(ctx, token)
                )
                .Where(static t => t is not null);

            // Merge all four candidate arrays into one before the final combine,
            // so the RegisterSourceOutput lambda sees a simple flat structure.
            var allCandidatesFlat = obsCandidatesProvider.Collect()
                .Combine(methodRelayCommandCandidatesProvider.Collect())
                .Combine(methodBindingCandidatesProvider.Collect())
                .Combine(npcfCandidatesProvider.Collect())
                .Select(static (data, _) => {
                    using var b = ImmutableArrayBuilder<StringAdapterCandidateInfo>.Rent();
                    foreach (var x in data.Left.Left.Left)  b.Add(x); // [ObservableProperty]
                    foreach (var x in data.Left.Left.Right) b.Add(x); // relay-command method
                    foreach (var x in data.Left.Right)      b.Add(x); // binding method
                    foreach (var x in data.Right)           b.Add(x); // [NotifyPropertyChangedFor]
                    return b.ToImmutable();
                });

            var combined = allCandidatesFlat
                .Combine(existingAdaptersProvider.Collect())
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right         // compilation
                    , source.Left.Left.Left     // merged candidates
                    , source.Left.Left.Right    // existingAdapterTypeNames
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        // -------------------------------------------------------------------------
        // Candidate transforms
        // -------------------------------------------------------------------------

        private static StringAdapterCandidateInfo GetCandidate_ObservableProperty(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE) == false)
            {
                return default;
            }

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

            return MakeCandidate(typeSymbol, context.TargetNode.GetLocation());
        }

        private static StringAdapterCandidateInfo GetCandidate_Method(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE) == false
                || context.TargetSymbol is not IMethodSymbol methodSymbol
                || methodSymbol.Parameters.Length != 1
                || methodSymbol.Parameters[0].Type is not ITypeSymbol typeSymbol
            )
            {
                return default;
            }

            return MakeCandidate(typeSymbol, context.TargetNode.GetLocation());
        }

        private static StringAdapterCandidateInfo GetCandidate_NotifyPropertyChangedFor(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE) == false)
            {
                return default;
            }

            ITypeSymbol typeSymbol;
            INamedTypeSymbol containingType;

            if (context.TargetSymbol is IFieldSymbol fieldSymbol
                && fieldSymbol.ContainingType is INamedTypeSymbol fieldContainingType
            )
            {
                typeSymbol = fieldSymbol.Type;
                containingType = fieldContainingType;
            }
            else if (context.TargetSymbol is IPropertySymbol propertySymbol
                && propertySymbol.ContainingType is INamedTypeSymbol propertyContainingType
            )
            {
                typeSymbol = propertySymbol.Type;
                containingType = propertyContainingType;
            }
            else
            {
                return default;
            }

            // Only generate adapters for members on types that implement IObservableObject.
            foreach (var iface in containingType.AllInterfaces)
            {
                if (iface.Name == "IObservableObject"
                    && iface.ContainingNamespace is { Name: "ComponentModel" } ns1
                    && ns1.ContainingNamespace is { Name: "Mvvm" } ns2
                    && ns2.ContainingNamespace is { Name: "EncosyTower" } ns3
                    && ns3.ContainingNamespace.IsGlobalNamespace
                )
                {
                    return MakeCandidate(typeSymbol, context.TargetNode.GetLocation());
                }
            }

            return default;
        }

        // -------------------------------------------------------------------------
        // Existing adapter transform
        // -------------------------------------------------------------------------

        private static string GetExistingAdapterSourceTypeName(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE) == false
                || context.TargetSymbol is not INamedTypeSymbol declaredSymbol
            )
            {
                return null;
            }

            // Check the type directly implements IAdapter.
            var implementsIAdapter = false;

            foreach (var iface in declaredSymbol.AllInterfaces)
            {
                if (iface.Name == "IAdapter"
                    && iface.ContainingNamespace is { Name: "ViewBinding" } ns1
                    && ns1.ContainingNamespace is { Name: "Mvvm" } ns2
                    && ns2.ContainingNamespace is { Name: "EncosyTower" } ns3
                    && ns3.ContainingNamespace.IsGlobalNamespace
                )
                {
                    implementsIAdapter = true;
                    break;
                }
            }

            if (implementsIAdapter == false)
            {
                return null;
            }

            // Walk the [Adapter(...)] attributes already resolved by ForAttributeWithMetadataName.
            foreach (var attribute in context.Attributes)
            {
                if (attribute.ConstructorArguments.Length != 2)
                {
                    continue;
                }

                if (attribute.ConstructorArguments[1].Value is not ITypeSymbol toArg
                    || toArg.SpecialType != SpecialType.System_String
                )
                {
                    continue;
                }

                if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol fromArg)
                {
                    return fromArg.ToFullName();
                }
            }

            return null;
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private static StringAdapterCandidateInfo MakeCandidate(ITypeSymbol typeSymbol, Location location)
        {
            var ns = typeSymbol.ContainingNamespace;

            return new StringAdapterCandidateInfo {
                location = LocationInfo.From(location),
                fullTypeName = typeSymbol.ToFullName(),
                simpleName = typeSymbol.Name,
                namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty,
            };
        }

        // -------------------------------------------------------------------------
        // Output
        // -------------------------------------------------------------------------

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ImmutableArray<StringAdapterCandidateInfo> candidates
            , ImmutableArray<string> existingAdapterTypeNames
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidates.Length < 1)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;

            var declaration = new InternalStringAdapterDeclaration(candidates, existingAdapterTypeNames);

            declaration.GenerateAdapters(context, compilation, outputSourceGenFiles);
        }
    }
}
