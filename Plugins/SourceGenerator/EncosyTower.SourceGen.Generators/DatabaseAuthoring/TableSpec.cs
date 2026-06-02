using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct TableSpec : IEquatable<TableSpec>
    {
        public string typeFullName;
        public string typeSimpleName;
        public string idTypeFullName;
        public string dataTypeFullName;
        public string propertyName;
        public NameCasing nameCasing;
        public bool deduplicateAssetName;

        // Pre-composed names, do not participate in Equals and GetHashCode
        public string baseSheetName;
        public string uniqueSheetName;

        public readonly bool Equals(TableSpec other)
            => string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeSimpleName, other.typeSimpleName, StringComparison.Ordinal)
            && string.Equals(idTypeFullName, other.idTypeFullName, StringComparison.Ordinal)
            && string.Equals(dataTypeFullName, other.dataTypeFullName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && nameCasing == other.nameCasing
            && deduplicateAssetName == other.deduplicateAssetName
            ;

        public readonly override bool Equals(object obj)
            => obj is TableSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeFullName
                , typeSimpleName
                , idTypeFullName
                , dataTypeFullName
                , propertyName
                , nameCasing
                , deduplicateAssetName
            );
    }
}
