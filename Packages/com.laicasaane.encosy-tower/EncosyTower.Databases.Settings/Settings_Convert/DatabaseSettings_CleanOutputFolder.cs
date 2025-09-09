namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class DatabaseSettings
        {
            public void CleanOutputFolder(DataSourceFlags sources)
            {
                if (sources.HasFlag(DataSourceFlags.GoogleSheet))
                {
                    googleSheetSettings.CleanOutputFolder();
                }

                if (sources.HasFlag(DataSourceFlags.Csv))
                {
                    csvSettings.CleanOutputFolder();
                }

                if (sources.HasFlag(DataSourceFlags.Excel))
                {
                    excelSettings.CleanOutputFolder();
                }
            }
        }
    }
}
