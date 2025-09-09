using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Databases.Authoring;
using EncosyTower.IO;
using EncosyTower.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using UnityEngine;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            public const string PROGRESS_TITLE = "Convert Google SpreadSheet";

            public string ProgressTitle => PROGRESS_TITLE;

            private bool Prepare(
                  RootPath rootPath
                , string databaseAssetName
                , out string tokenFolderPath
                , out string credentialText
                , out ClientSecrets secrets
                , out TimeZoneInfo timeZone
                , out FileDatabaseAuthoring.SheetContainer fileContainer
            )
            {
                fileContainer = new FileDatabaseAuthoring.SheetContainer();
                tokenFolderPath = GetCredentialTokenFolderPath(rootPath, databaseAssetName);
                timeZone = TimeZoneInfo.Utc;
                secrets = default;

                var secretsFilePath = authentication == AuthenticationType.OAuth
                    ? rootPath.GetFileAbsolutePath(credentialRelativeFilePath)
                    : rootPath.GetFileAbsolutePath(apiKeyRelativeFilePath);

                try
                {
                    credentialText = File.ReadAllText(secretsFilePath);
                }
                catch (Exception ex)
                {
                    credentialText = string.Empty;
                    DevLoggerAPI.LogException(ex);
                }

                if (string.IsNullOrWhiteSpace(credentialText))
                {
                    Log(LogMessageType.CredentialContentUndefined);
                    return false;
                }

                secrets = authentication == AuthenticationType.OAuth
                    ? LoadSecrets(credentialText)
                    : null;

                if (authentication == AuthenticationType.OAuth)
                {
                    if (secrets == null)
                    {
                        Log(LogMessageType.CredentialFileNotApplicableForOAuth);
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(secrets.ClientSecret))
                    {
                        Log(LogMessageType.CredentialClientSecretIsNullOrInvalid);
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(secrets.ClientId))
                    {
                        Log(LogMessageType.CredentialClientIdIsNullOrInvalid);
                        return false;
                    }
                }

                return true;
            }

            private string GetCredentialTokenFolderPath(RootPath rootPath, string databaseAssetName)
                 => string.IsNullOrWhiteSpace(credentialTokenRelativeFolderPath)
                    ? rootPath.GetFolderAbsolutePath($"Library/Google/{databaseAssetName}")
                    : rootPath.GetFolderAbsolutePath(credentialTokenRelativeFolderPath);

            private async Task<BaseClientService.Initializer> ConnectAsync(
                  string credential
                , ClientSecrets secrets
                , string tokenFolderPath
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {
                if (authentication == AuthenticationType.ApiKey)
                {
                    return new BaseClientService.Initializer {
                        ApiKey = credential,
                        ApplicationName = Application.productName
                    };
                }

                var userCredential = await AuthorizeOAuthAsync(
                      secrets
                    , tokenFolderPath
                    , token
                    , continueOnCapturedContext
                );

                return new BaseClientService.Initializer {
                    HttpClientInitializer = userCredential,
                    ApplicationName = Application.productName,
                };
            }

            /// <summary>
            /// Call to preauthorize when using OAuth authorization.
            /// This will cause a browser to open a Google authorization
            /// page after which the token will be stored in <paramref name="tokenFolderPath"/>
            /// so that this does not need to be done each time.
            /// </summary>
            /// <returns></returns>
            private static async Task<UserCredential> AuthorizeOAuthAsync(
                  ClientSecrets secrets
                , string tokenFolderPath
                , CancellationToken token
                , bool continueOnCapturedContext
            )
            {
                // We use the client Id for the user so that we can generate a unique token file and prevent conflicts
                // when using multiple OAuth authentications. (LOC-188)
                return await AuthorizeAsync(
                      secrets
                    , tokenFolderPath
                    , DatabaseGoogleSheetConverter.Scopes
                    , secrets.ClientId
                    , token
                    , continueOnCapturedContext
                );

                static async Task<UserCredential> AuthorizeAsync(
                      ClientSecrets secrets
                    , string tokenFolderPath
                    , IEnumerable<string> scopes
                    , string user
                    , CancellationToken token
                    , bool continueOnCapturedContext
                )
                {
                    var dataStore = new FileDataStore(tokenFolderPath, false);
                    var initializer = new GoogleAuthorizationCodeFlow.Initializer {
                        ClientSecrets = secrets,
                        Scopes = scopes,
                        DataStore = dataStore ?? new FileDataStore("Google.Apis.Auth"),
                    };

                    var flow = new GoogleAuthorizationCodeFlow(initializer);
                    var codeReceiver = new LocalServerCodeReceiver();

                    return await new AuthorizationCodeInstalledApp(flow, codeReceiver)
                        .AuthorizeAsync(user, token)
                        .ConfigureAwait(continueOnCapturedContext);
                }
            }

            private ClientSecrets LoadSecrets(string credential)
            {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(credential)))
                {
                    var gcs = GoogleClientSecrets.FromStream(stream);
                    return gcs.Secrets;
                }
            }

            private void Report(ReportAction reporter, ProgressMessageType messageType)
            {
                reporter(PROGRESS_TITLE, GetMessage(messageType));
            }

            private void Log(LogMessageType type)
            {
                DevLoggerAPI.LogError(GetMessage(type));
            }

            private string GetMessage(ProgressMessageType type)
            {
                return type switch {
                    ProgressMessageType.Authorize => "Authorizing user...",
                    ProgressMessageType.ReadFileTable => "Try retrieving 'file_list' table...",
                    ProgressMessageType.Download => "Downloading all SpreadSheets...",
                    ProgressMessageType.Validate => "Validating all SpreadSheets...",
                    _ => string.Empty,
                };
            }

            private string GetMessage(LogMessageType type)
            {
                return type switch {
                    LogMessageType.CredentialContentUndefined => "The content of credential file is undefined.",
                    LogMessageType.CredentialFileNotApplicableForOAuth => "The credential file is not applicable for OAuth 2.0.",
                    LogMessageType.CredentialClientSecretIsNullOrInvalid => "Found no Client Secret in the credential file.",
                    LogMessageType.CredentialClientIdIsNullOrInvalid => "Found no Client ID in the credential file.",
                    LogMessageType.FoundNoAppropriateSpreadSheet => "Found no appropriate SpreadSheet.",
                    _ => string.Empty,
                };
            }

            private enum ProgressMessageType
            {
                None,
                Authorize,
                ReadFileTable,
                Download,
                Validate,
            }

            private enum LogMessageType
            {
                None,
                CredentialContentUndefined,
                CredentialFileNotApplicableForOAuth,
                CredentialClientSecretIsNullOrInvalid,
                CredentialClientIdIsNullOrInvalid,
                FoundNoAppropriateSpreadSheet,
            }
        }
    }
}
