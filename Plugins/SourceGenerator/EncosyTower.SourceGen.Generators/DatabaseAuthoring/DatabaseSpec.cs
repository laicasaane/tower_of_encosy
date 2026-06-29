using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public partial struct DatabaseSpec : IEquatable<DatabaseSpec>
    {
        public LocationInfo location;
        public string databaseTypeName;
        public string databaseTypeKeyword;
        public string databaseIdentifier;
        public string openingSource;
        public string closingSource;
        public string containerHintName;
        public EquatableArray<DataSpec> allDataModels;
        public EquatableArray<ScopedConverterSpec> scopedConverters;
        public EquatableArray<HorizontalListSpec> horizontalListEntries;
        public EquatableArray<TableSpec> tables;
        public EquatableArray<SheetGroupSpec> sheetGroups;
        public EquatableArray<string> typeNames;
        public EquatableArray<SheetSpec> sheets;

        public readonly bool IsValid
            => string.IsNullOrEmpty(databaseTypeName) == false
            && string.IsNullOrEmpty(databaseTypeKeyword) == false
            && string.IsNullOrEmpty(databaseIdentifier) == false
            ;

        public readonly bool Equals(DatabaseSpec other)
            => string.Equals(databaseTypeName, other.databaseTypeName, StringComparison.Ordinal)
            && string.Equals(databaseTypeKeyword, other.databaseTypeKeyword, StringComparison.Ordinal)
            && string.Equals(databaseIdentifier, other.databaseIdentifier, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(containerHintName, other.containerHintName, StringComparison.Ordinal)
            && allDataModels.Equals(other.allDataModels)
            && scopedConverters.Equals(other.scopedConverters)
            && horizontalListEntries.Equals(other.horizontalListEntries)
            && tables.Equals(other.tables)
            && sheetGroups.Equals(other.sheetGroups)
            && typeNames.Equals(other.typeNames)
            && sheets.Equals(other.sheets)
            ;

        public readonly override bool Equals(object obj)
            => obj is DatabaseSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  databaseTypeName
                , databaseTypeKeyword
                , databaseIdentifier
                , openingSource
                , closingSource
                , containerHintName
                , allDataModels
                , horizontalListEntries
            )
            .Add(scopedConverters)
            .Add(tables)
            .Add(sheetGroups)
            .Add(typeNames)
            .Add(sheets)
            ;
    }
}
