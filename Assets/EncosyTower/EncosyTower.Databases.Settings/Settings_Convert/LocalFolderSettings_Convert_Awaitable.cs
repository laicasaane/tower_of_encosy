#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Collections;
using EncosyTower.Databases.Authoring;
using EncosyTower.IO;
using EncosyTower.UnityExtensions;
using UnityEditor;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalFolderSettings
        {
            private IEnumerator ConvertCoroutine(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
            )
            {
                var inputFolderPath = rootPath.GetFolderAbsolutePath(inputRelativeFolderPath);
                var importer = GetImporter(inputFolderPath, TimeZoneInfo.Utc);

                EditorUtility.DisplayProgressBar(ProgressTitle, "Converting all files...", 0);

                var sheetBakeTask = sheetContainer.Bake(importer).AsAwaitable();

                while (sheetBakeTask.GetAwaiter().IsCompleted == false)
                {
                    yield return null;
                }

                var exporter = new DatabaseAssetExporter<DatabaseAsset>(outputRelativeFolderPath, databaseAssetName);
                var sheetStoreTask = sheetContainer.Store(exporter).AsAwaitable();

                while (sheetStoreTask.GetAwaiter().IsCompleted == false)
                {
                    yield return null;
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
    }
}

#endif
