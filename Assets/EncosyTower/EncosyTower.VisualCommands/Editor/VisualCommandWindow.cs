#if UNITY_EDITOR

using EncosyTower.Editor;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.VisualCommands.Editor
{
    internal class VisualCommandWindow : EditorWindow
    {
        private VisualCommanderView _view;

        [MenuItem("Encosy Tower/Visual Commands")]
        public static void OpenVisualCommandWindow()
        {
            var window = GetWindow<VisualCommandWindow>();
            window.titleContent = new GUIContent("Visual Commands");
            window.Show();
        }

        private void CreateGUI()
        {
            ApplyStyleSheetsTo(rootVisualElement);

            _view = VisualCommanderAPI.CreateView(rootVisualElement, 150f);
        }

        private void OnDestroy()
        {
            _view?.Dispose();
            _view = null;
        }

        private static void ApplyStyleSheetsTo(VisualElement root, string builtinStyleSheet = "")
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
    }
}

#endif
