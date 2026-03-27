using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// Cache-friendly model for a single generated sheet file.
    /// Replaces the non-cacheable <c>DataTableAssetRef</c> class.
    /// </summary>
    public struct SheetModel : IEquatable<SheetModel>
    {
        /// <summary>Pre-computed hint name for this sheet's generated source file.</summary>
        public string hintName;

        /// <summary>Full name of the Id type, e.g. <c>global::Foo.MyId</c>.</summary>
        public string idTypeFullName;

        /// <summary>Simple name of the Id type, e.g. <c>MyId</c>.</summary>
        public string idTypeSimpleName;

        /// <summary>Full name of the main data type, e.g. <c>global::Foo.MyData</c>.</summary>
        public string dataTypeFullName;

        /// <summary>Simple name of the main data type, e.g. <c>MyData</c>.</summary>
        public string dataTypeSimpleName;

        /// <summary>Full name of the DataTableAsset subclass, e.g. <c>global::Foo.MyTable</c>.</summary>
        public string tableTypeFullName;

        /// <summary>Pre-computed sheet class name, e.g. <c>MyTable_MyDataSheet</c>.</summary>
        public string sheetName;

        /// <summary>Full names of nested data types (other <see cref="IData"/> types
        /// referenced in this sheet's data type hierarchy).</summary>
        public EquatableArray<string> nestedDataTypeFullNames;

        public readonly bool IsValid
            => string.IsNullOrEmpty(hintName) == false;

        public readonly bool Equals(SheetModel other)
            => string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(idTypeFullName, other.idTypeFullName, StringComparison.Ordinal)
            && string.Equals(idTypeSimpleName, other.idTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(dataTypeFullName, other.dataTypeFullName, StringComparison.Ordinal)
            && string.Equals(dataTypeSimpleName, other.dataTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(tableTypeFullName, other.tableTypeFullName, StringComparison.Ordinal)
            && string.Equals(sheetName, other.sheetName, StringComparison.Ordinal)
            && nestedDataTypeFullNames.Equals(other.nestedDataTypeFullNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is SheetModel other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  hintName
                , idTypeFullName
                , idTypeSimpleName
                , dataTypeFullName
                , dataTypeSimpleName
                , tableTypeFullName
                , sheetName
            )
            .Add(nestedDataTypeFullNames.GetHashCode());
    }
}
