using EncosyTower.Editor;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal static class ViewAPI
    {
        public static void ApplyStyleSheetsTo(VisualElement root, string builtinStyleSheet = "")
        {
            if (string.IsNullOrWhiteSpace(builtinStyleSheet) == false
                && EditorGUIUtility.Load(builtinStyleSheet) is StyleSheet commonUss
                && commonUss.IsValid()
            )
            {
                root.styleSheets.Add(commonUss);
            }

            var settingsTss = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(Constants.THEME_STYLE_SHEET);
            root.styleSheets.Add(settingsTss);

            var styleSheetPalette = EditorAPI.IsDark ? Constants.STYLE_SHEET_DARK : Constants.STYLE_SHEET_LIGHT;
            var settingsPaletteUss = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPalette);
            root.styleSheets.Add(settingsPaletteUss);
        }

        public static VisualElement GetResources()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.STYLE_SHEET_RESOURCES);
            return asset.CloneTree();
        }
    }
}
