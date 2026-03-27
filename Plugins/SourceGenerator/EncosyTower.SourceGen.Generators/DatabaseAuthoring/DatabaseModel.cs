using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// Cache-friendly, equatable top-level pipeline model for the DatabaseAuthoring generator.
    /// Replaces the non-cacheable <c>DatabaseRef</c> + <c>DatabaseDeclaration</c> combination.
    /// All extraction from Roslyn symbols happens once in <see cref="Extract"/> and the result
    /// is fully stable across incremental runs as long as the source code is unchanged.
    /// </summary>
    public partial struct DatabaseModel : IEquatable<DatabaseModel>
    {
        /// <summary>Excluded from equality/hash — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        /// <summary>Simple name of the <c>[AuthorDatabase]</c> type, e.g. <c>"MyAuthoringType"</c>.</summary>
        public string databaseTypeName;

        /// <summary><c>"class"</c> or <c>"struct"</c>.</summary>
        public string databaseTypeKeyword;

        /// <summary><c>symbol.ToValidIdentifier()</c> — used to build generated file hint names.</summary>
        public string databaseIdentifier;

        /// <summary>Pre-computed namespace/outer-type opening source (from <c>TypeCreationHelpers.GenerateOpeningAndClosingSource</c>).</summary>
        public string openingSource;

        /// <summary>Counterpart closing braces for <see cref="openingSource"/>.</summary>
        public string closingSource;

        /// <summary>Pre-computed hint name for the SheetContainer generated file.</summary>
        public string containerHintName;

        /// <summary>All data models involved in this database across all tables.</summary>
        public EquatableArray<DataModel> allDataModels;

        /// <summary>Horizontal-list mapping entries (one per unique target-type + containing-type pair).</summary>
        public EquatableArray<HorizontalListEntry> horizontalListEntries;

        /// <summary>Per-table info for <c>WriteDerivedSheetClasses</c>.</summary>
        public EquatableArray<TableModel> tables;

        /// <summary>Per-table-type asset-ref-list models for <c>WriteContainer</c>.</summary>
        public EquatableArray<AssetRefListModel> assetRefLists;

        /// <summary>Ordered list of unique sheet class names for the SheetContainer property declarations.</summary>
        public EquatableArray<string> typeNames;

        /// <summary>Maximum number of fields sharing the same table type — drives <c>RefList</c> capacity.</summary>
        public int maxFieldOfSameTable;

        /// <summary>Per-sheet models for <c>WriteSheet</c>.</summary>
        public EquatableArray<SheetModel> sheets;

        public readonly bool IsValid
            => string.IsNullOrEmpty(databaseTypeName) == false
            && string.IsNullOrEmpty(databaseTypeKeyword) == false
            && string.IsNullOrEmpty(databaseIdentifier) == false
            ;

        public readonly bool Equals(DatabaseModel other)
            => string.Equals(databaseTypeName, other.databaseTypeName, StringComparison.Ordinal)
            && string.Equals(databaseTypeKeyword, other.databaseTypeKeyword, StringComparison.Ordinal)
            && string.Equals(databaseIdentifier, other.databaseIdentifier, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(containerHintName, other.containerHintName, StringComparison.Ordinal)
            && allDataModels.Equals(other.allDataModels)
            && horizontalListEntries.Equals(other.horizontalListEntries)
            && tables.Equals(other.tables)
            && assetRefLists.Equals(other.assetRefLists)
            && typeNames.Equals(other.typeNames)
            && maxFieldOfSameTable == other.maxFieldOfSameTable
            && sheets.Equals(other.sheets)
            ;

        public readonly override bool Equals(object obj)
            => obj is DatabaseModel other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  databaseTypeName
                , databaseTypeKeyword
                , databaseIdentifier
                , openingSource
                , closingSource
                , containerHintName
            )
            .Add(allDataModels.GetHashCode())
            .Add(horizontalListEntries.GetHashCode())
            .Add(tables.GetHashCode())
            .Add(assetRefLists.GetHashCode())
            .Add(typeNames.GetHashCode())
            .Add(maxFieldOfSameTable)
            .Add(sheets.GetHashCode());
    }
}
