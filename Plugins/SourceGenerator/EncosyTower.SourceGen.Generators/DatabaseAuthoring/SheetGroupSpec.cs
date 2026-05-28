using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct SheetGroupSpec : IEquatable<SheetGroupSpec>
    {
        public string baseSheetName;
        public EquatableArray<SheetInfoSpec> sheets;

        public readonly bool Equals(SheetGroupSpec other)
            => string.Equals(baseSheetName, other.baseSheetName, StringComparison.Ordinal)
            && sheets.Equals(other.sheets)
            ;

        public readonly override bool Equals(object obj)
            => obj is SheetGroupSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(baseSheetName, sheets);
    }

    public struct SheetInfoSpec : IEquatable<SheetInfoSpec>
    {
        public string tableName;
        public string propertyName;

        public readonly bool Equals(SheetInfoSpec other)
            => string.Equals(tableName, other.tableName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is SheetGroupSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(tableName, propertyName);
    }
}
