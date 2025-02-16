using System;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Data.Settings.Views
{
    internal sealed class DatabasePresetListView : VisualElement
    {
        private const string USS_CLASS_NAME = "database-preset-list";

        public event Action<DatabaseSettingsPreset> PresetSelected;

        private readonly GenericMenuPopup _menu = new(new MenuItemNode(), "Presets");
        private readonly DropdownField _presetDropdown;
        private readonly Button _copyButton;
        private readonly Button _locateButton;

        public DatabasePresetListView() : base()
        {
            AddToClassList(USS_CLASS_NAME);

            {
                var dropdown = _presetDropdown = new("Preset");
                dropdown.value = Constants.UNDEFINED;
                dropdown.AddToClassList(Constants.PRESET_DROPDOWN);
                dropdown.RegisterCallback<PointerUpEvent>(PresetDropdown_OnPointerUpEvent);

                hierarchy.Add(dropdown.AddToAlignFieldClass());
            }

            {
                var button = _copyButton = new() {
                    enabledSelf = false,
                };

                button.text = "Copy";
                hierarchy.Add(button);

                button.clicked += CopyPreset;
            }

            {
                var button = _locateButton = new() {
                    enabledSelf = false,
                };

                var icon = EditorAPI.GetIcon("d_pick", "pick");
                button.iconImage = Background.FromTexture2D(icon.image as Texture2D);
                button.tooltip = "Locate Selected Preset";
                button.AddToClassList(Constants.ICON_BUTTON);
                hierarchy.Add(button);

                button.clicked += LocateSelectedPreset;
            }

            {
                var button = new Button();
                var icon = EditorAPI.GetIcon("d_refresh", "refresh");
                button.iconImage = Background.FromTexture2D(icon.image as Texture2D);
                button.tooltip = "Refresh Preset List";
                button.AddToClassList(Constants.ICON_BUTTON);
                hierarchy.Add(button);

                button.clicked += RefreshPresets;
            }

            RefreshPresets();
        }

        private void RefreshPresets()
        {
            var rootNode = _menu.RootNode;
            rootNode.Reset();

            var presets = AssetDatabaseAPI.FindAllObjectsByGlobalQualifiedType<DatabaseSettingsPreset>();
            var count = presets.Count;

            GenericMenu.MenuFunction2 func2 = Menu_OnSelect;

            {
                var currentNode = rootNode.CreateNode(Constants.UNDEFINED);
                currentNode.func2 = func2;
                currentNode.userData = null;
                currentNode.on = false;
            }

            for (var i = 0; i < count; i++)
            {
                var preset = presets[i];

                if (preset.IsInvalid())
                {
                    continue;
                }

                if (preset._database is not { } database)
                {
                    continue;
                }

                if (database.name == Constants.UNDEFINED || string.IsNullOrEmpty(database.type))
                {
                    continue;
                }

                var name = $"{preset.name}\n[{database.name}]";
                var currentNode = rootNode.CreateNode(name);
                currentNode.func2 = func2;
                currentNode.userData = preset;
                currentNode.on = false;
            }
        }

        private void PresetDropdown_OnPointerUpEvent(PointerUpEvent evt)
        {
            var menu = _menu;
            menu.width = 300;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.Show(Event.current.mousePosition);
        }

        private void Menu_OnSelect(object userData)
        {
            if (userData is not DatabaseSettingsPreset preset || preset.IsInvalid())
            {
                _presetDropdown.value = Constants.UNDEFINED;
                _presetDropdown.dataSource = null;
                _copyButton.enabledSelf = false;
                _locateButton.enabledSelf = false;
                return;
            }

            _presetDropdown.value = $"{preset.name} [{preset._database.name}]";
            _presetDropdown.dataSource = preset;
            _copyButton.enabledSelf = true;
            _locateButton.enabledSelf = true;
        }

        private void LocateSelectedPreset()
        {
            if (_presetDropdown.dataSource is not DatabaseSettingsPreset preset || preset.IsInvalid())
            {
                return;
            }

            EditorGUIUtility.PingObject(preset);
        }

        private void CopyPreset()
        {
            if (_presetDropdown.dataSource is not DatabaseSettingsPreset preset
                || preset.IsInvalid()
                || preset._database is not { } database
            )
            {
                return;
            }

            PresetSelected?.Invoke(preset);
        }
    }
}
