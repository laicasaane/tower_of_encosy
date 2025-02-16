using System;
using Cathei.BakingSheet;
using EncosyTower.Data.Authoring;

namespace EncosyTower.Editor.Data.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalExcelFolderSettings
        {
            private static readonly string s_progressTitle = "Convert CSV Files";

            protected override string ProgressTitle => s_progressTitle;

            protected override ISheetImporter GetImporter(string inputFolderPath, TimeZoneInfo timeZone)
                => new DatabaseExcelSheetConverter(inputFolderPath, timeZone);
        }
    }
}
