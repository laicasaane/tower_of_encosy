using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct SheetSpec : IEquatable<SheetSpec>
    {
        public string sheetName;
        public string idTypeFullName;
        public string idTypeSimpleName;
        public string dataTypeFullName;
        public string dataTypeSimpleName;
        public string tableTypeFullName;
        public EquatableArray<string> nestedDataTypeFullNames;
        public HashValue64 scopeKey;
        public string hintName;

        public readonly bool IsValid
            => string.IsNullOrEmpty(idTypeFullName) == false
            && string.IsNullOrEmpty(dataTypeFullName) == false;

        public readonly bool Equals(SheetSpec other)
            => string.Equals(idTypeFullName, other.idTypeFullName, StringComparison.Ordinal)
            && string.Equals(idTypeSimpleName, other.idTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(dataTypeFullName, other.dataTypeFullName, StringComparison.Ordinal)
            && string.Equals(dataTypeSimpleName, other.dataTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(tableTypeFullName, other.tableTypeFullName, StringComparison.Ordinal)
            && string.Equals(sheetName, other.sheetName, StringComparison.Ordinal)
            && scopeKey == other.scopeKey
            && nestedDataTypeFullNames.Equals(other.nestedDataTypeFullNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is SheetSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  idTypeFullName
                , idTypeSimpleName
                , dataTypeFullName
                , dataTypeSimpleName
                , tableTypeFullName
                , sheetName
                , scopeKey
                , nestedDataTypeFullNames
            );
    }
}
