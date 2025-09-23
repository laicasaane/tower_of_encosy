using System;
using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_6000_0_OR_NEWER
using EncosyTower.Editor;
#endif

namespace EncosyTower.Databases.Settings.Views
{
    using AuthenticationType = DatabaseCollectionSettings.AuthenticationType;
    using HelpType = HelpBoxMessageType;
    using OutputFileType = DatabaseCollectionSettings.OutputFileType;

    internal sealed class GoogleSheetSettingsView : SettingsView
    {
        private static readonly string[] s_fileExts = new[] { "ALL", "*", "JSON", "json", "TXT", "txt" };

        private readonly FolderTextField _credentialFileText;
        private readonly VisualElement _credentialFileSubInfo;
        private readonly FolderTextField _apiKeyFileText;
        private readonly VisualElement _apiKeyFileSubInfo;
        private readonly EnumField _authenticationEnum;
        private readonly HelpBox _credentialFileHelp;
        private readonly HelpBox _apiKeyFileHelp;
        private readonly ButtonTextField _spreadsheetIdText;
        private readonly HelpBox _spreadsheetIdHelp;
        private readonly VisualSeparator _tokenSeparator;
        private readonly FolderTextField _tokenFolderText;
        private readonly VisualElement _tokenSubInfo;
        private readonly HelpBox _tokenFolderHelp;
        private readonly FolderTextField _outputFolderText;
        private readonly HelpBox _outputFolderHelp;
        private readonly EnumField _outputFileTypeEnum;
        private readonly Toggle _cleanOutputFolderToggle;
        private readonly Toggle _alwaysDownloadAllToggle;
        private readonly IntegerField _emptyRowStreakThresholdField;
        private readonly Button _downloadButton;

        private bool _credentialFileValid;
        private bool _apiKeyFileValid;
        private bool _spreadsheetIdValid;
        private bool _tokenFolderValid;
        private bool _outputFolderValid;

        private GoogleSheetContext _context;

        public GoogleSheetSettingsView(ViewResources resources)
            : base("Google Sheet", resources)
        {
            AddToClassList("google-sheet");
            AddToClassList(Constants.SETTINGS_GROUP);

            Add(new HelpBox(resources.GoogleSheet.Credential, HelpType.Info));

            _authenticationEnum = new("Authentication", default(AuthenticationType));
            Add(_authenticationEnum.WithAlignFieldClass());

            Add(new VisualSeparator());

            _credentialFileHelp = new(resources.GoogleSheet.CredentialMissing, HelpType.Error);
            Add(_credentialFileHelp.WithDisplay(DisplayStyle.None));

            _credentialFileText = CreatePathField(
                  "Credential File"
                , BrowseCredentialFile
                , PathType.File
                , out _credentialFileSubInfo
            );

            _apiKeyFileHelp = new(resources.GoogleSheet.ApiKeyMissing, HelpType.Error);
            Add(_apiKeyFileHelp.WithDisplay(DisplayStyle.None));

            _apiKeyFileText = CreatePathField(
                  "API Key File"
                , BrowseApiKeyFile
                , PathType.File
                , out _apiKeyFileSubInfo
            );

            Add(new VisualSeparator());

            Add(_tokenSeparator = new VisualSeparator());
            Add((_tokenFolderHelp = new()).WithDisplay(DisplayStyle.None));

            _tokenFolderText = CreatePathField(
                  "Auth Token Folder"
                , BrowseFolder
                , PathType.Folder
                , out _tokenSubInfo
            );

            SetDisplayToTokenControls(DisplayStyle.None);

            Add(new VisualSeparator());
            Add(new HelpBox(resources.GoogleSheet.SpreadSheetId, HelpType.Info));

            _spreadsheetIdHelp = new(resources.GoogleSheet.SpreadSheetIdInvalid, HelpType.Error);
            Add(_spreadsheetIdHelp.WithDisplay(DisplayStyle.None));

            InitSpreadSheetIdField(
                  _spreadsheetIdText = new("Spread Sheet Id")
                , OpenSpreadSheetUrl
            );

            Add(new VisualSeparator());
            Add((_outputFolderHelp = new()).WithDisplay(DisplayStyle.None));

            _outputFolderText = CreatePathField(
                  "Output Folder"
                , BrowseFolder
                , PathType.Folder
                , out _
            );

            Add(new VisualSeparator());

            _outputFileTypeEnum = new("Output File Type", default(OutputFileType));
            Add(_outputFileTypeEnum.WithAlignFieldClass());

            _cleanOutputFolderToggle = new("Clean Output Folder?");
            Add(_cleanOutputFolderToggle.WithAlignFieldClass());

            _alwaysDownloadAllToggle = new("Always Download All?");
            Add(_alwaysDownloadAllToggle.WithAlignFieldClass());

            _emptyRowStreakThresholdField = new("Empty Row Streak Threshold") {
                tooltip = "The maximum number of continuous empty rows allowed before file is considered ended."
            };
            Add(_emptyRowStreakThresholdField.WithAlignFieldClass());

            Add(new VisualSeparator());

            Add(_downloadButton = new(DownloadButton_OnClicked) {
#if UNITY_6000_0_OR_NEWER
                enabledSelf = false,
#endif
            });

            _downloadButton.AddToClassList("convert-button");
            _downloadButton.AddToClassList("function-button");

#if !UNITY_6000_0_OR_NEWER
            _downloadButton.SetEnabled(false);
#endif

            Add(new VisualSeparator());

            var cleanOutputFolderButton = new Button(CleanOutputFolderButton_OnClicked) {
                text = "Clean Output Folder",
            };

            cleanOutputFolderButton.AddToClassList("function-button");
            Add(cleanOutputFolderButton);

            RegisterValueChangedCallbacks();
        }

