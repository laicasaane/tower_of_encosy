using UnityEditor;

namespace EncosyTower.Databases.Settings.Views
{
    using DatabaseSettings = DatabaseCollectionSettings.DatabaseSettings;
    using GoogleSheetSettings = DatabaseCollectionSettings.GoogleSheetSettings;
    using LocalFolderSettings = DatabaseCollectionSettings.LocalFolderSettings;
    using LocalCsvFolderSettings = DatabaseCollectionSettings.LocalCsvFolderSettings;

    internal class DatabaseSettingsContext
    {
        public DatabaseSettings DatabaseSettings { get; private set; }

        public SerializedObject SerializedObject { get; private set; }

        public DatabaseContext Database { get; private set; }

        public GoogleSheetContext GoogleSheet { get; private set; }

        public LocalFolderContext Csv { get; private set; }

        public LocalFolderContext Excel { get; private set; }

        public void Initialize(DatabaseSettings databaseSettings, SerializedProperty databaseProperty)
        {
            SerializedObject = databaseProperty.serializedObject;

            var dbSettings = DatabaseSettings = databaseSettings;

            Database = new(databaseProperty, dbSettings);

            {
                var prop = databaseProperty.FindPropertyRelative(nameof(DatabaseSettings.googleSheetSettings));
                GoogleSheet = new(prop, dbSettings);
            }

            {
                var prop = databaseProperty.FindPropertyRelative(nameof(DatabaseSettings.csvSettings));
                Csv = new(prop, dbSettings);
            }

            {
                var prop = databaseProperty.FindPropertyRelative(nameof(DatabaseSettings.excelSettings));
                Excel = new(prop, dbSettings);
            }
        }
    }

    internal readonly record struct DatabaseContext(SerializedProperty Property, DatabaseSettings Settings)
    {
        public SerializedProperty GetNameProperty()
            => Property.FindPropertyRelative(nameof(DatabaseSettings.name));

        public SerializedProperty GetAuthorTypeProperty()
            => Property.FindPropertyRelative(nameof(DatabaseSettings.authorType));

        public SerializedProperty GetDatabaseTypeProperty()
            => Property.FindPropertyRelative(nameof(DatabaseSettings.databaseType));
    }

    internal readonly record struct GoogleSheetContext(
          SerializedProperty Property
        , DatabaseSettings DatabaseSettings
    )
    {
        public SerializedProperty GetAuthenticationProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.authentication));

        public SerializedProperty GetCredentialRelativeFilePathProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.credentialRelativeFilePath));

        public SerializedProperty GetApiKeyRelativeFilePathProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.apiKeyRelativeFilePath));

        public SerializedProperty GetCredentialTokenRelativeFolderPathProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.credentialTokenRelativeFolderPath));

        public SerializedProperty GetSpreadsheetIdProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.spreadsheetId));

        public SerializedProperty GetOutputRelativeFolderPathProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.outputRelativeFolderPath));

        public SerializedProperty GetOutputFileTypeProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.outputFileType));

        public SerializedProperty GetEnabledProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.enabled));

        public SerializedProperty GetCleanOutputFolderProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.cleanOutputFolder));

        public SerializedProperty GetAlwaysDownloadAllProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.alwaysDownloadAll));

        public SerializedProperty GetEmptyRowStreakThresholdProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.emptyRowStreakThreshold));
    }

    internal readonly record struct LocalFolderContext(
          SerializedProperty Property
        , DatabaseSettings DatabaseSettings
    )
    {
        public SerializedProperty GetInputRelativeFolderPathProperty()
            => Property.FindPropertyRelative(nameof(LocalFolderSettings.inputRelativeFolderPath));

        public SerializedProperty GetOutputRelativeFolderPathProperty()
            => Property.FindPropertyRelative(nameof(LocalFolderSettings.outputRelativeFolderPath));

        public SerializedProperty GetEnabledProperty()
            => Property.FindPropertyRelative(nameof(LocalFolderSettings.enabled));

        public SerializedProperty GetLiveConversionProperty()
            => Property.FindPropertyRelative(nameof(LocalFolderSettings.liveConversion));

        public SerializedProperty GetEmptyRowStreakThresholdProperty()
            => Property.FindPropertyRelative(nameof(LocalFolderSettings.emptyRowStreakThreshold));

        public SerializedProperty GetIncludeSubFoldersProperty()
            => Property.FindPropertyRelative(nameof(LocalFolderSettings.includeSubFolders));

        public SerializedProperty GetIncludeCommentedFilesProperty()
            => Property.FindPropertyRelative(nameof(LocalFolderSettings.includeCommentedFiles));

        public SerializedProperty GetSplitHeaderProperty()
            => Property.FindPropertyRelative(nameof(LocalCsvFolderSettings.splitHeader));

        public SerializedProperty GetExtensionProperty()
            => Property.FindPropertyRelative(nameof(LocalCsvFolderSettings.extension));
    }
}
