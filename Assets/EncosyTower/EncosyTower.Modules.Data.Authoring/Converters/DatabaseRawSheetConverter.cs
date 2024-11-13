// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Raw;
using Microsoft.Extensions.Logging;

namespace EncosyTower.Modules.Data.Authoring
{
    /// <summary>
    /// Generic sheet converter for cell-based Spreadsheet sources.
    /// </summary>
    public abstract class DatabaseRawSheetConverter : DatabaseRawSheetImporter, ISheetConverter
    {
        public bool SplitHeader { get; set; }

        protected abstract Task<bool> SaveData();

        protected abstract IRawSheetExporterPage CreatePage(string sheetName);

        protected DatabaseRawSheetConverter(
              TimeZoneInfo timeZoneInfo
            , IFormatProvider formatProvider
            , bool splitHeader = false
            , int emptyRowStreakThreshold = 5
        )
            : base(timeZoneInfo, formatProvider, emptyRowStreakThreshold)
        {
            SplitHeader = splitHeader;
        }

        public async Task<bool> Export(SheetConvertingContext context)
        {
            var sheetProperties = context.Container.GetSheetProperties();
            var dataSheetContainer = context.Container as DataSheetContainerBase;

            foreach (var pair in sheetProperties)
            {
                var ignored = dataSheetContainer?.CheckSheetPropertyIsIgnored(pair.Key) ?? false;

                if (ignored)
                {
                    continue;
                }

                using (context.Logger.BeginScope(pair.Key))
                {
                    if (pair.Value.GetValue(context.Container) is not ISheet sheet)
                    {
                        continue;
                    }

                    var page = CreatePage(sheet.Name);
                    ExportPage(page, context, sheet);
                }
            }

            var success = await SaveData();

            if (success == false)
            {
                context.Logger.LogError("Failed to save data");
                return false;
            }

            return true;
        }

        private void ExportPage(IRawSheetExporterPage page, SheetConvertingContext context, ISheet sheet)
        {
            var propertyMap = sheet.GetPropertyMap(context);
            var resolver = context.Container.ContractResolver;

            propertyMap.UpdateIndex(sheet);

            var leafs = propertyMap.TraverseLeaf();
            var pageColumn = 0;
            var valueContext = new SheetValueConvertingContext(this, resolver);
            var headerRows = new List<string>();
            var arguments = new object[propertyMap.MaxDepth];

            foreach (var (node, indexes) in leafs)
            {
                var i = 0;

                foreach (var index in indexes)
                {
                    var arg = valueContext.ValueToString(index.GetType(), index);
                    arguments[i++] = arg;
                }

                var columnName = string.Format(node.FullPath, arguments);

                if (SplitHeader)
                {
                    var tempRow = 0;

                    foreach (var path in columnName.Split(Config.IndexDelimiterArray, StringSplitOptions.None))
                    {
                        while (headerRows.Count <= tempRow)
                            headerRows.Add(null);

                        if (headerRows[tempRow] != path)
                        {
                            headerRows[tempRow] = path;
                            page.SetCell(pageColumn, tempRow, path);
                        }

                        tempRow++;
                    }
                }
                else
                {
                    page.SetCell(pageColumn, 0, columnName);
                }

                pageColumn++;
            }

            var pageRow = SplitHeader ? headerRows.Count : 1;

            foreach (ISheetRow sheetRow in sheet)
            {
                var maxVerticalCount = 1;

                pageColumn = 0;

                foreach (var (node, indexes) in leafs)
                {
                    var verticalCount = node.GetVerticalCount(sheetRow, indexes.GetEnumerator());

                    for (var vIndex = 0; vIndex < verticalCount; ++vIndex)
                    {
                        var value = node.GetValue(sheetRow, vIndex, indexes.GetEnumerator());

                        string valueString = null;

                        if (value != null)
                        {
                            valueString = node.ValueConverter.ValueToString(node.ValueType, value, valueContext);
                        }

                        page.SetCell(pageColumn, pageRow + vIndex, valueString);
                    }

                    if (maxVerticalCount < verticalCount)
                    {
                        maxVerticalCount = verticalCount;
                    }

                    pageColumn++;
                }

                pageRow += maxVerticalCount;
            }
        }
    }
}
