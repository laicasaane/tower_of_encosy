#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using EncosyTower.VisualDebugging.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.VisualDebugging.Commands
{
    internal class VisualCommandWindow : EditorWindow
    {
        [SerializeField] private ThemeStyleSheet _themeStyleSheet;
        [SerializeField] private StyleSheet _darkThemeStyleSheet;
        [SerializeField] private StyleSheet _lightThemeStyleSheet;

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
            root.WithStyleSheet(_themeStyleSheet);
            root.WithEditorStyleSheet(_darkThemeStyleSheet, _lightThemeStyleSheet);

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
