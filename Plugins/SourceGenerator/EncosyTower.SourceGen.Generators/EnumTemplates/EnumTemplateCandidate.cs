using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    /// <summary>
    /// Cache-friendly pipeline model for the enum-template source generator.
    /// Holds only primitive values and equatable collections — no <see cref="SyntaxNode"/>
    /// or <see cref="ISymbol"/> references — so that Roslyn's incremental generator engine
    /// can cache and compare instances cheaply across multiple compilations.
    /// </summary>
    internal struct EnumTemplateCandidate : IEquatable<EnumTemplateCandidate>
    {
        private const string MEMBERS_FROM_ENUM_ATTRIBUTE = "global::EncosyTower.EnumExtensions.EnumTemplateMembersFromEnumAttribute";
        private const string MEMBER_FROM_TYPE_ATTRIBUTE = "global::EncosyTower.EnumExtensions.EnumTemplateMemberFromTypeAttribute";

        /// <summary>
        /// Location of the template struct declaration. Intentionally excluded from
        /// <see cref="Equals(EnumTemplateCandidate)"/> and <see cref="GetHashCode"/>:
        /// location data is not stable across incremental runs and must not drive
        /// cache invalidation.
        /// </summary>
        public LocationInfo location;

        /// <summary>Fully qualified name of the template struct.</summary>
        public string templateFullName;

        /// <summary>
        /// Simple identifier text of the template struct,
        /// equivalent to <c>Syntax.Identifier.Text</c> in the old non-cache-friendly design.
        /// </summary>
        public string templateSimpleName;

        /// <summary>Hint name fragment, derived from <c>symbol.ToFileName()</c>.</summary>
        public string fileHintName;

        public Accessibility accessibility;
        public bool parentIsNamespace;
        public string namespaceName;

        /// <summary>
        /// Ordered chain of containing type declarations (outer → inner), each formatted as
        /// <c>"&lt;accessibility&gt; partial &lt;keyword&gt; &lt;Name&gt;"</c>.
        /// Empty when the struct is not nested inside another type.
        /// </summary>
        public EquatableArray<string> containingTypes;

        /// <summary>
        /// Source-1 inline members extracted from attributes directly on the template struct
        /// (<c>[EnumTemplateMembersFromEnum]</c> / <c>[EnumTemplateMemberFromType]</c>).
        /// These are pre-extracted at transform time so they do not need to flow through
        /// <c>Collect()</c> alongside the source-2 external member provider.
        /// </summary>
        public EquatableArray<TemplateMemberCandidate> inlineMembers;

        public readonly bool IsValid => location.IsValid;

        /// <summary>
        /// Extracts all template metadata from the annotated struct symbol into a fully
        /// populated, cache-friendly <see cref="EnumTemplateCandidate"/>.
        /// Called once per struct inside the <c>ForAttributeWithMetadataName</c> transform.
        /// </summary>
        public static EnumTemplateCandidate Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol templateSymbol)
            {
                return default;
            }

            if (templateSymbol.IsUnmanagedType == false || templateSymbol.IsUnboundGenericType)
            {
                return default;
            }

            var syntax = context.TargetNode;
            var location = LocationInfo.From(syntax.GetLocation());

            var containingTypes = templateSymbol.GetContainingTypes();

            var ns = templateSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;

            var templateFullName = templateSymbol.ToFullName();

            // Extract source-1 inline members from attributes directly on the template struct.
            var attributes = templateSymbol.GetAttributes(MEMBERS_FROM_ENUM_ATTRIBUTE, MEMBER_FROM_TYPE_ATTRIBUTE);

            using var inlineMemberBuilder = ImmutableArrayBuilder<TemplateMemberCandidate>.Rent();

            foreach (var attrib in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attrib == null || attrib.ConstructorArguments.Length < 2)
                {
                    continue;
                }

                var typeArg = attrib.ConstructorArguments[0];

                if (typeArg.Kind != TypedConstantKind.Type
                    || typeArg.Value is not INamedTypeSymbol typeSymbol
                )
                {
                    continue;
                }

                var attribLocation = LocationInfo.From(
                    attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                    ?? Location.None
                );

                var candidate = TemplateMemberCandidate.Extract(
                      typeSymbol
                    , templateFullName
                    , attrib
                    , typeSymbol.TypeKind == TypeKind.Enum
                    , attribLocation
                    , token
                );

                if (candidate.IsValid)
                {
                    inlineMemberBuilder.Add(candidate);
                }
            }

            return new EnumTemplateCandidate {
                location = location,
                templateFullName = templateFullName,
                templateSimpleName = templateSymbol.Name,
                fileHintName = templateSymbol.ToFileName(),
                accessibility = templateSymbol.DeclaredAccessibility,
                parentIsNamespace = syntax.Parent is BaseNamespaceDeclarationSyntax,
                namespaceName = namespaceName,
                containingTypes = containingTypes,
                inlineMembers = new EquatableArray<TemplateMemberCandidate>(inlineMemberBuilder.ToImmutable()),
            };
        }

        public readonly bool Equals(EnumTemplateCandidate other)
            => string.Equals(templateFullName, other.templateFullName, StringComparison.Ordinal)
            && string.Equals(templateSimpleName, other.templateSimpleName, StringComparison.Ordinal)
            && string.Equals(fileHintName, other.fileHintName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && accessibility == other.accessibility
            && parentIsNamespace == other.parentIsNamespace
            && containingTypes.Equals(other.containingTypes)
            && inlineMembers.Equals(other.inlineMembers)
            ;

        public readonly override bool Equals(object obj)
            => obj is EnumTemplateCandidate other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(templateFullName, templateSimpleName, fileHintName, namespaceName)
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(containingTypes.GetHashCode())
            .Add(inlineMembers.GetHashCode());
    }
}
