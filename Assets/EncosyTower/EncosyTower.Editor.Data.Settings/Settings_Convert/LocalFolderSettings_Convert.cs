using System;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using EncosyTower.Data;
using EncosyTower.Data.Authoring;
using EncosyTower.IO;
using EncosyTower.Logging;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace EncosyTower.Editor.Data.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalFolderSettings
        {
            protected abstract string ProgressTitle { get; }

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

                var coroutine = ConvertCoroutine(
                      rootPath
                    , args.DatabaseAssetName
                    , args.SheetContainer
                );

                EditorCoroutineUtility.StartCoroutine(coroutine, args.Owner);
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

                return await ConvertAsync(
                      rootPath
                    , args.DatabaseAssetName
                    , args.SheetContainer
                    , continueOnCapturedContext
                );
            }

            protected abstract ISheetImporter GetImporter(string inputFolderPath, TimeZoneInfo timeZone);

            private async Task<bool> ConvertAsync(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
                , bool continueOnCapturedContext
            )
            {
                var inputFolderPath = rootPath.GetFolderAbsolutePath(inputRelativeFolderPath);
                var converter = GetImporter(inputFolderPath, TimeZoneInfo.Utc);

                await sheetContainer.Bake(converter)
                    .ConfigureAwait(continueOnCapturedContext);

                var exporter = new DatabaseAssetExporter<DatabaseAsset>(outputRelativeFolderPath, databaseAssetName);

                await sheetContainer.Store(exporter)
                    .ConfigureAwait(continueOnCapturedContext);

                if (continueOnCapturedContext)
                {
                    AssetDatabase.Refresh();
                }

                return true;
            }

            private ValidationResult Validate(RootPath rootPath)
            {
                if (string.IsNullOrWhiteSpace(inputRelativeFolderPath))
                {
                    return ValidationResult.InputFolderIsNullOrInvalid;
                }

                if (rootPath.ExistsRelativeFolder(inputRelativeFolderPath) == false)
                {
                    return ValidationResult.InputFolderDoesNotExist;
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
                    case ValidationResult.InputFolderIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Input folder is null or invalid: '{inputRelativeFolderPath}'");
                        break;

                    case ValidationResult.InputFolderDoesNotExist:
                        DevLoggerAPI.LogError($"Input folder does not exist: '{inputRelativeFolderPath}'");
                        break;

                    case ValidationResult.OutputFolderIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Output folder is null or invalid: '{outputRelativeFolderPath}'");
                        break;
                }
            }

            private enum ValidationResult
            {
                Success,
                InputFolderIsNullOrInvalid,
                InputFolderDoesNotExist,
                OutputFolderIsNullOrInvalid,
            }
        }
    }
}
