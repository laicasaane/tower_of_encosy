using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using EncosyTower.Databases.Authoring.SourceGen;
using EncosyTower.UnityExtensions;
using Microsoft.Extensions.Logging;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Databases.Authoring
{
    public class DatabaseAssetExporter : DatabaseAssetExporter<DatabaseAsset>
    {
        public DatabaseAssetExporter(string savePath, string databaseName = nameof(DatabaseAsset))
            : base(savePath, databaseName)
        { }
    }

    public class DatabaseAssetExporter<TDatabaseAsset> : ISheetExporter, ISheetFormatter
        where TDatabaseAsset : DatabaseAsset
    {
        private readonly string _savePath;
        private readonly string _databaseName;

        /// <param name="savePath">The location to store the exported data table assets</param>
        /// <param name="databaseName">The name of the exported database asset</param>
        public DatabaseAssetExporter(string savePath, string databaseName)
        {
            _savePath = savePath;
            _databaseName = databaseName;
        }

        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;

        public IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

        public TDatabaseAsset Result { get; private set; }

        public Task<bool> Export(SheetConvertingContext context)
        {
            var savePath = MakeFolderPath(_savePath);

            GenerateDatabaseAsset(
                  context
                , savePath
                , _databaseName
                , out var databaseAsset
                , out var dataTableAssets
            );

            SaveAsset(databaseAsset, dataTableAssets);
            Result = databaseAsset;

            return Task.FromResult(true);
        }

        private static string MakeFolderPath(string savePath)
        {
            if (AssetDatabase.IsValidFolder(savePath) == false)
            {
                savePath = AssetDatabase.CreateFolder(
                      Path.GetDirectoryName(savePath)
                    , Path.GetFileName(savePath)
                );
            }

            return savePath;
        }

        private static void GenerateDatabaseAsset(
              SheetConvertingContext context
            , string savePath
            , string databaseName
            , out TDatabaseAsset databaseAsset
            , out List<DataTableAssetBase> dataTableAssetList
        )
        {
            var databaseAssetPath = Path.Combine(savePath, $"{databaseName}.asset");

            databaseAsset = AssetDatabase.LoadAssetAtPath<TDatabaseAsset>(databaseAssetPath);

            if (databaseAsset == false)
            {
                databaseAsset = ScriptableObject.CreateInstance<TDatabaseAsset>();
                AssetDatabase.CreateAsset(databaseAsset, databaseAssetPath);
            }

            var redundantAssets = new HashSet<DataTableAssetBase>();

            foreach (var table in databaseAsset._tables)
            {
                redundantAssets.Add(table);
            }

            foreach (var table in databaseAsset._redundantTabless)
            {
                redundantAssets.Add(table);
            }

            databaseAsset.Clear();
            dataTableAssetList = new List<DataTableAssetBase>();

            var sheetProperties = context.Container.GetSheetProperties();
            var dataSheetContainer = context.Container as DataSheetContainerBase;

            foreach (var pair in sheetProperties)
            {
                using (context.Logger.BeginScope(pair.Key))
                {
                    var ignored = dataSheetContainer?.CheckSheetPropertyIsIgnored(pair.Key) ?? false;

                    if (ignored)
                    {
                        if (TryGetGeneratedSheetAttribute(context, sheetProperties, pair.Key, out var sheetAttrib) == false)
                        {
                            continue;
                        }

                        var dataTableAssetName = sheetAttrib.DataTableAssetName;
                        var dataTableAssetPath = Path.Combine(savePath, $"{dataTableAssetName}.asset");
                        var dataTableAsset = AssetDatabase.LoadAssetAtPath<DataTableAssetBase>(dataTableAssetPath);

                        if (dataTableAsset == null)
                        {
                            continue;
                        }

                        dataTableAsset.name = dataTableAssetName;
                        redundantAssets.Remove(dataTableAsset);
                        dataTableAssetList.Add(dataTableAsset);
                    }
                    else
                    {
                        if (pair.Value.GetValue(context.Container) is not ISheet sheet)
                        {
                            continue;
                        }

                        if (TryGetGeneratedSheetAttribute(context, sheet, out var sheetAttrib) == false
                            || TryGetToDataArrayMethod(context, sheet, sheetAttrib.DataType, out var toDataArrayMethod) == false
                        )
                        {
                            continue;
                        }

                        var dataTableAssetType = sheetAttrib.DataTableAssetType;
                        var dataTableAssetName = sheetAttrib.DataTableAssetName;
                        var dataTableAssetPath = Path.Combine(savePath, $"{dataTableAssetName}.asset");
                        var dataTableAsset = AssetDatabase.LoadAssetAtPath<DataTableAssetBase>(dataTableAssetPath);

                        if (dataTableAsset.IsInvalid())
                        {
                            dataTableAsset = ScriptableObject.CreateInstance(dataTableAssetType) as DataTableAssetBase;
                            AssetDatabase.CreateAsset(dataTableAsset, dataTableAssetPath);
                        }

                        dataTableAsset.name = dataTableAssetName;

                        redundantAssets.Remove(dataTableAsset);

                        var dataArray = toDataArrayMethod.Invoke(sheet, null);
                        dataTableAsset.SetEntries(dataArray);

                        dataTableAssetList.Add(dataTableAsset);
                    }
                }
            }

            databaseAsset.AddRange(dataTableAssetList.ToArray(), redundantAssets.ToArray());
        }

        private static bool TryGetGeneratedSheetAttribute(
              SheetConvertingContext context
            , IReadOnlyDictionary<string, PropertyInfo> sheetProperties
            , string sheetProperty
            , out GeneratedSheetAttribute attribute
        )
        {
            if (sheetProperties.TryGetValue(sheetProperty, out var property))
            {
                var sheetType = property.PropertyType;
                attribute = sheetType.GetCustomAttribute<GeneratedSheetAttribute>();

                if (attribute != null)
                {
                    return true;
                }

                context.Logger.LogError("Cannot find {Attribute} on {Sheet}", typeof(GeneratedSheetAttribute), sheetType);
                return false;
            }

            attribute = default;
            return false;
        }

        private static bool TryGetGeneratedSheetAttribute(
              SheetConvertingContext context
            , ISheet sheet
            , out GeneratedSheetAttribute attribute
        )
        {
            var sheetType = sheet.GetType();
            attribute = sheetType.GetCustomAttribute<GeneratedSheetAttribute>();

            if (attribute == null)
            {
                context.Logger.LogError("Cannot find {Attribute} on {Sheet}", typeof(GeneratedSheetAttribute), sheetType);
                return false;
            }

            return true;
        }

        private static bool TryGetToDataArrayMethod(
              SheetConvertingContext context
            , ISheet sheet
            , Type dataType
            , out MethodInfo toDataArrayMethod
        )
        {
            var sheetType = sheet.GetType();
            var methodName = $"To{dataType.Name}Array";

            toDataArrayMethod = sheetType.GetMethod(methodName, Type.EmptyTypes);

            if (toDataArrayMethod != null)
            {
                return true;
            }

            context.Logger.LogError("Cannot find {MethodName} method in {SheetType}", methodName, sheetType);
            return false;
        }

        private static void SaveAsset(
              TDatabaseAsset databaseAsset
            , List<DataTableAssetBase> dataTableAssets
        )
        {
            EditorUtility.SetDirty(databaseAsset);

            foreach (var asset in dataTableAssets)
            {
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
