using System;

namespace EncosyTower.SourceGen.Generators.Databases
{
    public struct TableSpec : IEquatable<TableSpec>
    {
        public string typeFullName;
        public string typeName;
        public string propertyName;
        public NameCasing nameCasing;
        public bool deduplicateAssetName;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeFullName) == false
            && string.IsNullOrEmpty(propertyName) == false;

        public readonly override bool Equals(object obj)
            => obj is TableSpec other && Equals(other);

        public readonly bool Equals(TableSpec other)
            => string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && nameCasing == other.nameCasing
            && deduplicateAssetName == other.deduplicateAssetName
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(typeFullName, typeName, propertyName, nameCasing, deduplicateAssetName);
    }
}
