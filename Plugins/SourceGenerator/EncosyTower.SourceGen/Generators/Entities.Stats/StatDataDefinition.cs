using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatDataDefinition : IEquatable<StatDataDefinition>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string valueTypeName;
        public string valueFullTypeName;
        public string underlyingTypeName;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public Location location;
        public int size;
        public bool singleValue;
        public bool isEnum;
        public bool withIndex;

        public readonly bool IsValid
            => size > 0
            && string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(valueTypeName) == false
            && string.IsNullOrEmpty(valueFullTypeName) == false
            && string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(sourceFilePath) == false
            && string.IsNullOrEmpty(openingSource) == false
            && string.IsNullOrEmpty(closingSource) == false
            && location != null
            ;

        public readonly override bool Equals(object obj)
            => obj is StatDataDefinition other && Equals(other);

        public readonly bool Equals(StatDataDefinition other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(valueTypeName, other.valueTypeName, StringComparison.Ordinal)
            && string.Equals(valueFullTypeName, other.valueFullTypeName, StringComparison.Ordinal)
            && string.Equals(underlyingTypeName, other.underlyingTypeName, StringComparison.Ordinal)
            && size == other.size
            && singleValue == other.singleValue
            && isEnum == other.isEnum
            && withIndex == other.withIndex
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                HashValue.Combine(
                      typeName
                    , typeNamespace
                    , valueTypeName
                    , valueFullTypeName
                    , underlyingTypeName
                    , size
                    , singleValue
                    , isEnum
                )
                , withIndex
            );
    }
}
