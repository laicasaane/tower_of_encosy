using EncosyTower.PageFlows.MonoPages;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings.Views
{
    internal static class Constants
    {
        private const string ROOT = "Assets/EncosyTower";

#if UNITY_6000_0_OR_NEWER
        public const string PROJECT_SETTINGS_STYLE_SHEET = "StyleSheets/ProjectSettings/ProjectSettingsCommon.uss";
#else
        private const string CORE_ROOT = $"{ROOT}/SuperGame.Core/UIElements/Resources";
        public const string PROJECT_SETTINGS_STYLE_SHEET = $"{CORE_ROOT}/Common_2022_3.uss";
#endif

        private const string MODULE_ROOT = $"{ROOT}/EncosyTower.Editor/PageFlows.MonoPages.Settings";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(MonoPageFlowSettings);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        public const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        public const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";
        public const string STYLE_SHEET_RESOURCES = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Resources.uxml";

        public const string MONO_PAGE_FLOW = "mono-page-flow";
    }
}
