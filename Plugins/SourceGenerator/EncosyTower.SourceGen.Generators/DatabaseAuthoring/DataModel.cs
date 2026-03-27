using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// Cache-friendly, equatable representation of a data type (<see cref="IData"/>
    /// implementer) that is referenced by a database table.
    /// Replaces the non-cacheable <c>DataDeclaration</c> class.
    /// </summary>
    public partial struct DataModel : IEquatable<DataModel>
    {
        /// <summary>Fully-qualified type name, e.g. <c>global::Foo.Bar.MyData</c>.</summary>
        public string fullName;

        /// <summary>Simple type name, e.g. <c>MyData</c>.</summary>
        public string simpleName;

        /// <summary><c>symbol.ToValidIdentifier()</c> — used to build <c>SetValues_XXX</c> calls.</summary>
        public string validIdentifier;

        /// <summary>Property-backed members declared on this type (includes <c>[DataProperty]</c> and
        /// <c>[GeneratedPropertyFromField]</c> members).</summary>
        public EquatableArray<MemberModel> propRefs;

        /// <summary>Field-backed members declared on this type (includes <c>[SerializeField]</c> and
        /// <c>[GeneratedFieldFromProperty]</c> members).</summary>
        public EquatableArray<MemberModel> fieldRefs;

        /// <summary>
        /// Base-type layers in bottom-up order (outermost first, i.e.
        /// the most-base implementing type first, direct base last).
        /// Replaces the recursive <c>DataDeclaration.BaseTypeRefs</c> list.
        /// </summary>
        public EquatableArray<DataModelLayer> baseTypeLayers;

        public readonly bool IsValid
            => string.IsNullOrEmpty(fullName) == false;

        public readonly bool Equals(DataModel other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(validIdentifier, other.validIdentifier, StringComparison.Ordinal)
            && propRefs.Equals(other.propRefs)
            && fieldRefs.Equals(other.fieldRefs)
            && baseTypeLayers.Equals(other.baseTypeLayers)
            ;

        public readonly override bool Equals(object obj)
            => obj is DataModel other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName, simpleName, validIdentifier)
            .Add(propRefs.GetHashCode())
            .Add(fieldRefs.GetHashCode())
            .Add(baseTypeLayers.GetHashCode());
    }
}
