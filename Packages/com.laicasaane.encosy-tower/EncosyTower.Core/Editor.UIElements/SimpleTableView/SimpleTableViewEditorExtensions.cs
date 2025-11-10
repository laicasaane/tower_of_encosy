#if UNITY_EDITOR

using System.Diagnostics.CodeAnalysis;
using EncosyTower.UIElements;

namespace EncosyTower.Editor.UIElements
{
    public static class SimpleTableViewEditorExtensions
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Core/UIElements/SimpleTableView";
        private const string EDITOR_MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Core/Editor.UIElements/SimpleTableView";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string EDITOR_STYLE_SHEETS_PATH = $"{EDITOR_MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(SimpleTableView<int>);

        private const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        private const string STYLE_SHEET_DARK = $"{EDITOR_STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        private const string STYLE_SHEET_LIGHT = $"{EDITOR_STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";

        /// <summary>
        /// Applies built-in editor style sheets to <see cref="SimpleTableView{TItem}"/>.
        /// </summary>
        public static SimpleTableView<TItem> WithEditorStyleSheets<TItem>(
            [NotNull] this SimpleTableView<TItem> self
        )
        {
            self.WithEditorStyleSheet(THEME_STYLE_SHEET);
            self.WithEditorStyleSheet(STYLE_SHEET_DARK, STYLE_SHEET_LIGHT);

            return self;
        }
    }
}

#endif
