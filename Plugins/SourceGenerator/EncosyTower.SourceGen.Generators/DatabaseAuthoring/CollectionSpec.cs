using System;
using EncosyTower.SourceGen.Helpers.Data;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct CollectionSpec : IEquatable<CollectionSpec>
    {
        public CollectionKind kind;
        public TypeSpec keyType;
        public TypeSpec elementType;

        public readonly bool Equals(CollectionSpec other)
            => kind == other.kind
            && keyType.Equals(other.keyType)
            && elementType.Equals(other.elementType)
            ;

        public readonly override bool Equals(object obj)
            => obj is CollectionSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(kind, keyType, elementType);
    }
}
