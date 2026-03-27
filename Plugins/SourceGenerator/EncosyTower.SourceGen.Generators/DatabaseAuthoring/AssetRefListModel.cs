using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// Cache-friendly model for a group of tables sharing the same DataTableAsset type.
    /// Replaces the non-cacheable <c>DataTableAssetRefList</c> class.
    /// Used to generate both <c>RefList&lt;T&gt;</c> members in SheetContainer and
    /// the <c>WriteDerivedSheetClasses</c> output.
    /// </summary>
    public struct AssetRefListModel : IEquatable<AssetRefListModel>
    {
        /// <summary>Full name of the DataTableAsset subclass (the table type).</summary>
        public string tableTypeFullName;

        /// <summary>Simple name of the table type, e.g. <c>MyTable</c>.</summary>
        public string tableTypeSimpleName;

        /// <summary>Full name of the data type stored in the table.</summary>
        public string dataTypeFullName;

        /// <summary>Simple name of the data type, e.g. <c>MyData</c>.</summary>
        public string dataTypeSimpleName;

        /// <summary>Property names on the database class that reference this table type.</summary>
        public EquatableArray<string> fieldNames;

        public readonly bool Equals(AssetRefListModel other)
            => string.Equals(tableTypeFullName, other.tableTypeFullName, StringComparison.Ordinal)
            && string.Equals(tableTypeSimpleName, other.tableTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(dataTypeFullName, other.dataTypeFullName, StringComparison.Ordinal)
            && string.Equals(dataTypeSimpleName, other.dataTypeSimpleName, StringComparison.Ordinal)
            && fieldNames.Equals(other.fieldNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is AssetRefListModel other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(tableTypeFullName, tableTypeSimpleName, dataTypeFullName, dataTypeSimpleName)
            .Add(fieldNames.GetHashCode());
    }
}
