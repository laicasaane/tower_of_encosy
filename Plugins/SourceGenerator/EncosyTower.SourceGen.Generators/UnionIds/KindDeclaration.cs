using System;
using EncosyTower.SourceGen.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    /// <summary>
    /// Cache-friendly, equatable model for a type decorated with <c>[KindForUnionId]</c>
    /// or declared inline via <c>[UnionIdKind]</c> on an id struct.
    /// All symbol-derived data is pre-computed at transform time; no
    /// <see cref="Microsoft.CodeAnalysis.ISymbol"/> or <see cref="Microsoft.CodeAnalysis.SyntaxNode"/>
    /// references are retained.
    /// </summary>
    public struct KindDeclaration : IEquatable<KindDeclaration>
    {
        /// <summary>Excluded from equality/hash — not stable across incremental runs.</summary>
        public LocationInfo location;

        // ── Identity ─────────────────────────────────────────────────────────────────────

        /// <summary>Fully-qualified name of the kind type (e.g. <c>global::MyNS.ColorKind</c>).</summary>
        public string kindFullName;

        /// <summary>Simple name of the kind type (e.g. <c>ColorKind</c>).</summary>
        public string kindSimpleName;

        /// <summary>
        /// Fully-qualified name of the union-id type this kind belongs to.
        /// Used to correlate kinds with their owning id during code generation.
        /// </summary>
        public string idFullName;

        /// <summary>
        /// When <see cref="equality"/> is nullable, the fully-qualified name of the inner
        /// <c>T</c> from <c>Nullable&lt;T&gt;</c>. Otherwise empty.
        /// </summary>
        public string kindFullNameFromNullable;

        // ── Enum-specific data ────────────────────────────────────────────────────────────

        /// <summary><see langword="true"/> when the kind type is an enum.</summary>
        public bool isEnum;

        public bool kindEnumHasFlags;

        /// <summary>Underlying type name of the enum (e.g. <c>int</c>).</summary>
        public string kindEnumUnderlyingTypeName;

        public bool kindEnumIsDisplayAttributeUsed;

        /// <summary>
        /// Members of the enum kind type.  Used to construct nested
        /// <see cref="EnumExtensionsDeclaration"/> without needing the symbol at code-gen time.
        /// </summary>
        public EquatableArray<EnumMemberDeclaration> kindEnumValues;

        /// <summary>Max byte count across all enum member names/display names.</summary>
        public int kindEnumFixedStringBytes;

        /// <summary><see langword="true"/> when the kind symbol already has <c>[EnumExtensions]</c>.</summary>
        public bool hasExternalEnumExtensions;

        /// <summary>Pre-computed <c>"{kindFullName}Extensions"</c> when <see cref="hasExternalEnumExtensions"/>.</summary>
        public string externalEnumExtensionsFullName;

        // ── General precomputed data ──────────────────────────────────────────────────────

        /// <summary><see langword="true"/> when the kind type itself also has <c>[UnionId]</c>.</summary>
        public bool isKindAlsoUnionId;

        /// <summary>Unmanaged size in bytes of the kind type.</summary>
        public int kindUnmanagedSize;

        /// <summary>Pre-computed TryParse(ReadOnlySpan&lt;char&gt;, out T) availability.</summary>
        public MemberExistence tryParseSpan;

        /// <summary>Pre-computed equality strategy for this kind type.</summary>
        public Equality equality;

        /// <summary><see langword="true"/> when the kind type has a public <c>ToDisplayString()</c> method.</summary>
        public bool hasToDisplayString;

        /// <summary><see langword="true"/> when the kind type has a public <c>ToFixedString()</c> method returning a <c>FixedStringXxx</c>.</summary>
        public bool hasToFixedString;

        /// <summary>Byte count of the fixed string returned by <c>ToFixedString()</c>.</summary>
        public int toFixedStringBytes;

        /// <summary><see langword="true"/> when the kind type has a public <c>ToDisplayFixedString()</c> returning a <c>FixedStringXxx</c>.</summary>
        public bool hasToDisplayFixedString;

        /// <summary>Byte count of the fixed string returned by <c>ToDisplayFixedString()</c>.</summary>
        public int toDisplayFixedStringBytes;

        // ── Attribute settings ────────────────────────────────────────────────────────────

        public string name;
        public string displayName;
        public ulong order;

        /// <summary>
        /// Base <see cref="ToStringMethods"/> flags from the attribute, plus non-unity-collections
        /// augmentations (e.g. <c>ToDisplayString</c> for enum kinds, <c>All</c> for union-id kinds).
        /// The caller must gate <c>ToFixedString</c>/<c>ToDisplayFixedString</c> on
        /// <c>references.unityCollections</c> at code-gen time.
        /// </summary>
        public ToStringMethods toStringMethods;

        public bool signed;

        // ── IEquatable<KindForUnionIdInfo> ────────────────────────────────────────────────
        // NOTE: `location` is intentionally excluded.

        public readonly bool Equals(KindDeclaration other)
            => string.Equals(kindFullName, other.kindFullName, StringComparison.Ordinal)
            && string.Equals(kindSimpleName, other.kindSimpleName, StringComparison.Ordinal)
            && string.Equals(idFullName, other.idFullName, StringComparison.Ordinal)
            && string.Equals(kindFullNameFromNullable, other.kindFullNameFromNullable, StringComparison.Ordinal)
            && string.Equals(kindEnumUnderlyingTypeName, other.kindEnumUnderlyingTypeName, StringComparison.Ordinal)
            && string.Equals(externalEnumExtensionsFullName, other.externalEnumExtensionsFullName, StringComparison.Ordinal)
            && string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(displayName, other.displayName, StringComparison.Ordinal)
            && kindEnumValues.Equals(other.kindEnumValues)
            && isEnum == other.isEnum
            && kindEnumHasFlags == other.kindEnumHasFlags
            && kindEnumIsDisplayAttributeUsed == other.kindEnumIsDisplayAttributeUsed
            && kindEnumFixedStringBytes == other.kindEnumFixedStringBytes
            && hasExternalEnumExtensions == other.hasExternalEnumExtensions
            && isKindAlsoUnionId == other.isKindAlsoUnionId
            && kindUnmanagedSize == other.kindUnmanagedSize
            && tryParseSpan.Equals(other.tryParseSpan)
            && equality.Equals(other.equality)
            && hasToDisplayString == other.hasToDisplayString
            && hasToFixedString == other.hasToFixedString
            && toFixedStringBytes == other.toFixedStringBytes
            && hasToDisplayFixedString == other.hasToDisplayFixedString
            && toDisplayFixedStringBytes == other.toDisplayFixedStringBytes
            && order == other.order
            && toStringMethods == other.toStringMethods
            && signed == other.signed
            ;

        public readonly override bool Equals(object obj)
            => obj is KindDeclaration other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  kindFullName
                , kindSimpleName
                , idFullName
                , kindFullNameFromNullable
                , name
                , displayName
            )
            .Add(kindEnumValues.GetHashCode())
            .Add(isEnum)
            .Add(kindEnumHasFlags)
            .Add(kindEnumFixedStringBytes)
            .Add(hasExternalEnumExtensions)
            .Add(isKindAlsoUnionId)
            .Add(kindUnmanagedSize)
            .Add(tryParseSpan.GetHashCode())
            .Add(equality.GetHashCode())
            .Add(hasToDisplayString)
            .Add(hasToFixedString)
            .Add(toFixedStringBytes)
            .Add(signed)
            ;
    }
}
