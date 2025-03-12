namespace EncosyTower.Databases.Authoring
{
    public static class AssetExtensions
    {
        public static void AddRange<TDatabaseAsset>(
              this TDatabaseAsset target
            , DataTableAssetBase[] tables
        )
            where TDatabaseAsset : DatabaseAsset
        {
            target._tables = tables;
        }

        public static void AddRange<TDatabaseAsset>(
              this TDatabaseAsset target
            , DataTableAssetBase[] tables
            , DataTableAssetBase[] redundantTables
        )
            where TDatabaseAsset : DatabaseAsset
        {
            target._tables = tables;
            target._redundantTabless = redundantTables;
        }

        public static void Clear<TDatabaseAsset>(this TDatabaseAsset target)
            where TDatabaseAsset : DatabaseAsset
        {
            target._tables = new DataTableAssetBase[0];
            target._redundantTabless = new DataTableAssetBase[0];
        }
    }
}
