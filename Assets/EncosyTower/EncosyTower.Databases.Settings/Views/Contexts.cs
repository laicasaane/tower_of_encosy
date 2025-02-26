using UnityEditor;

namespace EncosyTower.Databases.Settings.Views
{
    using DatabaseSettings = DatabaseCollectionSettings.DatabaseSettings;
    using GoogleSheetSettings = DatabaseCollectionSettings.GoogleSheetSettings;
    using LocalFolderSettings = DatabaseCollectionSettings.LocalFolderSettings;

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
        public SerializedProperty GetServiceAccountRelativeFilePathProperty()
            => Property.FindPropertyRelative(nameof(GoogleSheetSettings.serviceAccountRelativeFilePath));

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
    }
}
