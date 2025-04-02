#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;

namespace EncosyTower.Editor.VisualDebugging.Commands
{
    public static class Constants
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.VisualDebugging";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(VisualCommandWindow);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        public const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        public const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";
    }
}

#endif
