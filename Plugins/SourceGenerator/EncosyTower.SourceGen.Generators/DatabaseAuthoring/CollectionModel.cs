using System;
using EncosyTower.SourceGen.Common.Data.Common;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct CollectionModel : IEquatable<CollectionModel>
    {
        public CollectionKind kind;
        public string elementTypeName;
        public string elementTypeSimpleName;
        public string keyTypeName;
        public string keyTypeSimpleName;

        public readonly bool Equals(CollectionModel other)
            => kind == other.kind
            && string.Equals(elementTypeName, other.elementTypeName, StringComparison.Ordinal)
            && string.Equals(elementTypeSimpleName, other.elementTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(keyTypeName, other.keyTypeName, StringComparison.Ordinal)
            && string.Equals(keyTypeSimpleName, other.keyTypeSimpleName, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is CollectionModel other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(elementTypeName, elementTypeSimpleName, keyTypeName, keyTypeSimpleName)
            .Add((byte)kind);
    }
}
