using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EncosyTower.Conversion;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using NReco.Csv;

namespace EncosyTower.Data.Authoring
{
    public class DatabaseGoogleSheetCsvExporter
    {
        public readonly string SpreadsheetId;

        private readonly ICredential _credential;
        private readonly IExtendedFileSystem _fileSystem;
        private readonly ITransform<string, string> _spreadsheetNameTransformer;
        private readonly ITransform<string, string> _sheetNameTransformer;

        private Spreadsheet _spreadsheet;
        private bool _isLoaded;

        public DatabaseGoogleSheetCsvExporter(
              string spreadsheetId
            , string credential
            , IExtendedFileSystem fileSystem = null
            , ITransform<string, string> spreadsheetNameTransformer = null
            , ITransform<string, string> sheetNameTransformer = null
        )
        {
            SpreadsheetId = spreadsheetId;

            _credential = GoogleCredential
                .FromJson(credential)
                .CreateScoped(new[] { DriveService.Scope.DriveReadonly });

            _fileSystem = fileSystem ?? new DatabaseFileSystem();
            _spreadsheetNameTransformer = spreadsheetNameTransformer;
            _sheetNameTransformer = sheetNameTransformer;
        }

        public async Task<GoogleFileMetadata> FetchMetadata()
        {
            using var service = new DriveService(new BaseClientService.Initializer {
                HttpClientInitializer = _credential
            });

            var fileReq = service.Files.Get(SpreadsheetId);
            fileReq.SupportsTeamDrives = true;
            fileReq.Fields = "name,modifiedTime";

            var file = await fileReq.ExecuteAsync();
            return new(file.Name, file.ModifiedTime ?? default);
        }

        public async Task Export(
              string savePath
            , bool cleanOutputFolder
            , NamingStrategy namingStrategy
            , HashSet<string> existingFolderNames = null
        )
        {
            var spreadsheetId = SpreadsheetId;

            if (_isLoaded == false)
            {
                using (var service = new SheetsService(new BaseClientService.Initializer {
                    HttpClientInitializer = _credential
                }))
                {
                    var sheetReq = service.Spreadsheets.Get(spreadsheetId);
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
            var folderName = _spreadsheetNameTransformer?.Transform(spreadsheetName) ?? spreadsheetName;

            EnsureUniqueFolderName(existingFolderNames, ref folderName);

            var ignoreOuputPath = Path.Combine(savePath, $"{folderName}~");
            var outputPath = validSpreadsheet ? Path.Combine(savePath, folderName) : ignoreOuputPath;

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
            var sheetNameTransformer = _sheetNameTransformer;

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

                var validSheet = SheetUtility.ValidateSheetName(sheetName);
                var canBeIgnored = validSheet == false || validSpreadsheet == false;
                var fileName = sheetNameTransformer?.Transform(sheetName) ?? sheetName;
                var filePath = Path.Combine(canBeIgnored ? ignoreOuputPath : outputPath, $"{fileName}.csv");

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

        private void EnsureUniqueFolderName(HashSet<string> existingFolderNames, ref string folderName)
        {
            if (existingFolderNames == null)
            {
                return;
            }

            if (existingFolderNames.Contains(folderName))
            {
                folderName = $"{folderName}_{SpreadsheetId}";
            }

            existingFolderNames.Add(folderName);
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