        public void Bind(GoogleSheetContext context)
        {
            _context = context;

            BindFoldout(context.GetEnabledProperty());

            {
                var prop = context.GetCredentialRelativeFilePathProperty();
                _credentialFileText.Bind(prop);
                TryDisplayCredentialFileHelp(prop.stringValue);
            }

            {
                var prop = context.GetApiKeyRelativeFilePathProperty();
                _apiKeyFileText.Bind(prop);
                TryDisplayApiKeyFileHelp(prop.stringValue);
            }

            {
                var prop = context.GetSpreadsheetIdProperty();
                _spreadsheetIdText.Bind(prop);
                TryDisplaySpreadSheetIdHelp(prop.stringValue);
            }

            {
                var prop = context.GetCredentialTokenRelativeFolderPathProperty();
                _tokenFolderText.Bind(prop);
                TryDisplayTokenFolderHelp(prop.stringValue);
            }

            {
                var prop = context.GetOutputRelativeFolderPathProperty();
                _outputFolderText.Bind(prop);
                TryDisplayOutputFolderHelp(prop.stringValue);
            }

            _cleanOutputFolderToggle.BindProperty(context.GetCleanOutputFolderProperty());
            _alwaysDownloadAllToggle.BindProperty(context.GetAlwaysDownloadAllProperty());
            _emptyRowStreakThresholdField.BindProperty(context.GetEmptyRowStreakThresholdProperty());

            {
                var prop = context.GetOutputFileTypeProperty();
                _outputFileTypeEnum.BindProperty(prop);
                InitDownloadButton((OutputFileType)prop.enumValueIndex);
            }

            {
                var prop = context.GetAuthenticationProperty();
                _authenticationEnum.BindProperty(prop);
            }
        }

        public override void Unbind()
        {
            _context = default;

            base.Unbind();

            _credentialFileText.Unbind();
            _apiKeyFileText.Unbind();
            _authenticationEnum.Unbind();
            _spreadsheetIdText.Unbind();
            _tokenFolderText.Unbind();
            _outputFolderText.Unbind();
            _outputFileTypeEnum.Unbind();
            _cleanOutputFolderToggle.Unbind();
            _alwaysDownloadAllToggle.Unbind();
            _emptyRowStreakThresholdField.Unbind();
        }

        protected override void OnEnabled(bool value)
        {
            RefreshDownloadButton();
        }

