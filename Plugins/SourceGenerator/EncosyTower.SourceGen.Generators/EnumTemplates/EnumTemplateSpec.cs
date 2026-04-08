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
    internal struct EnumTemplateSpec : IEquatable<EnumTemplateSpec>
    {
        private const string MEMBERS_FROM_ENUM_ATTRIBUTE = "global::EncosyTower.EnumExtensions.EnumTemplateMembersFromEnumAttribute";
        private const string MEMBER_FROM_TYPE_ATTRIBUTE = "global::EncosyTower.EnumExtensions.EnumTemplateMemberFromTypeAttribute";

        /// <summary>
        /// Location of the template struct declaration. Intentionally excluded from
        /// <see cref="Equals(EnumTemplateSpec)"/> and <see cref="GetHashCode"/>:
        /// location data is not stable across incremental runs and must not drive
        /// cache invalidation.
        /// </summary>
        public LocationInfo location;

        /// <summary>
        /// Pre-computed namespace / containing-type scope opener, produced by
        /// <c>TypeCreationHelpers.GenerateOpeningAndClosingSource</c> in the
        /// incremental transform phase. Excluded from <see cref="Equals(EnumTemplateSpec)"/>
        /// and <see cref="GetHashCode"/> for the same reason as <see cref="location"/>.
        /// </summary>
        public string openingSource;

        /// <summary>
        /// Matching closing braces for <see cref="openingSource"/>. Excluded from
        /// equality / hash for the same reason as <see cref="location"/>.
        /// </summary>
        public string closingSource;

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
        public EquatableArray<TemplateMemberSpec> inlineMembers;

        public readonly bool IsValid => location.IsValid;

        /// <summary>
        /// Extracts all template metadata from the annotated struct symbol into a fully
        /// populated, cache-friendly <see cref="EnumTemplateSpec"/>.
        /// Called once per struct inside the <c>ForAttributeWithMetadataName</c> transform.
        /// </summary>
        public static EnumTemplateSpec Extract(
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

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var containingTypes = templateSymbol.GetContainingTypes();

            var ns = templateSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;

            var templateFullName = templateSymbol.ToFullName();

            // Extract source-1 inline members from attributes directly on the template struct.
            var attributes = templateSymbol.GetAttributes(MEMBERS_FROM_ENUM_ATTRIBUTE, MEMBER_FROM_TYPE_ATTRIBUTE);

            using var inlineMemberBuilder = ImmutableArrayBuilder<TemplateMemberSpec>.Rent();

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

                var candidate = TemplateMemberSpec.Extract(
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

            return new EnumTemplateSpec {
                location = location,
                openingSource = openingSource,
                closingSource = closingSource,
                templateFullName = templateFullName,
                templateSimpleName = templateSymbol.Name,
                fileHintName = templateSymbol.ToFileName(),
                accessibility = templateSymbol.DeclaredAccessibility,
                parentIsNamespace = syntax.Parent is BaseNamespaceDeclarationSyntax,
                namespaceName = namespaceName,
                containingTypes = containingTypes,
                inlineMembers = new EquatableArray<TemplateMemberSpec>(inlineMemberBuilder.ToImmutable()),
            };
        }

        public readonly bool Equals(EnumTemplateSpec other)
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
            => obj is EnumTemplateSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(templateFullName, templateSimpleName, fileHintName, namespaceName)
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(containingTypes.GetHashCode())
            .Add(inlineMembers.GetHashCode());

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
