using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public partial struct DataSpec : IEquatable<DataSpec>
    {
        public string fullName;
        public string simpleName;
        public string validIdentifier;
        public bool isValueType;
        public EquatableArray<MemberSpec> propRefs;
        public EquatableArray<MemberSpec> fieldRefs;
        public EquatableArray<BaseDataSpec> baseTypeRefs;

        public readonly bool IsValid
            => string.IsNullOrEmpty(fullName) == false;

        public readonly bool Equals(DataSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(validIdentifier, other.validIdentifier, StringComparison.Ordinal)
            && isValueType == other.isValueType
            && propRefs.Equals(other.propRefs)
            && fieldRefs.Equals(other.fieldRefs)
            && baseTypeRefs.Equals(other.baseTypeRefs)
            ;

        public readonly override bool Equals(object obj)
            => obj is DataSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  fullName
                , simpleName
                , validIdentifier
                , isValueType
                , propRefs
                , fieldRefs
                , baseTypeRefs
            );
    }
}
