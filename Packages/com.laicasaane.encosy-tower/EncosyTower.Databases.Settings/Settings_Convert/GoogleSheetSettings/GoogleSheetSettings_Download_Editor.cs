using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Databases.Authoring;
using EncosyTower.IO;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            private async Task<bool> DownloadDataTableEditorAsync(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
                , ReportAction reporter
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {
                Report(reporter, ProgressMessageType.None);

                var prepareResult = Prepare(
                      rootPath
                    , databaseAssetName
                    , out var tokenFolderPath
                    , out var credentialText
                    , out var secrets
                    , out var timeZone
                    , out var fileContainer
                );

                if (prepareResult == false)
                {
                    return false;
                }

                Report(reporter, ProgressMessageType.Authorize);

                var initializer = await ConnectAsync(
                      credentialText
                    , secrets
                    , tokenFolderPath
                    , token
                    , continueOnCapturedContext
                );

                if (token.IsCancellationRequested)
                {
                    return false;
                }

                if (initializer == null)
                {
                    return false;
                }

                var fileConverter = new DatabaseGoogleSheetConverter(
                      spreadsheetId
                    , initializer
                    , timeZone
                    , formatProvider: null
                    , emptyRowStreakThreshold
                );

                await fileContainer.Bake(fileConverter).ConfigureAwait(continueOnCapturedContext);

                var converters = new List<DatabaseGoogleSheetConverter>();

                if (fileContainer.FileTableAsset_FileDataSheets[0] is { } fileSheet && fileSheet.Count > 0)
                {
                    foreach (var row in fileSheet)
                    {
                        if (row.MimeType != "application/vnd.google-apps.spreadsheet"
                            || SheetUtility.ValidateSheetName(row.FileName) == false
                        )
                        {
                            continue;
                        }

                        converters.Add(new DatabaseGoogleSheetConverter(
                              row.FileId
                            , initializer
                            , timeZone
                            , formatProvider: null
                            , emptyRowStreakThreshold
                        ));
                    }
                }
                else
                {
                    converters.Add(fileConverter);
                }

                if (converters.Count < 1)
                {
                    Log(LogMessageType.FoundNoAppropriateSpreadSheet);
                    return false;
                }

                Report(reporter, ProgressMessageType.Download);

                await sheetContainer.Bake(converters.ToArray()).ConfigureAwait(continueOnCapturedContext);

                var exporter = new DatabaseAssetExporter<DatabaseAsset>(outputRelativeFolderPath, databaseAssetName);

                await sheetContainer.Store(exporter).ConfigureAwait(continueOnCapturedContext);

                if (continueOnCapturedContext)
                {
                    AssetDatabase.Refresh();
                }

                return true;
            }

            private async Task<bool> DownloadCsvEditorAsync(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
                , ReportAction reporter
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {

                Report(reporter, ProgressMessageType.None);

                var prepareResult = Prepare(
                      rootPath
                    , databaseAssetName
                    , out var tokenFolderPath
                    , out var credentialText
                    , out var secrets
                    , out var timeZone
                    , out var fileContainer
                );

                if (prepareResult == false)
                {
                    return false;
                }

                Report(reporter, ProgressMessageType.Authorize);

                var initializer = await ConnectAsync(
                      credentialText
                    , secrets
                    , tokenFolderPath
                    , token
                    , continueOnCapturedContext
                );

                if (token.IsCancellationRequested)
                {
                    return false;
                }

                if (initializer == null)
                {
                    return false;
                }

                var fileConverter = new DatabaseGoogleSheetConverter(
                      spreadsheetId
                    , initializer
                    , timeZone
                    , formatProvider: null
                    , emptyRowStreakThreshold
                );

                await fileContainer.Bake(fileConverter).ConfigureAwait(continueOnCapturedContext);

                Report(reporter, ProgressMessageType.Validate);

                var fileSystem = new DatabaseFileSystem();
                var exporters = new Dictionary<int, DatabaseGoogleSheetCsvExporter> {
                    { 0, new(spreadsheetId, initializer, fileSystem) }
                };

                if (fileContainer.FileTableAsset_FileDataSheets[0] is { } fileSheet && fileSheet.Count > 0)
                {
                    var id = 1;

                    foreach (var row in fileSheet)
                    {
                        if (row.MimeType != "application/vnd.google-apps.spreadsheet")
                        {
                            continue;
                        }

                        exporters.TryAdd(id, new DatabaseGoogleSheetCsvExporter(row.FileId, initializer, fileSystem));
                        id += 1;
                    }
                }

                if (alwaysDownloadAll && fileSystem.DirectoryExists(outputRelativeFolderPath))
                {
                    fileSystem.DeleteDirectory(outputRelativeFolderPath);
                }

                fileSystem.CreateDirectory(outputRelativeFolderPath);

                await ValidateAsync(exporters, fileSystem, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);

                Report(reporter, ProgressMessageType.Download);

                await DownloadAsync(exporters, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);

                if (continueOnCapturedContext)
                {
                    AssetDatabase.Refresh();
                }

                return true;
            }
        }
    }
}
