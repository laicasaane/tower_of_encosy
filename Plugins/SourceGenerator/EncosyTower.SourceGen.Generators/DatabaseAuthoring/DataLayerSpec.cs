using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// Stores the member info for a single base-type layer of a data type.
    /// Flat by design — no recursive <see cref="DataSpec"/> references — to avoid
    /// value-type recursion issues in C# structs.
    /// </summary>
    public struct DataLayerSpec : IEquatable<DataLayerSpec>
    {
        public string fullName;
        public string simpleName;
        public string validIdentifier;
        public EquatableArray<MemberSpec> propRefs;
        public EquatableArray<MemberSpec> fieldRefs;

        public readonly bool Equals(DataLayerSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(validIdentifier, other.validIdentifier, StringComparison.Ordinal)
            && propRefs.Equals(other.propRefs)
            && fieldRefs.Equals(other.fieldRefs)
            ;

        public readonly override bool Equals(object obj)
            => obj is DataLayerSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName, simpleName, validIdentifier)
            .Add(propRefs.GetHashCode())
            .Add(fieldRefs.GetHashCode());
    }
}
