#if UNITY_EDITOR

namespace EncosyTower.VisualCommands.Editor
{
    public static class Constants
    {
        private const string ROOT = "Assets/EncosyTower";

#if UNITY_6000_0_OR_NEWER
        public const string PROJECT_SETTINGS_STYLE_SHEET = "StyleSheets/ProjectSettings/ProjectSettingsCommon.uss";
#else
        private const string CORE_ROOT = $"{ROOT}/EncosyTower.Core/UIElements/Resources";
        public const string PROJECT_SETTINGS_STYLE_SHEET = $"{CORE_ROOT}/Common_2022_3.uss";
#endif

        private const string MODULE_ROOT = $"{ROOT}/EncosyTower.VisualCommands";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(VisualCommandWindow);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        public const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        public const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";
    }
}

#endif
