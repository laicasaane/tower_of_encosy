using System;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// Cache-friendly model for a single database table entry, used to generate
    /// the derived sheet class declarations (<c>WriteDerivedSheetClasses</c>).
    /// Replaces the non-cacheable <c>TableRef</c> class.
    /// </summary>
    public struct TableModel : IEquatable<TableModel>
    {
        /// <summary>Full name of the DataTableAsset subclass.</summary>
        public string typeFullName;

        /// <summary>Simple name of the DataTableAsset subclass.</summary>
        public string typeSimpleName;

        /// <summary>Full name of the Id type.</summary>
        public string idTypeFullName;

        /// <summary>Full name of the data type.</summary>
        public string dataTypeFullName;

        /// <summary>Property name on the database class.</summary>
        public string propertyName;

        public NamingStrategy namingStrategy;

        /// <summary>Pre-computed asset name, e.g. <c>"MyTable_MyProperty"</c> in the chosen naming strategy.</summary>
        public string assetName;

        /// <summary>Pre-computed unique sheet class name, e.g. <c>"MyTable_MyDataSheet__MyProperty"</c>.</summary>
        public string uniqueSheetName;

        /// <summary>Pre-computed base sheet class name, e.g. <c>"MyTable_MyDataSheet"</c>.</summary>
        public string baseSheetName;

        public readonly bool Equals(TableModel other)
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
            => obj is TableModel other && Equals(other);

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
