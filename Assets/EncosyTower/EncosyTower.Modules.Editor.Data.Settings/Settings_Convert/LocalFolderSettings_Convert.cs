using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using Cysharp.Threading.Tasks;
using EncosyTower.Modules.Data;
using EncosyTower.Modules.Data.Authoring;
using EncosyTower.Modules.IO;
using EncosyTower.Modules.Logging;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace EncosyTower.Modules.Editor.Data.Settings
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

            private IEnumerator ConvertCoroutine(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
            )
            {
                var inputFolderPath = rootPath.GetFolderAbsolutePath(inputRelativeFolderPath);
                var importer = GetImporter(inputFolderPath, TimeZoneInfo.Utc);

                EditorUtility.DisplayProgressBar(ProgressTitle, "Converting all files...", 0);

                var sheetBakeTask = sheetContainer.Bake(importer).AsUniTask();

                while (sheetBakeTask.Status == UniTaskStatus.Pending)
                {
                    yield return null;
                }

                var exporter = new DatabaseAssetExporter<DatabaseAsset>(outputRelativeFolderPath, databaseAssetName);
                var sheetStoreTask = sheetContainer.Store(exporter).AsUniTask();

                while (sheetStoreTask.Status == UniTaskStatus.Pending)
                {
                    yield return null;
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

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
