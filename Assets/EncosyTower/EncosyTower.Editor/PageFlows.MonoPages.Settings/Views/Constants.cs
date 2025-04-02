#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.PageFlows.MonoPages;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings.Views
{
    internal static class Constants
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/PageFlows.MonoPages.Settings";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(MonoPageFlowSettings);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
    }
}

#endif
