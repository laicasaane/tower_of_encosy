using EncosyTower.Editor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal static class ViewAPI
    {
        public static void ApplyStyleSheetsTo(VisualElement root, bool withBuiltInStyleSheet)
        {
            if (withBuiltInStyleSheet)
            {
                root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
            }

            root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);
            root.WithEditorStyleSheet(Constants.STYLE_SHEET_DARK, Constants.STYLE_SHEET_LIGHT);
        }

        public static VisualElement GetResources()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.STYLE_SHEET_RESOURCES);
            return asset.CloneTree();
        }
    }
}
