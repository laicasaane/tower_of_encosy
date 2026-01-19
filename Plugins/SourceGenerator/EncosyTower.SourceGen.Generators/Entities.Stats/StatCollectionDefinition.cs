using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatCollectionDefinition : IEquatable<StatCollectionDefinition>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string statSystemFullTypeName;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public Location location;
        public EquatableArray<StatDataDefinition> statDataCollection;
        public uint typeIdOffset;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(statSystemFullTypeName) == false
            && string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(sourceFilePath) == false
            && string.IsNullOrEmpty(openingSource) == false
            && string.IsNullOrEmpty(closingSource) == false
            && location != null
            && statDataCollection.IsEmpty == false
            ;

        public readonly bool Equals(StatCollectionDefinition other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(statSystemFullTypeName, other.statSystemFullTypeName, StringComparison.Ordinal)
            && statDataCollection.Equals(other.statDataCollection)
            && typeIdOffset == other.typeIdOffset
            ;

        public readonly override bool Equals(object obj)
            => obj is StatCollectionDefinition other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , statSystemFullTypeName
                , statDataCollection
                , typeIdOffset
            );

        internal partial struct StatDataDefinition : IEquatable<StatDataDefinition>
        {
            public string typeName;
            public string fieldName;
            public string valueTypeName;
            public bool singleValue;

            public readonly bool IsValid
                => string.IsNullOrEmpty(typeName) == false
                && string.IsNullOrEmpty(fieldName) == false
                && string.IsNullOrEmpty(valueTypeName) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is StatDataDefinition other && Equals(other);

            public readonly bool Equals(StatDataDefinition other)
                => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
                && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                && string.Equals(valueTypeName, other.valueTypeName, StringComparison.Ordinal)
                && singleValue == other.singleValue
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      typeName
                    , fieldName
                    , valueTypeName
                    , singleValue
                );
        }
    }
}
