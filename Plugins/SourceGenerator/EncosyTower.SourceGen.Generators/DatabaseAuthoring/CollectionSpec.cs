using System;
using EncosyTower.SourceGen.Helpers.Data;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct CollectionSpec : IEquatable<CollectionSpec>
    {
        public CollectionKind kind;
        public string elementTypeName;
        public string elementTypeSimpleName;
        public string keyTypeName;
        public string keyTypeSimpleName;

        public readonly bool Equals(CollectionSpec other)
            => kind == other.kind
            && string.Equals(elementTypeName, other.elementTypeName, StringComparison.Ordinal)
            && string.Equals(elementTypeSimpleName, other.elementTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(keyTypeName, other.keyTypeName, StringComparison.Ordinal)
            && string.Equals(keyTypeSimpleName, other.keyTypeSimpleName, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is CollectionSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(elementTypeName, elementTypeSimpleName, keyTypeName, keyTypeSimpleName)
            .Add((byte)kind);
    }
}
