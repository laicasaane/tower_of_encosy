#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.VisualDebugging.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.VisualDebugging.Commands
{
    internal class VisualCommandWindow : EditorWindow
    {
        [SerializeField] private ThemeStyleSheet _themeStyleSheet;
        [SerializeField] private StyleSheet _dark;
        [SerializeField] private StyleSheet _light;

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
            var root = rootVisualElement;

            if (Application.isPlaying == false)
            {
                root.styleSheets.Add(_themeStyleSheet);
                root.ApplyEditorStyleSheet(_dark, _light);
            }
            else
            {
                root.ApplyEditorStyleSheet(Constants.THEME_STYLE_SHEET);
                root.ApplyEditorStyleSheet(Constants.STYLE_SHEET_DARK, Constants.STYLE_SHEET_LIGHT);
            }

            _view = VisualCommanderAPI.CreateView(root, 150f);
        }

        private void OnDestroy()
        {
            _view?.Dispose();
            _view = null;
        }
    }
}

#endif
