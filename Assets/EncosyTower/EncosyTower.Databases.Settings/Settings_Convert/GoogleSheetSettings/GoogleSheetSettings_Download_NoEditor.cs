using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Conversion;
using EncosyTower.Databases.Authoring;
using EncosyTower.IO;
using EncosyTower.Naming;
using UnityEditor;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            private async Task<bool> DownloadDataTableAsync(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {
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

                var initializer = await ConnectAsync(
                      credentialText
                    , secrets
                    , tokenFolderPath
                    , token
                    , continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);

                var fileConverter = new DatabaseGoogleSheetConverter(spreadsheetId, initializer, timeZone);

                await fileContainer.Bake(fileConverter)
                    .ConfigureAwait(continueOnCapturedContext);

                var converters = new List<DatabaseGoogleSheetConverter>();

                if (fileContainer.FileTableAsset_FileDataSheets[0] is { } fileSheet)
                {
                    foreach (var row in fileSheet)
                    {
                        if (row.MimeType != "application/vnd.google-apps.spreadsheet"
                            || SheetUtility.ValidateSheetName(row.FileName) == false
                        )
                        {
                            continue;
                        }

                        converters.Add(new(row.FileId, initializer, timeZone));
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

                await sheetContainer.Bake(converters.ToArray())
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

            private async Task<bool> DownloadCsvAsync(
                  RootPath rootPath
                , string databaseAssetName
                , DataSheetContainerBase sheetContainer
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {
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

                var initializer = await ConnectAsync(
                      credentialText
                    , secrets
                    , tokenFolderPath
                    , token
                    , continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);

                var fileConverter = new DatabaseGoogleSheetConverter(spreadsheetId, initializer, timeZone);

                await fileContainer.Bake(fileConverter).ConfigureAwait(continueOnCapturedContext);

                var fileSystem = new DatabaseFileSystem();
                var fileSheetExporter = new DatabaseGoogleSheetCsvExporter(
                      spreadsheetId
                    , initializer
                    , fileSystem
                    , IgnoredNameTransformer.Default
                    , IgnoredNameTransformer.Default
                );

                var exporters = new Dictionary<int, DatabaseGoogleSheetCsvExporter> {
                    { 0, fileSheetExporter}
                };

                if (fileContainer.FileTableAsset_FileDataSheets[0] is { } fileSheet)
                {
                    var id = 1;

                    foreach (var row in fileSheet)
                    {
                        if (row.MimeType != "application/vnd.google-apps.spreadsheet")
                        {
                            continue;
                        }

                        exporters.TryAdd(id, new(row.FileId, initializer, fileSystem));
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

                await DownloadAsync(exporters, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);

                if (continueOnCapturedContext)
                {
                    AssetDatabase.Refresh();
                }

                return true;
            }

            private async Task ValidateAsync(
                  Dictionary<int, DatabaseGoogleSheetCsvExporter> exporters
                , DatabaseFileSystem fileSystem
                , bool continueOnCapturedContext
            )
            {
                var folderPath = Path.Combine(outputRelativeFolderPath, "$.time_checks~");
                fileSystem.CreateDirectory(folderPath);

                await File.WriteAllTextAsync(Path.Combine(folderPath, ".preserved"), string.Empty)
                    .ConfigureAwait(continueOnCapturedContext);

                var toRemove = new List<int>(exporters.Count);

                foreach (var (id, exporter) in exporters)
                {
                    var (gsheetName, datetime) = await exporter.FetchMetadata()
                        .ConfigureAwait(continueOnCapturedContext);

                    var name = SheetUtility.ToFileName(gsheetName);
                    var fileName = $"{name}_{exporter.SpreadsheetId}.time_check";
                    var filePath = Path.Combine(folderPath, fileName);
                    var savedTime = 0L;

                    if (File.Exists(filePath))
                    {
                        var text = await File.ReadAllTextAsync(filePath)
                            .ConfigureAwait(continueOnCapturedContext);

                        if (long.TryParse(text, out var value))
                        {
                            savedTime = value;
                        }
                    }

                    var modified = datetime.Ticks;

                    if (modified <= savedTime)
                    {
                        toRemove.Add(id);
                        continue;
                    }

                    await File.WriteAllTextAsync(filePath, modified.ToString())
                        .ConfigureAwait(continueOnCapturedContext);
                }

                foreach (var id in toRemove)
                {
                    exporters.Remove(id);
                }
            }

            private async Task DownloadAsync(
                  Dictionary<int, DatabaseGoogleSheetCsvExporter> exporters
                , bool continueOnCapturedContext
            )
            {
                var outputFolderPath = outputRelativeFolderPath;
                var cleanFolder = cleanOutputFolder;
                var existingFolderNames = new HashSet<string>(exporters.Count);

                foreach (var (_, exporter) in exporters)
                {
                    await exporter.Export(
                          outputFolderPath
                        , cleanFolder
                        , NamingStrategy.SnakeCase
                        , existingFolderNames
                    ).ConfigureAwait(continueOnCapturedContext);
                }
            }

            private class IgnoredNameTransformer : ITransform<string, string>
            {
                public static readonly IgnoredNameTransformer Default = new();

                public string Transform(string from)
                    => SheetUtility.ValidateSheetName(from) ? $"${from}" : from;
            }
        }
    }
}
