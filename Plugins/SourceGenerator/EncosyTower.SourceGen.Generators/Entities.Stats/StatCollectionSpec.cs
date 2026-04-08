using System;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatCollectionSpec : IEquatable<StatCollectionSpec>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string statSystemFullTypeName;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public LocationInfo location;
        public EquatableArray<StatDataSpec> statDataCollection;
        public uint typeIdOffset;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(statSystemFullTypeName) == false
            && string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(sourceFilePath) == false
            && string.IsNullOrEmpty(openingSource) == false
            && string.IsNullOrEmpty(closingSource) == false
            && statDataCollection.IsEmpty == false
            ;

        public readonly bool Equals(StatCollectionSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(statSystemFullTypeName, other.statSystemFullTypeName, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && statDataCollection.Equals(other.statDataCollection)
            && typeIdOffset == other.typeIdOffset
            ;

        public readonly override bool Equals(object obj)
            => obj is StatCollectionSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , statSystemFullTypeName
                , hintName
                , sourceFilePath
                , openingSource
                , closingSource
                , statDataCollection
            )
            .Add(typeIdOffset)
            ;

        internal partial struct StatDataSpec : IEquatable<StatDataSpec>
        {
            public string typeName;
            public string fieldName;
            public string valueTypeNamespace;
            public string valueType;
            public bool singleValue;

            public readonly bool IsValid
                => string.IsNullOrEmpty(typeName) == false
                && string.IsNullOrEmpty(fieldName) == false
                && string.IsNullOrEmpty(valueType) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is StatDataSpec other && Equals(other);

            public readonly bool Equals(StatDataSpec other)
                => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
                && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                && string.Equals(valueTypeNamespace, other.valueTypeNamespace, StringComparison.Ordinal)
                && string.Equals(valueType, other.valueType, StringComparison.Ordinal)
                && singleValue == other.singleValue
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      typeName
                    , fieldName
                    , valueTypeNamespace
                    , valueType
                    , singleValue
                );
        }
    }
}
