using System;
using Cathei.BakingSheet;
using EncosyTower.Databases.Authoring;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalCsvFolderSettings
        {
            private static readonly string s_progressTitle = "Convert CSV Files";

            protected override string ProgressTitle => s_progressTitle;

            protected override ISheetImporter GetImporter(string inputFolderPath, TimeZoneInfo timeZone)
                => new DatabaseCsvSheetConverter(inputFolderPath, timeZone);
        }
    }
}
