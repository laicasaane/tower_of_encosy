// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Raw;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using GSheet = Google.Apis.Sheets.v4.Data.Sheet;

namespace EncosyTower.Databases.Authoring
{
    public class DatabaseGoogleSheetConverter : DatabaseRawSheetImporter
    {
        public static readonly string[] Scopes = new[] { DriveService.Scope.DriveReadonly };

        public readonly string SpreadsheetId;

        private readonly string _applicationName;
        private readonly ICredential _credential;
        private readonly BaseClientService.Initializer _initializer;
        private readonly Dictionary<string, List<Page>> _pages;

        private Spreadsheet _spreadsheet;

        public DatabaseGoogleSheetConverter(
              [NotNull] string spreadsheetId
            , [NotNull] string credential
            , [NotNull] string applicationName
            , TimeZoneInfo timeZoneInfo = null
            , IFormatProvider formatProvider = null
            , int emptyRowStreakThreshold = 5
        )
            : base(timeZoneInfo, formatProvider, emptyRowStreakThreshold)
        {
            SpreadsheetId = spreadsheetId;

            _applicationName = applicationName;
            _credential = GoogleCredential
                .FromJson(credential)
                .CreateScoped(Scopes);

            _pages = new Dictionary<string, List<Page>>();
        }

        public DatabaseGoogleSheetConverter(
              [NotNull] string spreadsheetId
            , [NotNull] BaseClientService.Initializer initializer
            , TimeZoneInfo timeZoneInfo = null
            , IFormatProvider formatProvider = null
            , int emptyRowStreakThreshold = 5
        )
            : base(timeZoneInfo, formatProvider, emptyRowStreakThreshold)
        {
            SpreadsheetId = spreadsheetId;

            _initializer = initializer;
            _pages = new Dictionary<string, List<Page>>();
        }

        public async Task<GoogleFileMetadata> FetchMetadata()
        {
            using var service = new DriveService(_initializer ?? new BaseClientService.Initializer {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            });

            var fileReq = service.Files.Get(SpreadsheetId);
            fileReq.SupportsTeamDrives = true;
            fileReq.Fields = "name,modifiedTime";

            var file = await fileReq.ExecuteAsync();
            return new(file.Name, file.ModifiedTime ?? default);
        }

        public override void Reset()
        {
            base.Reset();
            _pages.Clear();
        }

        protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
        {
            return _pages.TryGetValue(sheetName, out var pages)
                ? pages
                : Enumerable.Empty<IRawSheetImporterPage>();
        }

        protected override async Task<bool> LoadData()
        {
            using (var service = new SheetsService(_initializer ?? new BaseClientService.Initializer() {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            }))
            {
                var sheetReq = service.Spreadsheets.Get(SpreadsheetId);
                sheetReq.Fields = "properties,sheets(properties,data.rowData.values.formattedValue)";
                _spreadsheet = await sheetReq.ExecuteAsync();
            }

            _pages.Clear();

            foreach (var gSheet in _spreadsheet.Sheets)
            {
                if (SheetUtility.ValidateSheetName(gSheet.Properties.Title) == false)
                {
                    continue;
                }

                var (sheetName, subName) = Config.ParseSheetName(gSheet.Properties.Title);

                if (_pages.TryGetValue(sheetName, out var sheetList) == false)
                {
                    sheetList = new List<Page>();
                    _pages.Add(sheetName, sheetList);
                }

                sheetList.Add(new Page(gSheet, subName));
            }

            return true;
        }

        private class Page : IRawSheetImporterPage
        {
            private readonly GridData _grid;

            public string SubName { get; }

            public Page(GSheet gSheet, string subName)
            {
                _grid = gSheet.Data.First();
                SubName = subName;
            }

            public string GetCell(int col, int row)
            {
                if (row >= _grid.RowData.Count || col >= _grid.RowData[row].Values?.Count)
                {
                    return null;
                }

                var value = _grid.RowData[row].Values?[col];
                return value?.FormattedValue;
            }
        }
    }
}
