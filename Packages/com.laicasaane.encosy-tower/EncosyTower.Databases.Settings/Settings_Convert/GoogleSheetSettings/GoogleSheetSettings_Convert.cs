using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Editor;
using EncosyTower.IO;
using EncosyTower.Logging;
using UnityEditor;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            public async Task<bool> ConvertEditorAsync(
                  ConversionArgs args
                , ReportAction reporter
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {
                var rootPath = new RootPath(EditorAPI.ProjectPath);
                var validationResult = Validate(rootPath);

                if (validationResult != ValidationResult.Success)
                {
                    Log(validationResult);
                    return false;
                }

                rootPath.CreateRelativeFolder(outputRelativeFolderPath);

                if (continueOnCapturedContext)
                {
                    AssetDatabase.Refresh();
                }

                switch (outputFileType)
                {
                    case OutputFileType.DataTableAsset:
                    {
                        return await DownloadDataTableEditorAsync(
                              rootPath
                            , args.DatabaseAssetName
                            , args.SheetContainer
                            , reporter
                            , token
                            , continueOnCapturedContext
                        );
                    }

                    case OutputFileType.Csv:
                    {
                        return await DownloadCsvEditorAsync(
                              rootPath
                            , args.DatabaseAssetName
                            , args.SheetContainer
                            , reporter
                            , token
                            , continueOnCapturedContext
                        );
                    }
                }

                return false;
            }

            public async Task<bool> ConvertAsync(
                  ConversionArgs args
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {
                var rootPath = new RootPath(EditorAPI.ProjectPath);
                var validationResult = Validate(rootPath);

                if (validationResult != ValidationResult.Success)
                {
                    Log(validationResult);
                    return false;
                }

                rootPath.CreateRelativeFolder(outputRelativeFolderPath);

                if (continueOnCapturedContext)
                {
                    AssetDatabase.Refresh();
                }

                switch (outputFileType)
                {
                    case OutputFileType.DataTableAsset:
                    {
                        return await DownloadDataTableAsync(
                              rootPath
                            , args.DatabaseAssetName
                            , args.SheetContainer
                            , token
                            , continueOnCapturedContext
                        );
                    }

                    case OutputFileType.Csv:
                    {
                        return await DownloadCsvAsync(
                              rootPath
                            , args.DatabaseAssetName
                            , args.SheetContainer
                            , token
                            , continueOnCapturedContext
                        );
                    }
                }

                return false;
            }

            private ValidationResult Validate(RootPath rootPath)
            {
                if (authentication != AuthenticationType.OAuth
                    && authentication != AuthenticationType.ApiKey
                )
                {
                    return ValidationResult.UnknownAuthentication;
                }

                if (authentication == AuthenticationType.OAuth
                    && rootPath.ExistsRelativeFile(credentialRelativeFilePath) == false
                )
                {
                    return ValidationResult.OAuth2CredentialFileDoesNotExist;
                }

                if (authentication == AuthenticationType.ApiKey
                    && rootPath.ExistsRelativeFile(apiKeyRelativeFilePath) == false
                )
                {
                    return ValidationResult.ApiKeyFileDoesNotExist;
                }

                if (string.IsNullOrWhiteSpace(spreadsheetId))
                {
                    return ValidationResult.SpreadSheetIdIsNullOrInvalid;
                }

                if (string.IsNullOrWhiteSpace(outputRelativeFolderPath))
                {
                    return ValidationResult.OutputFolderIsNullOrInvalid;
                }

                return ValidationResult.Success;
            }

            private void Log(ValidationResult validationResult)
            {
                switch (validationResult)
                {
                    case ValidationResult.UnknownAuthentication:
                        DevLoggerAPI.LogError($"Unknown authentication type: '{authentication}'");
                        break;

                    case ValidationResult.OAuth2CredentialFileDoesNotExist:
                        DevLoggerAPI.LogError($"OAuth 2.0 credential file does not exist: '{credentialRelativeFilePath}'");
                        break;

                    case ValidationResult.ApiKeyFileDoesNotExist:
                        DevLoggerAPI.LogError($"API Key file does not exist: '{apiKeyRelativeFilePath}'");
                        break;

                    case ValidationResult.SpreadSheetIdIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Spreadsheet ID is null or invalid: '{spreadsheetId}'");
                        break;

                    case ValidationResult.OutputFolderIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Output folder is null or invalid: '{outputRelativeFolderPath}'");
                        break;
                }
            }

            private enum ValidationResult
            {
                Success,
                UnknownAuthentication,
                OAuth2CredentialFileDoesNotExist,
                ApiKeyFileDoesNotExist,
                SpreadSheetIdIsNullOrInvalid,
                OutputFolderIsNullOrInvalid,
            }
        }
    }
}
