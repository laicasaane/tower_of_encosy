using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using NReco.Csv;

namespace EncosyTower.Modules.Data.Authoring
{
    public class DatabaseGoogleSheetCsvExporter
    {
        private readonly string _gsheetAddress;
        private readonly ICredential _credential;
        private readonly IExtendedFileSystem _fileSystem;

        private Spreadsheet _spreadsheet;
        private bool _isLoaded;

        public DatabaseGoogleSheetCsvExporter(
              string gsheetAddress
            , string credential
            , IExtendedFileSystem fileSystem = null
        )
        {
            _gsheetAddress = gsheetAddress;
            _credential = GoogleCredential
                .FromJson(credential)
                .CreateScoped(new[] { DriveService.Scope.DriveReadonly });

            _fileSystem = fileSystem ?? new DatabaseFileSystem();
        }

        public async Task<DateTime> FetchModifiedTime()
        {
            using var service = new DriveService(new BaseClientService.Initializer {
                HttpClientInitializer = _credential
            });

            var fileReq = service.Files.Get(_gsheetAddress);
            fileReq.SupportsTeamDrives = true;
            fileReq.Fields = "modifiedTime";

            var file = await fileReq.ExecuteAsync();
            return file.ModifiedTime ?? default;
        }

        public async Task Export(
              string savePath
            , bool spreadsheetAsFolder
            , bool cleanOutputFolder
            , NamingStrategy namingStrategy
        )
        {
            if (_isLoaded == false)
            {
                using (var service = new SheetsService(new BaseClientService.Initializer {
                    HttpClientInitializer = _credential
                }))
                {
                    var sheetReq = service.Spreadsheets.Get(_gsheetAddress);
                    sheetReq.Fields = "properties,sheets(properties,data.rowData.values.formattedValue)";
                    _spreadsheet = await sheetReq.ExecuteAsync();
                }

                _isLoaded = true;
            }

            var spreadsheetName = NamingMap.ConvertName(
                  SheetUtility.ToFileName(_spreadsheet.Properties.Title)
                , namingStrategy
            );

            var fileSystem = _fileSystem;
            var validSpreadsheet = SheetUtility.ValidateSheetName(spreadsheetName);
            var ignoreOuputPath = Path.Combine(savePath, $"{spreadsheetName}~");

            var outputPath = validSpreadsheet
                ? spreadsheetAsFolder ? Path.Combine(savePath, spreadsheetName) : savePath
                : ignoreOuputPath;

            if (cleanOutputFolder)
            {
                if (fileSystem.DirectoryExists(ignoreOuputPath))
                {
                    fileSystem.DeleteDirectory(ignoreOuputPath, true);
                }

                if (fileSystem.DirectoryExists(outputPath))
                {
                    fileSystem.DeleteDirectory(outputPath, true);
                }
            }

            var sheets = _spreadsheet.Sheets;
            var sheetCount = sheets.Count;
            var outputPathExists = false;
            var ignoreOutputPathExists = false;

            for (var i = 0; i < sheetCount; i++)
            {
                var gSheet = sheets[i];
                var data = gSheet.Data.FirstOrDefault();

                if (data == null)
                {
                    continue;
                }

                var sheetName = NamingMap.ConvertName(
                      SheetUtility.ToFileName(gSheet.Properties.Title, i)
                    , namingStrategy
                );


                var canBeIgnored = CanBeIgnored(spreadsheetName, sheetName);
                var filePath = Path.Combine(canBeIgnored ? ignoreOuputPath : outputPath, $"{sheetName}.csv");

                if (canBeIgnored)
                {
                    EnsureOutputPath(fileSystem, ignoreOuputPath, ref ignoreOutputPathExists);
                }
                else
                {
                    EnsureOutputPath(fileSystem, outputPath, ref outputPathExists);
                }

                await using var stream = fileSystem.OpenWrite(filePath);
                await using var writer = new StreamWriter(stream);

                var rows = data.RowData;
                var rowCount = rows?.Count ?? 0;

                if (rowCount < 1)
                {
                    continue;
                }

                var totalColumnCount = CountColumns(rows);

                if (totalColumnCount < 1)
                {
                    continue;
                }

                var csv = new CsvWriter(writer);

                for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    var row = rows[rowIndex];
                    var columns = row.Values;

                    if (columns == null || columns.Count < 1)
                    {
                        continue;
                    }

                    var columnCount = columns.Count;

                    for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        csv.WriteField(columns[columnIndex]?.FormattedValue);
                    }

                    var emptyCount = totalColumnCount - columnCount;

                    for (var columnIndex = 0; columnIndex <= emptyCount; columnIndex++)
                    {
                        csv.WriteField(string.Empty);
                    }

                    csv.NextRecord();
                }
            }
        }

        private static int CountColumns([NotNull] IList<RowData> rows)
        {
            var result = 0;

            foreach (var row in rows)
            {
                var cols = row.Values;
                var colCount = cols?.Count ?? 0;

                if (colCount > result)
                {
                    result = colCount;
                }
            }

            return result;
        }

        private static bool CanBeIgnored(string spreadsheetName, string sheetName)
        {
            return SheetUtility.ValidateSheetName(sheetName) == false
                || SheetUtility.ValidateSheetName(spreadsheetName) == false;
        }

        private static void EnsureOutputPath(IExtendedFileSystem fileSystem, string ouputPath, ref bool exists)
        {
            if (exists)
            {
                return;
            }

            exists = fileSystem.Exists(ouputPath);

            if (exists == false)
            {
                fileSystem.CreateDirectory(ouputPath);
            }
        }
    }
}
