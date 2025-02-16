using System;
using System.Collections.Generic;
using EncosyTower.Settings;
using UnityEngine;

namespace EncosyTower.Editor.Data.Settings
{
    [Settings(SettingsUsage.EditorProject)]
    public partial class DatabaseCollectionSettings : Settings<DatabaseCollectionSettings>
    {
        public const DataSourceFlags ALL_SOURCES
                = DataSourceFlags.GoogleSheet
                | DataSourceFlags.Csv
                | DataSourceFlags.Excel;

        [SerializeField] internal List<DatabaseSettings> _databases = new();

        [Serializable]
        internal partial class DatabaseSettings : SubSettings
        {
            public string name;
            public string type;
            public string assetName;
            public GoogleSheetSettings googleSheetSettings = new();
            public LocalCsvFolderSettings csvSettings = new();
            public LocalExcelFolderSettings excelSettings = new();
        }

        [Serializable]
        internal partial class GoogleSheetSettings
        {
            public string serviceAccountRelativeFilePath = string.Empty;
            public string spreadsheetId = string.Empty;
            public string outputRelativeFolderPath = string.Empty;
            public OutputFileType outputFileType = default;
            public bool enabled = false;
            public bool cleanOutputFolder = true;
            public bool alwaysDownloadAll = false;
        }

        [Serializable]
        internal abstract partial class LocalFolderSettings
        {
            public string inputRelativeFolderPath = string.Empty;
            public string outputRelativeFolderPath = string.Empty;
            public bool enabled = false;
            public bool liveConversion = true;
        }

        [Serializable]
        internal sealed partial class LocalCsvFolderSettings : LocalFolderSettings
        {
        }

        [Serializable]
        internal sealed partial class LocalExcelFolderSettings : LocalFolderSettings
        {
        }

        internal enum OutputFileType
        {
            DataTable = 0,
            Csv,
        }
    }
}
