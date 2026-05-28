// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet;
using EncosyTower.Collections;
using Microsoft.Extensions.Logging;

namespace EncosyTower.Databases.Authoring
{
    public abstract class DataSheetContainerBase : SheetContainerBase
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _ignoredSheetProperties;

        protected DataSheetContainerBase(ILogger logger) : base(logger)
        {
            _logger = logger;
            _ignoredSheetProperties = new();
        }

        public ILogger Logger => _logger;

        public HashSetReadOnly<string> IgnoredSheetProperties => _ignoredSheetProperties;

        public void IgnoreSheetProperties(IEnumerable<string> properties)
        {
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    _ignoredSheetProperties.Add(prop);
                }
            }
        }

        public bool CheckSheetPropertyIsIgnored(string property)
        {
            if (string.IsNullOrWhiteSpace(property)) return true;
            return _ignoredSheetProperties.Contains(property);
        }

        public override void PostLoad()
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            var properties = GetSheetProperties();
            var rowTypeToSheet = new Dictionary<Type, ISheet>(properties.Count);
            var dataSheets = new List<IDataSheet>(properties.Count);

            foreach (var pair in properties)
            {
                if (CheckSheetPropertyIsIgnored(pair.Key))
                {
                    continue;
                }

                if (pair.Value.GetValue(this) is not ISheet sheet)
                {
                    context.Logger.LogError("Failed to find sheet: {SheetName}", pair.Key);
                    continue;
                }

                sheet.Name = pair.Key;

                if (rowTypeToSheet.TryAdd(sheet.RowType, sheet))
                {
                    if (sheet is IDataSheet dataSheet)
                    {
                        dataSheets.Add(dataSheet);
                    }
                }
                else
                {
                    // row type must be unique in a sheet container
                    context.Logger.LogError("Duplicated Row type is used for {SheetName}", pair.Key);
                }
            }

            OnBeforePostLoad(context);

            foreach (var sheet in dataSheets)
            {
                sheet.Initialize(context);
            }

            // making sure all references are mapped before calling PostLoad
            foreach (var sheet in rowTypeToSheet.Values)
            {
                sheet.MapReferences(context, rowTypeToSheet);
            }

            foreach (var sheet in rowTypeToSheet.Values)
            {
                sheet.PostLoad(context);
            }

            OnAfterPostLoad(context);
        }

        protected virtual void OnBeforePostLoad(SheetConvertingContext context) { }

        protected virtual void OnAfterPostLoad(SheetConvertingContext context) { }
    }
}