        private void InitSpreadSheetIdField(
              ButtonTextField element
            , Action<ButtonTextField> onClicked
        )
        {
            Add(element.WithAlignFieldClass());

            var icon = EditorAPI.GetIcon("d_buildsettings.web.small", "buildsettings.web.small");
            var iconImage = Background.FromTexture2D(icon.image as Texture2D);

#if UNITY_6000_0_OR_NEWER
            element.Button.iconImage = iconImage;
            element.Button.text = "Open";
#else
            element.Button.SetToImageElement(iconImage);
            element.Button.SetToTextElement("Open");
#endif

            element.Clicked += onClicked;
        }

        private void RegisterValueChangedCallbacks()
        {
            _credentialFileText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _apiKeyFileText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _authenticationEnum.RegisterValueChangedCallback(OnValueChanged_ComparableUntyped);
            _spreadsheetIdText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _tokenFolderText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _outputFolderText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _outputFileTypeEnum.RegisterValueChangedCallback(OnValueChanged_ComparableUntyped);
            _cleanOutputFolderToggle.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _alwaysDownloadAllToggle.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _emptyRowStreakThresholdField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);

            _authenticationEnum.RegisterValueChangedCallback(AuthenticationType_OnChanged);
            _credentialFileText.TextField.RegisterValueChangedCallback(CredentialFile_OnChanged);
            _apiKeyFileText.TextField.RegisterValueChangedCallback(ApiKeyFile_OnChanged);
            _spreadsheetIdText.TextField.RegisterValueChangedCallback(SpreadSheetId_OnChanged);
            _tokenFolderText.TextField.RegisterValueChangedCallback(TokenFolder_OnChanged);
            _outputFolderText.TextField.RegisterValueChangedCallback(OutputFolder_OnChanged);
            _outputFileTypeEnum.RegisterValueChangedCallback(OutputFileType_OnChanged);
        }

        private void SetDisplayToTokenControls(DisplayStyle style)
        {
            _tokenSeparator.WithDisplay(style);
            _tokenFolderHelp.WithDisplay(style);
            _tokenFolderText.WithDisplay(style);
            _tokenSubInfo.WithDisplay(style);
        }

        private void SetDisplayToCredentialControls(DisplayStyle style)
        {
            _credentialFileText.WithDisplay(style);
            _credentialFileHelp.WithDisplay(style);
            _credentialFileSubInfo.WithDisplay(style);
        }

        private void SetDisplayToApiKeyControls(DisplayStyle style)
        {
            _apiKeyFileText.WithDisplay(style);
            _apiKeyFileHelp.WithDisplay(style);
            _apiKeyFileSubInfo.WithDisplay(style);
        }

        private void InitDownloadButton(OutputFileType fileType)
            => _downloadButton.text = $"Download to {fileType.ToDisplayStringFast()}";

        private void BrowseCredentialFile(ButtonTextField sender)
            => DirectoryAPI.OpenFilePanel(sender.TextField, "Select Credential File", s_fileExts);

        private void BrowseApiKeyFile(ButtonTextField sender)
            => DirectoryAPI.OpenFilePanel(sender.TextField, "Select API Key File", s_fileExts);

        private void OpenSpreadSheetUrl(ButtonTextField sender)
            => Application.OpenURL($"https://docs.google.com/spreadsheets/d/{sender.TextField.value}");

        private void AuthenticationType_OnChanged(ChangeEvent<Enum> evt)
        {
            if (evt.newValue is AuthenticationType authentication
                && authentication == AuthenticationType.OAuth
            )
            {
                var prop = _context.GetCredentialTokenRelativeFolderPathProperty();

                SetDisplayToCredentialControls(DisplayStyle.Flex);
                SetDisplayToApiKeyControls(DisplayStyle.None);
                SetDisplayToTokenControls(DisplayStyle.Flex);
                TryDisplayTokenFolderHelp(prop.stringValue);
                TryDisplayCredentialFileHelp(_credentialFileText.TextField.value);
            }
            else
            {
                SetDisplayToCredentialControls(DisplayStyle.None);
                SetDisplayToApiKeyControls(DisplayStyle.Flex);
                SetDisplayToTokenControls(DisplayStyle.None);
                TryDisplayApiKeyFileHelp(_apiKeyFileText.TextField.value);
            }
        }

