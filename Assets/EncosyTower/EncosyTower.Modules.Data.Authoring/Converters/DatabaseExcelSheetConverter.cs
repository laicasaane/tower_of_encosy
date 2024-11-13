// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Raw;
using ExcelDataReader;

namespace EncosyTower.Modules.Data.Authoring
{
    public class DatabaseExcelSheetConverter : DatabaseRawSheetImporter
    {
        private readonly string _loadPath;
        private readonly string _extension;
        private readonly Dictionary<string, List<Page>> _pages;
        private readonly IFileSystem _fileSystem;
        private readonly bool _includeCommentedFiles;

        public DatabaseExcelSheetConverter(
              string loadPath
            , TimeZoneInfo timeZoneInfo = null
            , string extension = "xlsx"
            , IFileSystem fileSystem = null
            , IFormatProvider formatProvider = null
            , int emptyRowStreakThreshold = 5
            , bool includeCommentedFiles = false
        )
            : base(timeZoneInfo, formatProvider, emptyRowStreakThreshold)
        {
            _loadPath = loadPath;
            _extension = extension;
            _fileSystem = fileSystem ?? new FileSystem();
            _pages = new Dictionary<string, List<Page>>();
            _includeCommentedFiles = includeCommentedFiles;

#if NET5_0_OR_GREATER
            // https://github.com/ExcelDataReader/ExcelDataReader?tab=readme-ov-file#important-note-on-net-core
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
        }

        public override void Reset()
        {
            base.Reset();
            _pages.Clear();
        }

        protected override Task<bool> LoadData()
        {
            var files = _fileSystem.GetFiles(_loadPath, _extension);

            _pages.Clear();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                if (fileName.StartsWith("~$"))
                {
                    continue;
                }

                if (SheetUtility.ValidateSheetName(fileName, _includeCommentedFiles) == false)
                {
                    continue;
                }

                using var stream = _fileSystem.OpenRead(file);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var dataset = reader.AsDataSet(new ExcelDataSetConfiguration {
                    UseColumnDataType = false,
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration {
                        UseHeaderRow = false,
                    }
                });

                for (var i = 0; i < dataset.Tables.Count; ++i)
                {
                    var table = dataset.Tables[i];
                    var tableName = table.TableName;

                    if (SheetUtility.ValidateSheetName(tableName) == false)
                    {
                        continue;
                    }

                    var (sheetName, subName) = Config.ParseSheetName(tableName);

                    if (_pages.TryGetValue(sheetName, out var sheetList) == false)
                    {
                        sheetList = new List<Page>();
                        _pages.Add(sheetName, sheetList);
                    }

                    sheetList.Add(new Page(table, subName, FormatProvider));
                }
            }

            return Task.FromResult(true);
        }

        protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
        {
            return _pages.TryGetValue(sheetName, out var page)
                ? page
                : Enumerable.Empty<IRawSheetImporterPage>();
        }

        private class Page : IRawSheetImporterPage
        {
            private readonly DataTable _table;
            private readonly IFormatProvider _formatProvider;

            public string SubName { get; }

            public Page(DataTable table, string subName, IFormatProvider formatProvider)
            {
                _table = table;
                _formatProvider = formatProvider;
                SubName = subName;
            }

            public string GetCell(int col, int row)
            {
                if (col >= _table.Columns.Count || row >= _table.Rows.Count)
                {
                    return null;
                }

                return Convert.ToString(_table.Rows[row][col], _formatProvider);
            }
        }
    }
}
