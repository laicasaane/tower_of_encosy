using System;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatDataSpec : IEquatable<StatDataSpec>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string valueTypeName;
        public string valueTypeNs;
        public string valueType;
        public string underlyingTypeName;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public LocationInfo location;
        public int size;
        public bool singleValue;
        public bool isEnum;

        public readonly bool IsValid
            => size > 0
            && string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(valueTypeName) == false
            && string.IsNullOrEmpty(valueType) == false
            ;

        public readonly bool HasCustomNs
            => string.IsNullOrEmpty(valueTypeNs) == false;

        public readonly override bool Equals(object obj)
            => obj is StatDataSpec other && Equals(other);

        public readonly bool Equals(StatDataSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(valueTypeName, other.valueTypeName, StringComparison.Ordinal)
            && string.Equals(valueTypeNs, other.valueTypeNs, StringComparison.Ordinal)
            && string.Equals(valueType, other.valueType, StringComparison.Ordinal)
            && string.Equals(underlyingTypeName, other.underlyingTypeName, StringComparison.Ordinal)
            && size == other.size
            && singleValue == other.singleValue
            && isEnum == other.isEnum
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , valueTypeName
                , valueTypeNs
                , valueType
                , underlyingTypeName
            )
            .Add(size)
            .Add(singleValue)
            .Add(isEnum)
            ;
    }
}
