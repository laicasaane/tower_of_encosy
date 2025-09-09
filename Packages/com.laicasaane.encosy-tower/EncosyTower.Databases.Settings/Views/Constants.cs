using EncosyTower.Editor.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal static class Constants
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Databases.Settings";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(DatabaseCollectionSettings);

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
        public const string PRESET_DROPDOWN = "preset-dropdown";
        public const string NAME_DROPDOWN = "name-dropdown";
        public const string SETTINGS_GROUP = "settings-group";
        public const string ICON_BUTTON = "icon-button";

        public const string UNDEFINED = "<Undefined>";
    }
}
