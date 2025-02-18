#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EncosyTower.Databases.Authoring;
using EncosyTower.IO;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEditor;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            private IEnumerator DownloadDataTableCoroutine(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
            )
            {
                Progress(ProgressMessageType.None);

                var serviceAccountFilePath = rootPath.GetFileAbsolutePath(serviceAccountRelativeFilePath);
                string serviceAccountJson = null;

                try
                {
                    serviceAccountJson = File.ReadAllText(serviceAccountFilePath);
                }
                catch (Exception ex)
                {
                    DevLoggerAPI.LogException(ex);
                }

                if (string.IsNullOrWhiteSpace(serviceAccountJson))
                {
                    Log(LogMessageType.ServiceAccountJsonUndefined);
                    StopProgress();
                    yield break;
                }

                Progress(ProgressMessageType.ReadFileTable);

                var timeZone = TimeZoneInfo.Utc;
                var fileContainer = new FileDatabase.SheetContainer();
                var fileConverter = new DatabaseGoogleSheetConverter(spreadsheetId, serviceAccountJson, timeZone);
                var fileBakeTask = fileContainer.Bake(fileConverter).AsAwaitable();

                while (fileBakeTask.GetAwaiter().IsCompleted == false)
                {
                    yield return null;
                }

                var converters = new List<DatabaseGoogleSheetConverter>();

                if (fileContainer.FileTableAsset_FileDataSheet is { } fileSheet)
                {
                    foreach (var row in fileSheet)
                    {
                        if (row.MimeType != "application/vnd.google-apps.spreadsheet"
                            || SheetUtility.ValidateSheetName(row.FileName) == false
                        )
                        {
                            continue;
                        }

                        converters.Add(new(row.FileId, serviceAccountJson, timeZone));
                    }
                }
                else
                {
                    converters.Add(fileConverter);
                }

                if (converters.Count < 1)
                {
                    Log(LogMessageType.FoundNoAppropriateSpreadSheet);
                    StopProgress();
                    yield break;
                }

                Progress(ProgressMessageType.Download);

                var sheetBakeTask = sheetContainer.Bake(converters.ToArray()).AsAwaitable();

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

                StopProgress();
                AssetDatabase.Refresh();
            }

            private IEnumerator DownloadCsvCoroutine(
                  RootPath rootPath
                , DataSheetContainerBase sheetContainer
            )
            {
                Progress(ProgressMessageType.None);

                var serviceAccountFilePath = rootPath.GetFileAbsolutePath(serviceAccountRelativeFilePath);
                string serviceAccountJson = null;

                try
                {
                    serviceAccountJson = File.ReadAllText(serviceAccountFilePath);
                }
                catch (Exception ex)
                {
                    DevLoggerAPI.LogException(ex);
                }

                if (string.IsNullOrWhiteSpace(serviceAccountJson))
                {
                    Log(LogMessageType.ServiceAccountJsonUndefined);
                    StopProgress();
                    yield break;
                }

                Progress(ProgressMessageType.ReadFileTable);

                var timeZone = TimeZoneInfo.Utc;
                var fileContainer = new FileDatabase.SheetContainer();
                var fileConverter = new DatabaseGoogleSheetConverter(spreadsheetId, serviceAccountJson, timeZone);
                var fileBakeTask = fileContainer.Bake(fileConverter).AsAwaitable();

                while (fileBakeTask.GetAwaiter().IsCompleted == false)
                {
                    yield return null;
                }

                var fileSystem = new DatabaseFileSystem();
                var exporters = new Dictionary<int, DatabaseGoogleSheetCsvExporter> {
                    { 0, new(spreadsheetId, serviceAccountJson, fileSystem) }
                };

                if (fileContainer.FileTableAsset_FileDataSheet is { } fileSheet)
                {
                    var id = 1;

                    foreach (var row in fileSheet)
                    {
                        if (row.MimeType != "application/vnd.google-apps.spreadsheet")
                        {
                            continue;
                        }

                        exporters.TryAdd(id, new(row.FileId, serviceAccountJson, fileSystem));
                        id += 1;
                    }
                }

                if (alwaysDownloadAll && fileSystem.DirectoryExists(outputRelativeFolderPath))
                {
                    fileSystem.DeleteDirectory(outputRelativeFolderPath);
                }

                fileSystem.CreateDirectory(outputRelativeFolderPath);

                Progress(ProgressMessageType.Validate);

                var validateTask = ValidateAsync(exporters, fileSystem, false).AsAwaitable();

                while (validateTask.GetAwaiter().IsCompleted == false)
                {
                    yield return null;
                }

                Progress(ProgressMessageType.Download);

                var downloadTask = DownloadAsync(exporters, false).AsAwaitable();

                while (downloadTask.GetAwaiter().IsCompleted == false)
                {
                    yield return null;
                }

                StopProgress();
                AssetDatabase.Refresh();
            }
        }
    }
}

#endif
