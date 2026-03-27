using System;

namespace EncosyTower.SourceGen.Common.Data.Common
{
    /// <summary>
    /// Cache-friendly, equatable model for a <c>[SerializeField]</c>-decorated field that backs
    /// a generated property (a <c>FieldRef</c> in the old DataDeclaration).
    /// All symbol-derived data is precomputed as strings — no <see cref="Microsoft.CodeAnalysis.ISymbol"/>
    /// or <see cref="Microsoft.CodeAnalysis.SyntaxNode"/> references are retained.
    /// </summary>
    public struct FieldRefData : IEquatable<FieldRefData>
    {
        /// <summary>Excluded from <see cref="Equals(FieldRefData)"/> and <see cref="GetHashCode"/>
        /// — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        /// <summary>Name of the backing field (e.g. "_name").</summary>
        public string fieldName;

        /// <summary>Name of the generated property (e.g. "Name").</summary>
        public string propertyName;

        /// <summary>Full type name of the backing field as declared (e.g. "global::System.String").</summary>
        public string fieldTypeName;

        /// <summary>Full type name of the property type (may differ when [PropertyType] is used).</summary>
        public string propertyTypeName;

        /// <summary>Mutable form of the property type (for collection projection, e.g. List&lt;T&gt;).</summary>
        public string mutablePropertyTypeName;

        /// <summary>Immutable form of the property type (for collection projection, e.g. ReadOnlyMemory&lt;T&gt;).</summary>
        public string immutablePropertyTypeName;

        /// <summary>True when the mutable and immutable property type names are the same (non-collection or simple types).</summary>
        public bool samePropertyType;

        /// <summary>True when FieldType != PropertyType — the initial "mustCast" value.</summary>
        public bool typesAreDifferent;

        /// <summary>True when the field uses an implicit conversion to/from the property type.</summary>
        public bool implicitlyConvertible;

        /// <summary>Whether the property was already declared by the user (no need to generate it).</summary>
        public bool hasImplementedProperty;

        /// <summary>True when the implemented property has public accessibility (used in ReadOnlyView struct filtering).</summary>
        public bool isImplementedPropertyPublic;

        /// <summary>
        /// Optional converter method call string for converting field → property (e.g. "MyConverter.Convert").
        /// </summary>
        public string propertyConverter;

        /// <summary>
        /// Optional converter method call string for converting property → field.
        /// </summary>
        public string fieldConverter;

        /// <summary>
        /// Optional equality comparer method string (e.g. "MyComparer.Equals").
        /// </summary>
        public string fieldEqualityComparer;

        /// <summary>Collection metadata for the field type.</summary>
        public FieldCollectionData fieldCollection;

        /// <summary>Equality strategy for the field type.</summary>
        public Equality fieldEquality;

        /// <summary>Full type name used for equality comparisons (nullable-unwrapped, using ToFullName()).</summary>
        public string fieldTypeFullNameForEquality;

        /// <summary>Whether the field type (nullable-unwrapped) is a reference type.</summary>
        public bool fieldTypeIsReferenceType;

        /// <summary>Precomputed full type name of the original FieldType symbol, used in attribute annotations.</summary>
        public string fieldTypeOriginalFullName;

        /// <summary>Precomputed forwarded property attributes as syntax strings.</summary>
        public EquatableArray<string> forwardedPropertyAttributeSyntaxes;

        public readonly bool Equals(FieldRefData other)
            => string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && string.Equals(fieldTypeName, other.fieldTypeName, StringComparison.Ordinal)
            && string.Equals(propertyTypeName, other.propertyTypeName, StringComparison.Ordinal)
            && string.Equals(mutablePropertyTypeName, other.mutablePropertyTypeName, StringComparison.Ordinal)
            && string.Equals(immutablePropertyTypeName, other.immutablePropertyTypeName, StringComparison.Ordinal)
            && samePropertyType == other.samePropertyType
            && typesAreDifferent == other.typesAreDifferent
            && implicitlyConvertible == other.implicitlyConvertible
            && hasImplementedProperty == other.hasImplementedProperty
            && isImplementedPropertyPublic == other.isImplementedPropertyPublic
            && string.Equals(propertyConverter, other.propertyConverter, StringComparison.Ordinal)
            && string.Equals(fieldConverter, other.fieldConverter, StringComparison.Ordinal)
            && string.Equals(fieldEqualityComparer, other.fieldEqualityComparer, StringComparison.Ordinal)
            && fieldCollection.Equals(other.fieldCollection)
            && fieldEquality.Equals(other.fieldEquality)
            && string.Equals(fieldTypeFullNameForEquality, other.fieldTypeFullNameForEquality, StringComparison.Ordinal)
            && fieldTypeIsReferenceType == other.fieldTypeIsReferenceType
            && string.Equals(fieldTypeOriginalFullName, other.fieldTypeOriginalFullName, StringComparison.Ordinal)
            && forwardedPropertyAttributeSyntaxes.Equals(other.forwardedPropertyAttributeSyntaxes);

        public readonly override bool Equals(object obj)
            => obj is FieldRefData other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(fieldName);
            hash.Add(propertyName);
            hash.Add(fieldTypeName);
            hash.Add(propertyTypeName);
            hash.Add(mutablePropertyTypeName);
            hash.Add(immutablePropertyTypeName);
            hash.Add(samePropertyType);
            hash.Add(typesAreDifferent);
            hash.Add(implicitlyConvertible);
            hash.Add(hasImplementedProperty);
            hash.Add(isImplementedPropertyPublic);
            hash.Add(propertyConverter);
            hash.Add(fieldConverter);
            hash.Add(fieldEqualityComparer);
            hash.Add(fieldCollection);
            hash.Add(fieldEquality);
            hash.Add(fieldTypeFullNameForEquality);
            hash.Add(fieldTypeIsReferenceType);
            hash.Add(fieldTypeOriginalFullName);
            hash.Add(forwardedPropertyAttributeSyntaxes);
            return hash.ToHashCode();
        }
    }
}
