using System.Threading.Tasks;
using EncosyTower.IO;
using EncosyTower.Logging;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace EncosyTower.Editor.Data.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            public void Convert(ConversionArgs args)
            {
                var rootPath = new RootPath(EditorAPI.ProjectPath);
                var validationResult = Validate(rootPath);

                if (validationResult != ValidationResult.Success)
                {
                    Log(validationResult);
                    return;
                }

                rootPath.CreateRelativeFolder(outputRelativeFolderPath);

                AssetDatabase.Refresh();

                switch (outputFileType)
                {
                    case OutputFileType.DataTable:
                    {
                        var coroutine = DownloadDataTableCoroutine(
                              rootPath
                            , args.DatabaseAssetName
                            , args.SheetContainer
                        );

                        EditorCoroutineUtility.StartCoroutine(coroutine, args.Owner);
                        return;
                    }

                    case OutputFileType.Csv:
                    {
                        var coroutine = DownloadCsvCoroutine(
                              rootPath
                            , args.SheetContainer
                        );

                        EditorCoroutineUtility.StartCoroutine(coroutine, args.Owner);
                        return;
                    }
                }
            }

            public async Task<bool> ConvertAsync(ConversionArgs args, bool continueOnCapturedContext)
            {
                var rootPath = new RootPath(EditorAPI.ProjectPath);
                var validationResult = Validate(rootPath);

                if (validationResult != ValidationResult.Success)
                {
                    Log(validationResult);
                    return false;
                }

                rootPath.CreateRelativeFolder(outputRelativeFolderPath);

                if (continueOnCapturedContext)
                {
                    AssetDatabase.Refresh();
                }

                switch (outputFileType)
                {
                    case OutputFileType.DataTable:
                    {
                        return await DownloadDataTableAsync(
                              rootPath
                            , args.DatabaseAssetName
                            , args.SheetContainer
                            , continueOnCapturedContext
                        );
                    }

                    case OutputFileType.Csv:
                    {
                        return await DownloadCsvAsync(
                              rootPath
                            , args.SheetContainer
                            , continueOnCapturedContext
                        );
                    }
                }

                return false;
            }

            private ValidationResult Validate(RootPath rootPath)
            {
                if (rootPath.ExistsRelativeFile(serviceAccountRelativeFilePath) == false)
                {
                    return ValidationResult.ServiceAccountFileDoesNotExist;
                }

                if (string.IsNullOrWhiteSpace(spreadsheetId))
                {
                    return ValidationResult.SpreadSheetIdIsNullOrInvalid;
                }

                if (string.IsNullOrWhiteSpace(outputRelativeFolderPath))
                {
                    return ValidationResult.OutputFolderIsNullOrInvalid;
                }

                return ValidationResult.Success;
            }

            private void Log(ValidationResult validationResult)
            {
                switch (validationResult)
                {
                    case ValidationResult.ServiceAccountFileDoesNotExist:
                        DevLoggerAPI.LogError($"Service account file does not exist: '{serviceAccountRelativeFilePath}'");
                        break;

                    case ValidationResult.SpreadSheetIdIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Spreadsheet ID is null or invalid: '{spreadsheetId}'");
                        break;

                    case ValidationResult.OutputFolderIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Output folder is null or invalid: '{outputRelativeFolderPath}'");
                        break;
                }
            }

            private enum ValidationResult
            {
                Success,
                ServiceAccountFileDoesNotExist,
                SpreadSheetIdIsNullOrInvalid,
                OutputFolderIsNullOrInvalid,
            }
        }
    }
}
