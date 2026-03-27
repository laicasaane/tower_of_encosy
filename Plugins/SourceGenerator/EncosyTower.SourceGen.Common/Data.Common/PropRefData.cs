using System;

namespace EncosyTower.SourceGen.Common.Data.Common
{
    /// <summary>
    /// Cache-friendly, equatable model for a <c>[DataProperty]</c>-decorated property that has a
    /// generated backing field (a <c>PropertyRef</c> in the old DataDeclaration).
    /// All symbol-derived data is precomputed as strings — no <see cref="Microsoft.CodeAnalysis.ISymbol"/>
    /// or <see cref="Microsoft.CodeAnalysis.SyntaxNode"/> references are retained.
    /// </summary>
    public struct PropRefData : IEquatable<PropRefData>
    {
        /// <summary>Excluded from <see cref="Equals(PropRefData)"/> and <see cref="GetHashCode"/>
        /// — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        /// <summary>Name of the property (e.g. "Name").</summary>
        public string propertyName;

        /// <summary>Name of the generated backing field (e.g. "_name").</summary>
        public string fieldName;

        /// <summary>
        /// Declared type name of the generated backing field (resolved collection-aware,
        /// e.g. "global::System.Collections.Generic.List&lt;T&gt;" or "T[]").
        /// </summary>
        public string fieldTypeName;

        /// <summary>Full type name of the property type.</summary>
        public string propertyTypeName;

        /// <summary>Mutable form of the property type name (for collection projection).</summary>
        public string mutablePropertyTypeName;

        /// <summary>Immutable form of the property type name (for collection projection).</summary>
        public string immutablePropertyTypeName;

        /// <summary>True when the mutable and immutable property type names are the same (non-collection).</summary>
        public bool samePropertyType;

        /// <summary>True when FieldType != PropertyType — the initial "mustCast" value.</summary>
        public bool typesAreDifferent;

        /// <summary>True when the property uses an implicit conversion to/from the field type.</summary>
        public bool implicitlyConvertible;

        /// <summary>True when the backing field already exists in user code.</summary>
        public bool fieldIsImplemented;

        /// <summary>True when the property has public accessibility (used in ReadOnlyView struct filtering).</summary>
        public bool isPropertyPublic;

        /// <summary>True when <c>[CreateProperty]</c> is applied (Unity.Properties integration).</summary>
        public bool doesCreateProperty;

        /// <summary>
        /// Optional converter method call string for converting field → property.
        /// </summary>
        public string propertyConverter;

        /// <summary>
        /// Optional converter method call string for converting property → field.
        /// </summary>
        public string fieldConverter;

        /// <summary>
        /// Optional equality comparer method string.
        /// </summary>
        public string fieldEqualityComparer;

        /// <summary>Collection metadata for the backing field type.</summary>
        public FieldCollectionData fieldCollection;

        /// <summary>Equality strategy for the backing field type.</summary>
        public Equality fieldEquality;

        /// <summary>
        /// Full type name used for equality comparisons (nullable-unwrapped, using GetFieldTypeName).
        /// </summary>
        public string fieldTypeDeclNameForEquality;

        /// <summary>Whether the field type (nullable-unwrapped) is a reference type.</summary>
        public bool fieldTypeIsReferenceType;

        /// <summary>Precomputed forwarded field attributes (fullTypeName + syntax string pairs).</summary>
        public EquatableArray<ForwardedFieldAttributeData> forwardedFieldAttributes;

        public readonly bool Equals(PropRefData other)
            => string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
            && string.Equals(fieldTypeName, other.fieldTypeName, StringComparison.Ordinal)
            && string.Equals(propertyTypeName, other.propertyTypeName, StringComparison.Ordinal)
            && string.Equals(mutablePropertyTypeName, other.mutablePropertyTypeName, StringComparison.Ordinal)
            && string.Equals(immutablePropertyTypeName, other.immutablePropertyTypeName, StringComparison.Ordinal)
            && samePropertyType == other.samePropertyType
            && typesAreDifferent == other.typesAreDifferent
            && implicitlyConvertible == other.implicitlyConvertible
            && fieldIsImplemented == other.fieldIsImplemented
            && isPropertyPublic == other.isPropertyPublic
            && doesCreateProperty == other.doesCreateProperty
            && string.Equals(propertyConverter, other.propertyConverter, StringComparison.Ordinal)
            && string.Equals(fieldConverter, other.fieldConverter, StringComparison.Ordinal)
            && string.Equals(fieldEqualityComparer, other.fieldEqualityComparer, StringComparison.Ordinal)
            && fieldCollection.Equals(other.fieldCollection)
            && fieldEquality.Equals(other.fieldEquality)
            && string.Equals(fieldTypeDeclNameForEquality, other.fieldTypeDeclNameForEquality, StringComparison.Ordinal)
            && fieldTypeIsReferenceType == other.fieldTypeIsReferenceType
            && forwardedFieldAttributes.Equals(other.forwardedFieldAttributes);

        public readonly override bool Equals(object obj)
            => obj is PropRefData other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(propertyName);
            hash.Add(fieldName);
            hash.Add(fieldTypeName);
            hash.Add(propertyTypeName);
            hash.Add(mutablePropertyTypeName);
            hash.Add(immutablePropertyTypeName);
            hash.Add(samePropertyType);
            hash.Add(typesAreDifferent);
            hash.Add(implicitlyConvertible);
            hash.Add(fieldIsImplemented);
            hash.Add(isPropertyPublic);
            hash.Add(doesCreateProperty);
            hash.Add(propertyConverter);
            hash.Add(fieldConverter);
            hash.Add(fieldEqualityComparer);
            hash.Add(fieldCollection);
            hash.Add(fieldEquality);
            hash.Add(fieldTypeDeclNameForEquality);
            hash.Add(fieldTypeIsReferenceType);
            hash.Add(forwardedFieldAttributes);
            return hash.ToHashCode();
        }
    }
}
