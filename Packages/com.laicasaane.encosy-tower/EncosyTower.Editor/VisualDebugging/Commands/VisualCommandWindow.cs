#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.VisualDebugging.Commands;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.VisualDebugging.Commands
{
    internal class VisualCommandWindow : EditorWindow
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/VisualDebugging";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/Commands";
        private const string FILE_NAME = nameof(VisualCommandWindow);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        public const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        public const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";

        private VisualCommanderView _view;

        [MenuItem("Encosy Tower/Visual Commands", priority = 86_73_00_00)]
        public static void OpenVisualCommandWindow()
        {
            var window = GetWindow<VisualCommandWindow>();
            window.titleContent = new GUIContent("Visual Commands");
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.WithEditorStyleSheet(THEME_STYLE_SHEET);
            root.WithEditorStyleSheet(STYLE_SHEET_DARK, STYLE_SHEET_LIGHT);

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
