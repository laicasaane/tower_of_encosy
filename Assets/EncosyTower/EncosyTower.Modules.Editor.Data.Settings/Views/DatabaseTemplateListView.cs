using System;
using EncosyTower.Modules.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor.Data.Settings.Views
{
    internal sealed class DatabaseTemplateListView : VisualElement
    {
        private const string USS_CLASS_NAME = "database-template-list";

        public event Action<DatabaseSettingsTemplate> TemplateSelected;

        private readonly GenericMenuPopup _menu = new(new MenuItemNode(), "Templates");
        private readonly DropdownField _templateDropdown;
        private readonly Button _copyButton;
        private readonly Button _locateButton;

        public DatabaseTemplateListView() : base()
        {
            AddToClassList(USS_CLASS_NAME);

            {
                var dropdown = _templateDropdown = new("Template");
                dropdown.value = Constants.UNDEFINED;
                dropdown.AddToClassList(Constants.TEMPLATE_DROPDOWN);
                dropdown.RegisterCallback<PointerUpEvent>(TemplateDropdown_OnPointerUpEvent);

                hierarchy.Add(dropdown.AddToAlignFieldClass());
            }

            {
                var button = _copyButton = new() {
                    enabledSelf = false,
                };

                button.text = "Copy";
                hierarchy.Add(button);

                button.clicked += CopyTemplate;
            }

            {
                var button = _locateButton = new() {
                    enabledSelf = false,
                };

                var icon = EditorAPI.GetIcon("d_pick", "pick");
                button.iconImage = Background.FromTexture2D(icon.image as Texture2D);
                button.tooltip = "Locate Selected Template";
                button.AddToClassList(Constants.ICON_BUTTON);
                hierarchy.Add(button);

                button.clicked += LocateSelectedTemplate;
            }

            {
                var button = new Button();
                var icon = EditorAPI.GetIcon("d_refresh", "refresh");
                button.iconImage = Background.FromTexture2D(icon.image as Texture2D);
                button.tooltip = "Refresh Template List";
                button.AddToClassList(Constants.ICON_BUTTON);
                hierarchy.Add(button);

                button.clicked += RefreshTemplates;
            }

            RefreshTemplates();
        }

        private void RefreshTemplates()
        {
            var rootNode = _menu.RootNode;
            rootNode.Reset();

            var templates = AssetDatabaseAPI.FindAllObjectsByGlobalQualifiedType<DatabaseSettingsTemplate>();
            var count = templates.Count;

            GenericMenu.MenuFunction2 func2 = Menu_OnSelect;

            {
                var currentNode = rootNode.CreateNode(Constants.UNDEFINED);
                currentNode.func2 = func2;
                currentNode.userData = null;
                currentNode.on = false;
            }

            for (var i = 0; i < count; i++)
            {
                var template = templates[i];

                if (template.IsInvalid())
                {
                    continue;
                }

                if (template._database is not { } database)
                {
                    continue;
                }

                if (database.name == Constants.UNDEFINED || string.IsNullOrEmpty(database.type))
                {
                    continue;
                }

                var name = $"{template.name}\n[{database.name}]";
                var currentNode = rootNode.CreateNode(name);
                currentNode.func2 = func2;
                currentNode.userData = template;
                currentNode.on = false;
            }
        }

        private void TemplateDropdown_OnPointerUpEvent(PointerUpEvent evt)
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
            if (userData is not DatabaseSettingsTemplate template || template.IsInvalid())
            {
                _templateDropdown.value = Constants.UNDEFINED;
                _templateDropdown.dataSource = null;
                _copyButton.enabledSelf = false;
                _locateButton.enabledSelf = false;
                return;
            }

            _templateDropdown.value = $"{template.name} [{template._database.name}]";
            _templateDropdown.dataSource = template;
            _copyButton.enabledSelf = true;
            _locateButton.enabledSelf = true;
        }

        private void LocateSelectedTemplate()
        {
            if (_templateDropdown.dataSource is not DatabaseSettingsTemplate template || template.IsInvalid())
            {
                return;
            }

            EditorGUIUtility.PingObject(template);
        }

        private void CopyTemplate()
        {
            if (_templateDropdown.dataSource is not DatabaseSettingsTemplate template
                || template.IsInvalid()
                || template._database is not { } database
            )
            {
                return;
            }

            TemplateSelected?.Invoke(template);
        }
    }
}
