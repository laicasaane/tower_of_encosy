using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct SheetSpec : IEquatable<SheetSpec>
    {
        public string hintName;
        public string idTypeFullName;
        public string idTypeSimpleName;
        public string dataTypeFullName;
        public string dataTypeSimpleName;
        public string tableTypeFullName;
        public string sheetName;
        public EquatableArray<string> nestedDataTypeFullNames;

        public readonly bool IsValid
            => string.IsNullOrEmpty(hintName) == false;

        public readonly bool Equals(SheetSpec other)
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
            => obj is SheetSpec other && Equals(other);

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
