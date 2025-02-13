using EncosyTower.Modules.Editor.UIElements;
using EncosyTower.Modules.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor.Data.Settings.Views
{
    using HelpType = HelpBoxMessageType;

    internal sealed class LocalFolderSettingsView : SettingsView
    {
        private readonly DataSourceFlags _source;
        private readonly FolderTextField _inputFolderText;
        private readonly HelpBox _inputFolderHelp;
        private readonly FolderTextField _outputFolderText;
        private readonly HelpBox _outputFolderHelp;
        private readonly Toggle _liveConversionToggle;
        private readonly Button _convertButton;

        private LocalFolderContext _context;
        private bool _inputFolderValid;
        private bool _outputFolderValid;

        public LocalFolderSettingsView(
              string text
            , string ussClassName
            , ViewResources resources
            , DataSourceFlags source
        ) : base(text, resources)
        {
            _source = source;

            AddToClassList(ussClassName);
            AddToClassList(Constants.SETTINGS_GROUP);
            Add((_inputFolderHelp = new()).SetDisplay(DisplayStyle.None));

            InitPathField(
                  _inputFolderText = new("Input Folder")
                , BrowseFolder
                , PathType.Folder
            );

            Add(new VisualSeparator());
            Add((_outputFolderHelp = new()).SetDisplay(DisplayStyle.None));

            InitPathField(
                  _outputFolderText = new("Output Folder")
                , BrowseFolder
                , PathType.Folder
            );

            Add(new VisualSeparator());

            _liveConversionToggle = new("Live Conversion?");
            Add(_liveConversionToggle.AddToAlignFieldClass());

            Add(new VisualSeparator());

            Add(_convertButton = new(ConvertButton_OnClicked) {
                text = "Convert",
                enabledSelf = false,
            });

            _convertButton.AddToClassList("convert-button");
            _convertButton.AddToClassList("function-button");
            _convertButton.clicked += ConvertButton_OnClicked;

            Add(new VisualSeparator());

            var cleanOutputFolderButton = new Button() { text = "Clean Output Folder" };
            cleanOutputFolderButton.AddToClassList("function-button");
            cleanOutputFolderButton.clicked += CleanOutputFolderButton_OnClicked;
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

            _liveConversionToggle.Bind(context.GetLiveConversionProperty());
        }

        public override void Unbind()
        {
            _context = default;

            base.Unbind();

            _inputFolderText.Unbind();
            _outputFolderText.Unbind();
            _liveConversionToggle.Unbind();
        }

        protected override void OnEnabled(bool value)
        {
            _convertButton.enabledSelf = value;
        }

        private void RegisterValueChangedCallbacks()
        {
            _inputFolderText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _outputFolderText.TextField.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);
            _liveConversionToggle.RegisterValueChangedCallback(OnValueChanged_EquatableTyped);

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
            _convertButton.enabledSelf = Enabled
                && _inputFolderValid
                && _outputFolderValid;
        }

        private void ConvertButton_OnClicked()
        {
            var owner = _context.Property.serializedObject.targetObject;
            _context.DatabaseSettings?.Convert(_source, owner);
        }

        private void CleanOutputFolderButton_OnClicked()
        {
            _context.DatabaseSettings?.CleanOutputFolder(_source);
            AssetDatabase.Refresh();
        }
    }
}
