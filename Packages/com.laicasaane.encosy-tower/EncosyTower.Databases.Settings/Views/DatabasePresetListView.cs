using System;
using EncosyTower.Editor;
using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
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

                hierarchy.Add(dropdown.WithAlignFieldClass());
            }

            {
                var button = _copyButton = new(CopyPreset) {
#if UNITY_6000_0_OR_NEWER
                    enabledSelf = false,
#endif
                    text = "Copy",
                };

                hierarchy.Add(button);
            }

            {
                var icon = EditorAPI.GetIcon("d_pick", "pick");
                var iconImage = Background.FromTexture2D(icon.image as Texture2D);

#if UNITY_6000_0_OR_NEWER
                var button = _locateButton = new(iconImage, LocateSelectedPreset) {
                    enabledSelf = false,
                };
#else
                var button = _locateButton = ButtonAPI.CreateButton(iconImage, LocateSelectedPreset);
                button.AddToClassList(ButtonAPI.IconOnlyUssClassName);
#endif

                button.tooltip = "Locate Selected Preset";
                button.AddToClassList(Constants.ICON_BUTTON);
                hierarchy.Add(button);
            }

            {
                var icon = EditorAPI.GetIcon("d_refresh", "refresh");
                var iconImage = Background.FromTexture2D(icon.image as Texture2D);

#if UNITY_6000_0_OR_NEWER
                var button = new Button(iconImage, RefreshPresets);
#else
                var button = ButtonAPI.CreateButton(iconImage, RefreshPresets);
#endif

                button.tooltip = "Refresh Preset List";
                button.AddToClassList(Constants.ICON_BUTTON);
                hierarchy.Add(button);
            }

#if !UNITY_6000_0_OR_NEWER
            _copyButton.SetEnabled(false);
            _locateButton.SetEnabled(false);
#endif

            RefreshPresets();
        }

        private void RefreshPresets()
        {
            var rootNode = _menu.rootNode;
            rootNode.Reset();

            var presets = AssetDatabaseAPI.FindAllObjectsByGlobalQualifiedTypeName<DatabaseSettingsPreset>();
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

                if (database.name == Constants.UNDEFINED
                    || string.IsNullOrEmpty(database.authorType)
                    || string.IsNullOrEmpty(database.databaseType)
                )
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
            menu.Show(evt.originalMousePosition);
        }

        private void Menu_OnSelect(object userData)
        {
            if (userData is not DatabaseSettingsPreset preset || preset.IsInvalid())
            {
                _presetDropdown.value = Constants.UNDEFINED;
                _presetDropdown.userData = null;

#if UNITY_6000_0_OR_NEWER
                _copyButton.enabledSelf = false;
                _locateButton.enabledSelf = false;
#else
                _copyButton.SetEnabled(false);
                _locateButton.SetEnabled(false);
#endif

                return;
            }

            _presetDropdown.value = $"{preset.name} [{preset._database.name}]";
            _presetDropdown.userData = preset;

#if UNITY_6000_0_OR_NEWER
            _copyButton.enabledSelf = true;
            _locateButton.enabledSelf = true;
#else
            _copyButton.SetEnabled(true);
            _locateButton.SetEnabled(true);
#endif
        }

        private void LocateSelectedPreset()
        {
            if (_presetDropdown.userData is not DatabaseSettingsPreset preset || preset.IsInvalid())
            {
                return;
            }

            EditorGUIUtility.PingObject(preset);
        }

        private void CopyPreset()
        {
            if (_presetDropdown.userData is not DatabaseSettingsPreset preset
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
