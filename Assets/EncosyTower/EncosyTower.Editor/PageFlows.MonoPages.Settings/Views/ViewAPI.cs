using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings.Views
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
        }

        public static VisualElement GetResources()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.STYLE_SHEET_RESOURCES);
            return asset.CloneTree();
        }
    }
}
