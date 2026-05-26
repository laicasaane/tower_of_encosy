using Cathei.BakingSheet;
using EncosyTower.Databases.Authoring;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalExcelFolderSettings
        {
            public const string PROGRESS_TITLE = "Convert CSV Files";

            public override string ProgressTitle => PROGRESS_TITLE;

            protected override ISheetImporter GetImporter(string inputFolderPath)
            {
                return new DatabaseExcelSheetConverter(
                      inputFolderPath
                    , extension
                    , fileSystem: null
                    , emptyRowStreakThreshold
                    , includeSubFolders
                    , includeCommentedFiles
                );
            }
        }
    }
}
