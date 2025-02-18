#if UNITASK

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using EncosyTower.Databases.Authoring;
using EncosyTower.IO;
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
        }
    }
}

#endif
