using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    /// <summary>
    /// Cache-friendly, equatable model for a type decorated with <c>[UnionId]</c>.
    /// Holds only primitives, strings, and other equatable value types — no
    /// <see cref="Microsoft.CodeAnalysis.ISymbol"/> or <see cref="Microsoft.CodeAnalysis.SyntaxNode"/>
    /// references that would root the compilation graph and prevent GC.
    /// </summary>
    public struct IdDeclaration : IEquatable<IdDeclaration>
    {
        /// <summary>Excluded from equality/hash — location is not stable across incremental runs.</summary>
        public LocationInfo location;

        /// <summary>Fully-qualified name, e.g. <c>global::MyNS.MyUnionId</c>.</summary>
        public string fullName;

        /// <summary>Simple type name, e.g. <c>MyUnionId</c>.</summary>
        public string simpleName;

        /// <summary>Stable file hint name used for <c>context.AddSource()</c>.</summary>
        public string fileHintName;

        /// <summary>Namespace name; empty string when in the global namespace.</summary>
        public string namespaceName;

        /// <summary>
        /// Outer-to-inner chain of containing type declarations, each formatted as
        /// <c>"&lt;accessibility&gt; partial &lt;keyword&gt; &lt;Name&gt;"</c>.
        /// Empty when the struct is not nested.
        /// </summary>
        public EquatableArray<string> containingTypes;

        /// <summary>Declared accessibility of the struct.</summary>
        public Accessibility accessibility;

        /// <summary>
        /// <see langword="true"/> when the direct parent syntax node is a namespace declaration
        /// (rather than another type), used by <see cref="EnumExtensions.EnumExtensionsDeclaration"/>
        /// when building the <c>IdKind</c> extensions.
        /// </summary>
        public bool parentIsNamespace;

        /// <summary>
        /// Inline kind hints declared via <c>[UnionIdKind(...)]</c> directly on this id struct.
        /// These are combined with the external <c>[KindForUnionId]</c> candidates at code-gen time.
        /// </summary>
        public EquatableArray<KindDeclaration> inlineKinds;

        // ── Attribute settings ───────────────────────────────────────────────────────────

        public UnionIdSize size;
        public string displayNameForId;
        public string displayNameForKind;
        public char separator;
        public UnionIdKindSettings kindSettings;
        public ParsableStructConverterSettings converterSettings;
        public int? fixedStringBytes;

        // ── Validity ─────────────────────────────────────────────────────────────────────

        public readonly bool IsValid
            => string.IsNullOrEmpty(fullName) == false
            && string.IsNullOrEmpty(simpleName) == false;

        // ── IEquatable<UnionIdInfo> ───────────────────────────────────────────────────────
        // NOTE: `location` is intentionally excluded — it is not stable between incremental passes.

        public readonly bool Equals(IdDeclaration other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(fileHintName, other.fileHintName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && string.Equals(displayNameForId, other.displayNameForId, StringComparison.Ordinal)
            && string.Equals(displayNameForKind, other.displayNameForKind, StringComparison.Ordinal)
            && containingTypes.Equals(other.containingTypes)
            && inlineKinds.Equals(other.inlineKinds)
            && accessibility == other.accessibility
            && parentIsNamespace == other.parentIsNamespace
            && size == other.size
            && separator == other.separator
            && kindSettings == other.kindSettings
            && converterSettings == other.converterSettings
            && fixedStringBytes == other.fixedStringBytes
            ;

        public readonly override bool Equals(object obj)
            => obj is IdDeclaration other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  fullName
                , simpleName
                , fileHintName
                , namespaceName
                , displayNameForId
                , displayNameForKind
            )
            .Add(containingTypes.GetHashCode())
            .Add(inlineKinds.GetHashCode())
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(size)
            .Add(separator)
            .Add(kindSettings)
            .Add(converterSettings)
            .Add(fixedStringBytes)
            ;
    }
}
