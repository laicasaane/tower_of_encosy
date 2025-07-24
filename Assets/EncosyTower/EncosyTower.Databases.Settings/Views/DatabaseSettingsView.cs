using System;
using EncosyTower.Editor;
using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal sealed class DatabaseSettingsView : VisualElement
    {
        private const string USS_CLASS_NAME = "database";
        private const float SAVE_DELAY = 0.5f;

        public event Action<DatabaseSettingsView, DatabaseRecord> DatabaseTypeSelected;
        public event Action<DatabaseSettingsView> OtherValueUpdated;

        private readonly GenericMenuPopup _menu = new(new MenuItemNode(), "Databases");

        private readonly DropdownField _dbDropdown;
        private readonly HelpBox _databaseHelp;
        private readonly VisualElement _container;
        private readonly GoogleSheetSettingsView _googleSheetView;
        private readonly LocalFolderSettingsView _csvView;
        private readonly LocalFolderSettingsView _excelView;

        // An optimization to limit the chance the
        // frequent of OtherValueUpdated being invoked.
        private bool _otherValueUpdated;
        private double _markTime;

        public DatabaseSettingsView(bool visible, ViewResources resources) : base()
        {
            this.visible = visible;

            AddToClassList(USS_CLASS_NAME);

            InitMenu();

            var dbDropdown = _dbDropdown = new DropdownField("Database") { value = Constants.UNDEFINED };
            dbDropdown.AddToClassList(Constants.NAME_DROPDOWN);
            dbDropdown.RegisterCallback<PointerUpEvent>(NameDropdown_OnPointerUpEvent);

            hierarchy.Add(dbDropdown.AddToAlignFieldClass());
            hierarchy.Add(new VisualSeparator("section"));

            _databaseHelp = new(resources.DatabaseInvalid, HelpBoxMessageType.Error);
            hierarchy.Add(_databaseHelp.SetDisplay(DisplayStyle.None));

            var container = _container = new();
            container.AddToClassList("settings-groups-list");
            hierarchy.Add(container.SetDisplay(DisplayStyle.None));

            var googleSheetView = _googleSheetView = new(resources);
            container.Add(googleSheetView);

            var csvView = _csvView = new("Csv", "csv", resources, DataSourceFlags.Csv);
            container.Add(csvView);

            var excelView = _excelView = new("Excel", "excel", resources, DataSourceFlags.Excel);
            container.Add(excelView);

            RegisterValueChangedCallbacks();
        }

        public void Bind(DatabaseSettingsContext context)
        {
            {
                var prop = context.Database.GetNameProperty();
                _dbDropdown.BindProperty(prop);

                var typeNameValid = string.Equals(prop.stringValue, Constants.UNDEFINED) == false;
                ToggleDisplayContainer(typeNameValid);
            }

            _googleSheetView.Bind(context.GoogleSheet);
            _csvView.Bind(context.Csv);
            _excelView.Bind(context.Excel);
        }

        public void Unbind()
        {
            _dbDropdown.Unbind();
            _googleSheetView.Unbind();
            _csvView.Unbind();
            _excelView.Unbind();
        }

        public void Update()
        {
            // Only invoke `OtherValueUpdated` once if possible,
            // because it is going to invoke saving asset to disk.
            if (_otherValueUpdated == false)
            {
                return;
            }

            var time = EditorApplication.timeSinceStartup - _markTime;

            if (time >= SAVE_DELAY)
            {
                Save();
            }
        }

        public void Save()
        {
            if (_otherValueUpdated)
            {
                OtherValueUpdated?.Invoke(this);
                _otherValueUpdated = false;
            }
        }

        public void ToggleDisplayContainer(bool value)
        {
            _databaseHelp.SetDisplay(value ? DisplayStyle.None : DisplayStyle.Flex);
            _container.SetDisplay(value ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void RegisterValueChangedCallbacks()
        {
            _googleSheetView.ValueUpdated += OnValueUpdated;
            _csvView.ValueUpdated += OnValueUpdated;
            _excelView.ValueUpdated += OnValueUpdated;
        }

        private void InitMenu()
        {
            var rootNode = _menu.rootNode;

            if (rootNode.Nodes.Count > 0)
            {
                return;
            }

            var records = DatabaseTypeVault.Records.AsSpan();

            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                var currentNode = rootNode.CreateNode(record.Name);
                currentNode.func2 = userData => DatabaseTypeSelected?.Invoke(this, userData as DatabaseRecord);
                currentNode.userData = record;
                currentNode.on = false;
            }
        }

        private void NameDropdown_OnPointerUpEvent(PointerUpEvent evt)
        {
            var menu = _menu;
            menu.width = 300;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.Show(evt.originalMousePosition);
        }

        private void OnValueUpdated()
        {
            _markTime = EditorApplication.timeSinceStartup;
            _otherValueUpdated = true;
        }
    }
}
