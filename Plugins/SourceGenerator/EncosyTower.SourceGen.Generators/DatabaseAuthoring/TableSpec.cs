using System;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct TableSpec : IEquatable<TableSpec>
    {
        public string typeFullName;
        public string typeSimpleName;
        public string idTypeFullName;
        public string dataTypeFullName;
        public string propertyName;
        public NamingStrategy namingStrategy;
        public string assetName;
        public string uniqueSheetName;
        public string baseSheetName;

        public readonly bool Equals(TableSpec other)
            => string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeSimpleName, other.typeSimpleName, StringComparison.Ordinal)
            && string.Equals(idTypeFullName, other.idTypeFullName, StringComparison.Ordinal)
            && string.Equals(dataTypeFullName, other.dataTypeFullName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && namingStrategy == other.namingStrategy
            && string.Equals(assetName, other.assetName, StringComparison.Ordinal)
            && string.Equals(uniqueSheetName, other.uniqueSheetName, StringComparison.Ordinal)
            && string.Equals(baseSheetName, other.baseSheetName, StringComparison.Ordinal)
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
                , assetName
                , uniqueSheetName
                , baseSheetName
            )
            .Add((byte)namingStrategy);
    }
}
