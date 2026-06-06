using Cathei.BakingSheet;
using EncosyTower.Data.Authoring;
using EncosyTower.Databases.Authoring;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalCsvFolderSettings
        {
            public const string PROGRESS_TITLE = "Convert CSV Files";

            public override string ProgressTitle => PROGRESS_TITLE;

            protected override ISheetImporter GetImporter(string inputFolderPath)
            {
                var convertingCtx = DataConvertingContext.Default;

                return new DatabaseCsvSheetConverter(
                      inputFolderPath
                    , extension
                    , fileSystem: null
                    , splitHeader
                    , emptyRowStreakThreshold
                    , includeSubFolders
                    , includeCommentedFiles
                );
            }
        }
    }
}
