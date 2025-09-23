using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    using HelpType = HelpBoxMessageType;

    internal abstract class LocalFolderSettingsView : SettingsView
    {
        private readonly DataSourceFlags _source;
        private readonly FolderTextField _inputFolderText;
        private readonly HelpBox _inputFolderHelp;
        private readonly FolderTextField _outputFolderText;
        private readonly HelpBox _outputFolderHelp;
        private readonly Toggle _liveConversionToggle;
        private readonly IntegerField _emptyRowStreakThresholdField;
        private readonly Toggle _includeSubFoldersToggle;
        private readonly Toggle _includeCommentedFilesToggle;
        private readonly Button _convertButton;

        private LocalFolderContext _context;
        private bool _inputFolderValid;
        private bool _outputFolderValid;

        protected LocalFolderSettingsView(
              string text
            , string ussClassName
            , ViewResources resources
            , DataSourceFlags source
        ) : base(text, resources)
        {
            _source = source;

            AddToClassList(ussClassName);
            AddToClassList(Constants.SETTINGS_GROUP);
            Add((_inputFolderHelp = new()).WithDisplay(DisplayStyle.None));

            _inputFolderText = CreatePathField(
                  "Input Folder"
                , BrowseFolder
                , PathType.Folder
                , out _
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

            _liveConversionToggle = new("Live Conversion?");
            Add(_liveConversionToggle.WithAlignFieldClass());

            _emptyRowStreakThresholdField = new("Empty Row Streak Threshold") {
                tooltip = "The maximum number of continuous empty rows allowed before file is considered ended."
            };
            Add(_emptyRowStreakThresholdField.WithAlignFieldClass());

            Add(new VisualSeparator());

            _includeSubFoldersToggle = new("Include Sub-Folders?");
            Add(_includeSubFoldersToggle.WithAlignFieldClass());

            _includeCommentedFilesToggle = new("Include Commented Files?");
            Add(_includeCommentedFilesToggle.WithAlignFieldClass());

            CreateAdditionalFields();

            Add(new VisualSeparator());

            Add(_convertButton = new(ConvertButton_OnClicked) {
                text = "Convert",
#if UNITY_6000_0_OR_NEWER
                enabledSelf = false,
#endif
            });

            _convertButton.AddToClassList("convert-button");
            _convertButton.AddToClassList("function-button");

#if !UNITY_6000_0_OR_NEWER
            _convertButton.SetEnabled(false);
#endif

            Add(new VisualSeparator());

            var cleanOutputFolderButton = new Button(CleanOutputFolderButton_OnClicked) {
                text = "Clean Output Folder"
            };

            cleanOutputFolderButton.AddToClassList("function-button");
            Add(cleanOutputFolderButton);

            RegisterValueChangedCallbacks();
        }

        public void Bind(LocalFolderContext context)
        {
            _context = context;

            BindFoldout(context.GetEnabledProperty());

            {
                var prop = context.GetInputRelativeFolderPathProperty();
                _inputFolderText.Bind(prop);
                TryDisplayInputFolderHelp(prop.stringValue);
            }

            {
                var prop = context.GetOutputRelativeFolderPathProperty();
                _outputFolderText.Bind(prop);
                TryDisplayOutputFolderHelp(prop.stringValue);
            }

            _emptyRowStreakThresholdField.BindProperty(context.GetEmptyRowStreakThresholdProperty());
            _includeSubFoldersToggle.BindProperty(context.GetIncludeSubFoldersProperty());
            _includeCommentedFilesToggle.BindProperty(context.GetIncludeCommentedFilesProperty());
            _liveConversionToggle.BindProperty(context.GetLiveConversionProperty());

            OnBind(context);
        }

        public sealed override void Unbind()
        {
            _context = default;

            base.Unbind();

            _inputFolderText.Unbind();
            _outputFolderText.Unbind();
            _liveConversionToggle.Unbind();
            _emptyRowStreakThresholdField.Unbind();
            _includeSubFoldersToggle.Unbind();
            _includeCommentedFilesToggle.Unbind();

            OnUnbind();
        }

        protected abstract void CreateAdditionalFields();

        protected abstract void OnBind(LocalFolderContext context);

        protected abstract void OnUnbind();

        protected sealed override void OnEnabled(bool value)
        {
#if UNITY_6000_0_OR_NEWER
            _convertButton.enabledSelf = value;
#else
            _convertButton.SetEnabled(value);
#endif
        }

        private void RegisterValueChangedCallbacks()
        {
            _inputFolderText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _outputFolderText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _liveConversionToggle.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _emptyRowStreakThresholdField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _includeSubFoldersToggle.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _includeCommentedFilesToggle.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);

            _inputFolderText.TextField.RegisterValueChangedCallback(InputFolder_OnChanged);
            _outputFolderText.TextField.RegisterValueChangedCallback(OutputFolder_OnChanged);
        }

        private void InputFolder_OnChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue.Contains('<'))
            {
                /// README
                Notes.ToPreventIllegalCharsExceptionWhenSearch();
                return;
            }

            TryDisplayInputFolderHelp(evt.newValue);
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

        private void TryDisplayInputFolderHelp(string relativePath)
        {
            var resources = Resources.LocalFolder;

            TryDisplayFolderHelp(
                  _inputFolderHelp
                , relativePath
                , new(resources.InputFolderInvalid, HelpType.Error, false)
                , new(resources.InputFolderMissing, HelpType.Error, false)
                , ref _inputFolderValid
            );

            RefreshConvertButton();
        }

        private void TryDisplayOutputFolderHelp(string relativePath)
        {
            var resources = Resources.LocalFolder;

            TryDisplayFolderHelp(
                  _outputFolderHelp
                , relativePath
                , new(resources.OutputFolderInvalid, HelpType.Error, false)
                , new(resources.OutputFolderMissing, HelpType.Warning, true)
                , ref _outputFolderValid
            );

            RefreshConvertButton();
        }

        private void RefreshConvertButton()
        {
            var value = Enabled
                && _inputFolderValid
                && _outputFolderValid;

#if UNITY_6000_0_OR_NEWER
            _convertButton.enabledSelf = value;
#else
            _convertButton.SetEnabled(value);
#endif
        }

        private void ConvertButton_OnClicked()
        {
            ConversionTask.Run(
                  _context.DatabaseSettings
                , _source
                , _context.Property.serializedObject.targetObject
                , DatabaseCollectionSettings.LocalFolderSettings.GetProgressTitle(_source)
            );
        }

        private void CleanOutputFolderButton_OnClicked()
        {
            _context.DatabaseSettings?.CleanOutputFolder(_source);
            AssetDatabase.Refresh();
        }
    }
}
