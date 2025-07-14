namespace EncosyTower.Databases.Settings.Views
{
    using LocalFolderSettings = DatabaseCollectionSettings.LocalFolderSettings;

    internal partial class DatabaseCollectionSettingsEditor
    {
        private void CopyFromPreset(Context destContext, DatabaseSettingsPreset preset)
        {
            var database = preset._database;

            // DatabaseSettings
            {
                var context = destContext.Database;
                var settings = database;

                {
                    var prop = context.GetNameProperty();
                    prop.stringValue = settings.name;
                }

                {
                    var prop = context.GetAuthorTypeProperty();
                    prop.stringValue = settings.authorType;
                }

                {
                    var prop = context.GetDatabaseTypeProperty();
                    prop.stringValue = settings.databaseType;
                }
            }

            // GoogleSheetSettings
            {
                var context = destContext.GoogleSheet;
                var settings = database.googleSheetSettings;

                {
                    var prop = context.GetAuthenticationProperty();
                    prop.enumValueIndex = (int)settings.authentication;
                }

                {
                    var prop = context.GetCredentialRelativeFilePathProperty();
                    prop.stringValue = settings.credentialRelativeFilePath;
                }

                {
                    var prop = context.GetApiKeyRelativeFilePathProperty();
                    prop.stringValue = settings.apiKeyRelativeFilePath;
                }

                {
                    var prop = context.GetCredentialTokenRelativeFolderPathProperty();
                    prop.stringValue = settings.credentialTokenRelativeFolderPath;
                }

                {
                    var prop = context.GetSpreadsheetIdProperty();
                    prop.stringValue = settings.spreadsheetId;
                }

                {
                    var prop = context.GetOutputRelativeFolderPathProperty();
                    prop.stringValue = settings.outputRelativeFolderPath;
                }

                {
                    var prop = context.GetOutputFileTypeProperty();
                    prop.enumValueIndex = (int)settings.outputFileType;
                }

                {
                    var prop = context.GetEnabledProperty();
                    prop.boolValue = settings.enabled;
                }

                {
                    var prop = context.GetCleanOutputFolderProperty();
                    prop.boolValue = settings.cleanOutputFolder;
                }

                {
                    var prop = context.GetAlwaysDownloadAllProperty();
                    prop.boolValue = settings.alwaysDownloadAll;
                }
            }

            Copy(destContext.Csv, database.csvSettings);
            Copy(destContext.Excel, database.excelSettings);

            return;

            static void Copy(LocalFolderContext context, LocalFolderSettings settings)
            {
                {
                    var prop = context.GetInputRelativeFolderPathProperty();
                    prop.stringValue = settings.inputRelativeFolderPath;
                }
                {
                    var prop = context.GetOutputRelativeFolderPathProperty();
                    prop.stringValue = settings.outputRelativeFolderPath;
                }
                {
                    var prop = context.GetEnabledProperty();
                    prop.boolValue = settings.enabled;
                }
                {
                    var prop = context.GetLiveConversionProperty();
                    prop.boolValue = settings.liveConversion;
                }
            }
        }
    }
}
