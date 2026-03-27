using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// Stores the member info for a single base-type layer of a data type.
    /// Flat by design — no recursive <see cref="DataModel"/> references — to avoid
    /// value-type recursion issues in C# structs.
    /// </summary>
    public struct DataModelLayer : IEquatable<DataModelLayer>
    {
        public string fullName;
        public string simpleName;
        public string validIdentifier;
        public EquatableArray<MemberModel> propRefs;
        public EquatableArray<MemberModel> fieldRefs;

        public readonly bool Equals(DataModelLayer other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(validIdentifier, other.validIdentifier, StringComparison.Ordinal)
            && propRefs.Equals(other.propRefs)
            && fieldRefs.Equals(other.fieldRefs)
            ;

        public readonly override bool Equals(object obj)
            => obj is DataModelLayer other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName, simpleName, validIdentifier)
            .Add(propRefs.GetHashCode())
            .Add(fieldRefs.GetHashCode());
    }
}
