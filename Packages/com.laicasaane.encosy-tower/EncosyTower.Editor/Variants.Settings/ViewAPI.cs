using EncosyTower.Editor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Variants.Settings
{
    internal static class ViewAPI
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/Variants.Settings";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(VariantTypeSettings);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";

        public static void ApplyStyleSheetsTo(VisualElement root, bool withBuiltInStyleSheet)
        {
            if (withBuiltInStyleSheet)
            {
                root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
            }

            root.WithEditorStyleSheet(THEME_STYLE_SHEET);
        }
    }
}
