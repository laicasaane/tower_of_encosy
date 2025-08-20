using System;
using Cathei.BakingSheet;
using EncosyTower.Databases.Authoring;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalCsvFolderSettings
        {
            public const string PROGRESS_TITLE = "Convert CSV Files";

            public override string ProgressTitle => PROGRESS_TITLE;

            protected override ISheetImporter GetImporter(string inputFolderPath, TimeZoneInfo timeZone)
            {
                return new DatabaseCsvSheetConverter(
                      inputFolderPath
                    , timeZone
                    , extension
                    , fileSystem: null
                    , splitHeader
                    , formatProvider: null
                    , emptyRowStreakThreshold
                    , includeSubFolders
                    , includeCommentedFiles
                );
            }
        }
    }
}
