// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Raw;
using NReco.Csv;

namespace EncosyTower.Modules.Data.Authoring
{
    public class DatabaseCsvSheetConverter : DatabaseRawSheetConverter
    {
        private readonly Dictionary<string, List<Page>> _pages = new();
        private readonly IExtendedFileSystem _fileSystem;
        private readonly string _loadPath;
        private readonly string _extension;
        private readonly bool _includeSubFolders;
        private readonly bool _includeCommentedFiles;

        public DatabaseCsvSheetConverter(
              string loadPath
            , TimeZoneInfo timeZoneInfo = null
            , string extension = "csv"
            , IExtendedFileSystem fileSystem = null
            , bool splitHeader = false
            , IFormatProvider formatProvider = null
            , int emptyRowStreakThreshold = 5
            , bool includeSubFolders = true
            , bool includeCommentedFiles = false
        )
            : base(timeZoneInfo, formatProvider, splitHeader, emptyRowStreakThreshold)
        {
            _loadPath = loadPath;
            _extension = extension;
            _fileSystem = fileSystem ?? new DatabaseFileSystem();
            _includeSubFolders = includeSubFolders;
            _includeCommentedFiles = includeCommentedFiles;
        }

        private class CsvTable : List<List<string>>
        {
            public List<string> AddRow()
            {
                var row = new List<string>();
                Add(row);
                return row;
            }
        }

        public override void Reset()
        {
            base.Reset();
            _pages.Clear();
        }

        protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
        {
            if (_pages.TryGetValue(sheetName, out var pages))
                return pages;

            return Enumerable.Empty<IRawSheetImporterPage>();
        }

        protected override IRawSheetExporterPage CreatePage(string sheetName)
        {
            var page = new Page(new CsvTable(), null);
            _pages[sheetName] = new List<Page> { page };
            return page;
        }

        protected override Task<bool> LoadData()
        {
            var files = _fileSystem.GetFiles(_loadPath, _extension, _includeSubFolders);

            _pages.Clear();

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                if (SheetUtility.ValidateSheetName(fileName, _includeCommentedFiles) == false)
                {
                    continue;
                }

                if (_includeCommentedFiles)
                {
                    fileName = fileName.Replace("$", "");
                }

                var (sheetName, subName) = Config.ParseSheetName(fileName);

                using var stream = _fileSystem.OpenRead(file);
                using var reader = new StreamReader(stream);
                var csv = new CsvReader(reader);
                var table = new CsvTable();

                while (csv.Read())
                {
                    var row = table.AddRow();

                    for (var i = 0; i < csv.FieldsCount; ++i)
                    {
                        row.Add(csv[i]);
                    }
                }

                if (_pages.TryGetValue(sheetName, out var sheetList) == false)
                {
                    sheetList = new List<Page>();
                    _pages.Add(sheetName, sheetList);
                }

                sheetList.Add(new Page(table, subName));
            }

            return Task.FromResult(true);
        }

        protected override Task<bool> SaveData()
        {
            _fileSystem.CreateDirectory(_loadPath);

            foreach (var pageItem in _pages)
            {
                var file = Path.Combine(_loadPath, $"{pageItem.Key}.{_extension}");

                using var stream = _fileSystem.OpenWrite(file);
                using var writer = new StreamWriter(stream);
                var csv = new CsvWriter(writer);

                foreach (var page in pageItem.Value)
                {
                    foreach (var row in page.Table)
                    {
                        foreach (var cell in row)
                        {
                            csv.WriteField(cell);
                        }

                        csv.NextRecord();
                    }
                }
            }

            return Task.FromResult(true);
        }

        private class Page : IRawSheetImporterPage, IRawSheetExporterPage
        {
            public string SubName { get; }

            public CsvTable Table { get; }

            public Page(CsvTable table, string subName)
            {
                Table = table;
                SubName = subName;
            }

            public string GetCell(int col, int row)
            {
                if (row >= Table.Count)
                {
                    return null;
                }

                return col >= Table[row].Count ? null : Table[row][col];
            }

            public void SetCell(int col, int row, string data)
            {
                for (var i = Table.Count; i <= row; ++i)
                {
                    Table.AddRow();
                }

                for (var i = Table[row].Count; i <= col; ++i)
                {
                    Table[row].Add(null);
                }

                Table[row][col] = data;
            }
        }
    }
}