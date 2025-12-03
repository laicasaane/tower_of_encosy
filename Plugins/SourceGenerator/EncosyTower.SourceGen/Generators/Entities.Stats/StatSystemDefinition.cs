using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatSystemDefinition : IEquatable<StatSystemDefinition>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string syntaxKeyword;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public Location location;
        public int maxDataSize;
        public bool isStatic;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(typeIdentifier) == false
            && string.IsNullOrEmpty(syntaxKeyword) == false
            && string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(sourceFilePath) == false
            && string.IsNullOrEmpty(openingSource) == false
            && string.IsNullOrEmpty(closingSource) == false
            && location != null
            ;

        public readonly override bool Equals(object obj)
            => obj is StatSystemDefinition other && Equals(other);

        public readonly bool Equals(StatSystemDefinition other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(syntaxKeyword, other.syntaxKeyword, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && maxDataSize == other.maxDataSize
            && isStatic == other.isStatic
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , syntaxKeyword
                , openingSource
                , closingSource
                , maxDataSize
                , isStatic
            );
    }
}
