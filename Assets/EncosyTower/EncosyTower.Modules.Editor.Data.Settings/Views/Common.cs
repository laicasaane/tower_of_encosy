using System;
using EncosyTower.Modules.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor.Data.Settings.Views
{
    internal static class Constants
    {
        public const string PROJECT_SETTINGS_STYLE_SHEET = "StyleSheets/ProjectSettings/ProjectSettingsCommon.uss";

        private const string ROOT_PATH = "Assets/EncosyTower/EncosyTower.Modules.Editor.Data.Settings";
        private const string STYLE_SHEETS_PATH = $"{ROOT_PATH}/StyleSheets";
        private const string FILE_NAME = "DatabaseCollectionSettings";

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        public const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        public const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";
        public const string STYLE_SHEET_RESOURCES = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Resources.uxml";

        public const string DATABASE_COLLECTION = "database-collection";
        public const string DATABASE_SELECTOR_LIST = "database-selector-list";
        public const string DATABASE_SELECTOR = "database-selector";
        public const string NAME = "name";
        public const string LEFT_PANEL = "left-panel";
        public const string RIGHT_PANEL = "right-panel";
        public const string TEMPLATE_DROPDOWN = "template-dropdown";
        public const string NAME_DROPDOWN = "name-dropdown";
        public const string SETTINGS_GROUP = "settings-group";
        public const string ICON_BUTTON = "icon-button";

        public const string UNDEFINED = "<Undefined>";
    }

    internal static class PrefKeys
    {
        private const string BASE = "database_collection_settings";

        public const string SELECTED_INDEX = $"{BASE}__selected_index";
    }

    internal static class DirectoryAPI
    {
        public static RootPath ProjectPath => EditorAPI.ProjectPath;

        public static void OpenFilePanel(TextField textField, string title, string[] filters)
        {
            var oldPath = ProjectPath.GetFileAbsolutePath(textField.value);
            var newPath = EditorUtility.OpenFilePanelWithFilters(title, oldPath, filters);

            if (string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }

            textField.value = ProjectPath.GetRelativePath(newPath);
        }

        public static void OpenFolderPanel(TextField textField, string title)
        {
            var oldPath = ProjectPath.GetFolderAbsolutePath(textField.value);
            var newPath = EditorUtility.OpenFolderPanel(title, oldPath, string.Empty);

            if (string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }

            textField.value = ProjectPath.GetRelativePath(newPath);
        }
    }
}
