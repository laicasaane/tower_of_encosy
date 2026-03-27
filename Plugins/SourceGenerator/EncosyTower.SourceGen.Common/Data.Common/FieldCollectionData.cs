using System;

namespace EncosyTower.SourceGen.Common.Data.Common
{
    /// <summary>
    /// Cache-friendly, equatable model for collection type metadata extracted from a field or property.
    /// Replaces <c>DataDeclaration.CollectionRef</c> — stores string type names instead of
    /// <see cref="Microsoft.CodeAnalysis.ITypeSymbol"/> references.
    /// </summary>
    public struct FieldCollectionData : IEquatable<FieldCollectionData>
    {
        public CollectionKind kind;
        public string elementTypeName;
        public string keyTypeName;
        public bool isElementEquatable;
        public bool isKeyEquatable;

        public readonly bool Equals(FieldCollectionData other)
            => kind == other.kind
            && string.Equals(elementTypeName, other.elementTypeName, StringComparison.Ordinal)
            && string.Equals(keyTypeName, other.keyTypeName, StringComparison.Ordinal)
            && isElementEquatable == other.isElementEquatable
            && isKeyEquatable == other.isKeyEquatable;

        public readonly override bool Equals(object obj)
            => obj is FieldCollectionData other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(kind, elementTypeName, keyTypeName, isElementEquatable, isKeyEquatable);
    }
}
