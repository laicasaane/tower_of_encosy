using System;
using System.Collections.Generic;
using EncosyTower.EnumExtensions;
using EncosyTower.Settings;
using UnityEngine;

namespace EncosyTower.Databases.Settings
{
    [Settings(SettingsUsage.EditorProject, "Encosy Tower/Database Collection")]
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
            public string authorType;
            public string databaseType;
            public string assetName;
            public GoogleSheetSettings googleSheetSettings = new();
            public LocalCsvFolderSettings csvSettings = new();
            public LocalExcelFolderSettings excelSettings = new();
        }

        [Serializable]
        internal partial class GoogleSheetSettings
        {
            public string credentialRelativeFilePath = string.Empty;
            public string apiKeyRelativeFilePath = string.Empty;
            public string credentialTokenRelativeFolderPath = string.Empty;
            public string spreadsheetId = string.Empty;
            public string outputRelativeFolderPath = string.Empty;
            public AuthenticationType authentication = default;
            public OutputFileType outputFileType = default;
            public bool enabled = false;
            public bool cleanOutputFolder = true;
            public bool alwaysDownloadAll = false;
            public int emptyRowStreakThreshold = 5;
        }

        [Serializable]
        internal abstract partial class LocalFolderSettings
        {
            public string inputRelativeFolderPath = string.Empty;
            public string outputRelativeFolderPath = string.Empty;
            public bool enabled = false;
            public bool liveConversion = true;
            public int emptyRowStreakThreshold = 5;
            public bool includeSubFolders = true;
            public bool includeCommentedFiles = false;

            public static string GetProgressTitle(DataSourceFlags source)
            {
                return source switch {
                    DataSourceFlags.Csv => LocalCsvFolderSettings.PROGRESS_TITLE,
                    DataSourceFlags.Excel => LocalExcelFolderSettings.PROGRESS_TITLE,
                    _ => "Converting Local Folder"
                };
            }
        }

        [Serializable]
        internal sealed partial class LocalCsvFolderSettings : LocalFolderSettings
        {
            public bool splitHeader = false;
            public string extension = "csv";
        }

        [Serializable]
        internal sealed partial class LocalExcelFolderSettings : LocalFolderSettings
        {
            public string extension = "xlsx";
        }

        internal enum AuthenticationType
        {
            [InspectorName("OAuth 2.0")]
            OAuth = 0,

            [InspectorName("API Key")]
            ApiKey,
        }

        internal enum OutputFileType
        {
            [InspectorName("Data Table Asset")]
            DataTableAsset = 0,

            [InspectorName("CSV")]
            Csv,
        }
    }

    [EnumExtensionsFor(typeof(DatabaseCollectionSettings.OutputFileType))]
    internal static partial class OutputFileTypeExtensions { }
}
