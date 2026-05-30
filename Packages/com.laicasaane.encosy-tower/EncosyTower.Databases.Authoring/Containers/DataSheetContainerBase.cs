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

            OnBeforeMapReferences(context);

            // making sure all references are mapped before calling PostLoad
            foreach (var sheet in rowTypeToSheet.Values)
            {
                sheet.MapReferences(context, rowTypeToSheet);
            }

            OnAfterMapReferences(context);
            OnBeforePostLoad(context);

            foreach (var sheet in rowTypeToSheet.Values)
            {
                sheet.PostLoad(context);
            }

            OnAfterPostLoad(context);
            OnBeforePreprocess(context);

            foreach (var sheet in dataSheets)
            {
                sheet.Preprocess(context);
            }

            OnAfterPreprocess(context);
            OnBeforeProcess(context);

            foreach (var sheet in dataSheets)
            {
                sheet.Process(context);
            }

            OnAfterProcess(context);
            OnBeforePostprocess(context);

            foreach (var sheet in dataSheets)
            {
                sheet.Postprocess(context);
            }

            OnAfterPostprocess(context);
        }

        /// <summary>
        /// Callback invoked before all <c>sheet.MapReferences</c>.
        /// </summary>
        protected virtual void OnBeforeMapReferences(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.MapReferences</c>.
        /// </summary>
        protected virtual void OnAfterMapReferences(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.PostLoad</c>.
        /// </summary>
        protected virtual void OnBeforePostLoad(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.PostLoad</c>.
        /// </summary>
        protected virtual void OnAfterPostLoad(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.Preprocess</c>.
        /// </summary>
        protected virtual void OnBeforePreprocess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.Preprocess</c>.
        /// </summary>
        protected virtual void OnAfterPreprocess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.Process</c>.
        /// </summary>
        protected virtual void OnBeforeProcess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.Process</c>.
        /// </summary>
        protected virtual void OnAfterProcess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.Postprocess</c>.
        /// </summary>
        protected virtual void OnBeforePostprocess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.Postprocess</c>.
        /// </summary>
        protected virtual void OnAfterPostprocess(SheetConvertingContext context) { }
    }
}
