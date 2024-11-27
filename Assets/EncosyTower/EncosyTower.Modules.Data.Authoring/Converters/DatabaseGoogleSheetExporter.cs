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
    public class DatabaseGoogleSheetExporter
    {
        private readonly string _gsheetAddress;
        private readonly ICredential _credential;
        private readonly IExtendedFileSystem _fileSystem;

        private Spreadsheet _spreadsheet;
        private bool _isLoaded;

        public DatabaseGoogleSheetExporter(
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
            , bool spreadsheetAsFolder = true
            , bool cleanOutputFolder = true
            , ITransformFileName fileNameTransformer = null
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

            var spreadsheetName = SheetUtility.ToFileName(_spreadsheet.Properties.Title);
            var outputPath = spreadsheetAsFolder
                ? Path.Combine(savePath, spreadsheetName)
                : savePath;

            var fileSystem = _fileSystem;

            if (cleanOutputFolder && fileSystem.DirectoryExists(outputPath))
            {
                fileSystem.DeleteDirectory(outputPath);
            }

            fileSystem.CreateDirectory(outputPath);

            var sheets = _spreadsheet.Sheets;
            var sheetCount = sheets.Count;

            for (var i = 0; i < sheetCount; i++)
            {
                var gSheet = sheets[i];
                var data = gSheet.Data.FirstOrDefault();

                if (data == null)
                {
                    continue;
                }

                var sheetName = SheetUtility.ToFileName(gSheet.Properties.Title, i);
                string fileName;

                if (fileNameTransformer != null)
                {
                    fileName = fileNameTransformer.Transform(new ITransformFileName.Args {
                        spreadsheetName = spreadsheetName,
                        sheetName = sheetName,
                    });
                }
                else
                {
                    fileName = sheetName;
                }

                var file = Path.Combine(outputPath, $"{fileName}.csv");

                await using var stream = fileSystem.OpenWrite(file);
                await using var writer = new StreamWriter(stream);
                var rows = data.RowData;

                if (rows == null || rows.Count < 1)
                {
                    continue;
                }

                var rowCount = rows.Count;
                var totalColCount = CountColumns(rows);

                if (totalColCount < 1)
                {
                    continue;
                }

                var csv = new CsvWriter(writer);

                for (var r = 0; r < rowCount; r++)
                {
                    var row = rows[r];
                    var cols = row.Values;

                    if (cols == null || cols.Count < 1)
                    {
                        continue;
                    }

                    var colCount = cols.Count;

                    for (var c = 0; c < colCount; c++)
                    {
                        csv.WriteField(cols[c]?.FormattedValue);
                    }

                    var emptyCount = totalColCount - colCount;

                    for (var c = 0; c <= emptyCount; c++)
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

        public interface ITransformFileName
        {
            string Transform(Args args);

            public struct Args
            {
                public string spreadsheetName;
                public string sheetName;
            }
        }
    }
}
