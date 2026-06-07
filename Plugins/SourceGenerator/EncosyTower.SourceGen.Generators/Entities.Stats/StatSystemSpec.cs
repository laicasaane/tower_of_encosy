using System;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatSystemSpec : IEquatable<StatSystemSpec>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string syntaxKeyword;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public LocationInfo location;
        public int maxDataSize;
        public int maxUserDataSize;
        public bool isStatic;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(typeIdentifier) == false
            && string.IsNullOrEmpty(syntaxKeyword) == false
            && maxDataSize > 0
            && maxUserDataSize > 0
            ;

        public readonly override bool Equals(object obj)
            => obj is StatSystemSpec other && Equals(other);

        public readonly bool Equals(StatSystemSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(syntaxKeyword, other.syntaxKeyword, StringComparison.Ordinal)
            && maxDataSize == other.maxDataSize
            && maxUserDataSize == other.maxUserDataSize
            && isStatic == other.isStatic
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , syntaxKeyword
                , maxDataSize
            )
            .Add(maxUserDataSize)
            .Add(isStatic)
            ;
    }
}
