using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct BaseDataSpec : IEquatable<BaseDataSpec>
    {
        public string fullName;
        public string simpleName;
        public string validIdentifier;
        public bool isValueType;
        public EquatableArray<MemberSpec> propRefs;
        public EquatableArray<MemberSpec> fieldRefs;

        public readonly bool Equals(BaseDataSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(validIdentifier, other.validIdentifier, StringComparison.Ordinal)
            && isValueType == other.isValueType
            && propRefs.Equals(other.propRefs)
            && fieldRefs.Equals(other.fieldRefs)
            ;

        public readonly override bool Equals(object obj)
            => obj is BaseDataSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName, simpleName, validIdentifier, isValueType, propRefs, fieldRefs);
    }
}