        private void CredentialFile_OnChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue.Contains('<'))
            {
                /// README
                Notes.ToPreventIllegalCharsExceptionWhenSearch();
                return;
            }

            TryDisplayCredentialFileHelp(evt.newValue);
        }

        private void ApiKeyFile_OnChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue.Contains('<'))
            {
                /// README
                Notes.ToPreventIllegalCharsExceptionWhenSearch();
                return;
            }

            TryDisplayApiKeyFileHelp(evt.newValue);
        }

        private void SpreadSheetId_OnChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue.Contains('<'))
            {
                /// README
                Notes.ToPreventIllegalCharsExceptionWhenSearch();
                return;
            }

            TryDisplaySpreadSheetIdHelp(evt.newValue);
        }

        private void TokenFolder_OnChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue.Contains('<'))
            {
                /// README
                Notes.ToPreventIllegalCharsExceptionWhenSearch();
                return;
            }

            TryDisplayTokenFolderHelp(evt.newValue);
        }

        private void OutputFolder_OnChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue.Contains('<'))
            {
                /// README
                Notes.ToPreventIllegalCharsExceptionWhenSearch();
                return;
            }

            TryDisplayOutputFolderHelp(evt.newValue);
        }

        private void OutputFileType_OnChanged(ChangeEvent<Enum> evt)
        {
            if (evt.newValue is OutputFileType fileType)
            {
                InitDownloadButton(fileType);
            }
        }

        private void TryDisplayCredentialFileHelp(string relativePath)
        {
            _credentialFileValid = DisplayIfFileNotExist(_credentialFileHelp, relativePath);
            RefreshDownloadButton();
        }

        private void TryDisplayApiKeyFileHelp(string relativePath)
        {
            _apiKeyFileValid = DisplayIfFileNotExist(_apiKeyFileHelp, relativePath);
            RefreshDownloadButton();
        }

        private void TryDisplaySpreadSheetIdHelp(string value)
        {
            _spreadsheetIdValid = DisplayIfStringEmpty(_spreadsheetIdHelp, value);
            RefreshDownloadButton();
        }

        private void TryDisplayTokenFolderHelp(string relativePath)
        {
            var prop = _context.GetAuthenticationProperty();

            if ((AuthenticationType)prop.enumValueIndex != AuthenticationType.OAuth)
            {
                return;
            }

            var resources = Resources.GoogleSheet;

            TryDisplayFolderHelp(
                  _tokenFolderHelp
                , relativePath
                , new(resources.TokenFolderInvalid, HelpType.Error, false)
                , new(resources.TokenFolderMissing, HelpType.Warning, true)
                , ref _tokenFolderValid
            );
        }

        private void TryDisplayOutputFolderHelp(string relativePath)
        {
            var resources = Resources.GoogleSheet;

            TryDisplayFolderHelp(
                  _outputFolderHelp
                , relativePath
                , new(resources.OutputFolderInvalid, HelpType.Error, false)
                , new(resources.OutputFolderMissing, HelpType.Warning, true)
                , ref _outputFolderValid
            );

            RefreshDownloadButton();
        }

        private void RefreshDownloadButton()
        {
            var authentication = (AuthenticationType)_context.GetAuthenticationProperty().enumValueIndex;
            var credentialFileValid = authentication == AuthenticationType.OAuth && _credentialFileValid;
            var apiKeyFileValid = authentication == AuthenticationType.ApiKey && _apiKeyFileValid;

            var value = Enabled
                && (credentialFileValid || apiKeyFileValid)
                && _spreadsheetIdValid
                && _outputFolderValid;

#if UNITY_6000_0_OR_NEWER
            _downloadButton.enabledSelf = value;
#else
            _downloadButton.SetEnabled(value);
#endif
        }

        private void DownloadButton_OnClicked()
        {
            ConversionTask.Run(
                  _context.DatabaseSettings
                , DataSourceFlags.GoogleSheet
                , _context.Property.serializedObject.targetObject
                , DatabaseCollectionSettings.GoogleSheetSettings.PROGRESS_TITLE
            );
        }

        private void CleanOutputFolderButton_OnClicked()
        {
            _context.DatabaseSettings?.CleanOutputFolder(DataSourceFlags.GoogleSheet);
            AssetDatabase.Refresh();
        }
    }
}
