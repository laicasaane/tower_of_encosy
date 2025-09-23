#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    public class SimpleTableView<TItem> : MultiColumnListView
    {
        public const float DEFAULT_ROW_HEIGHT = 20;

        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/UIElements/SimpleTableView";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(SimpleTableView<TItem>);

        private const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        private const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        private const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";

        public static readonly string TableViewUssClassName = "table-view";
        public static readonly string TableHeaderUssClassName = "table-header";
        public static readonly string HeaderContainerUssClassName = "header-container";
        public static readonly string HeaderLabelUssClassName = "header-label";
        public static readonly string SortIndicatorUssClassName = "sort-indicator";
        public static readonly string TableRowUssClassName = "table-row";
        public static readonly string OddRowUssClassName = "odd-row";
        public static readonly string EvenRowUssClassName = "even-row";

        public bool showDivider = true;
        public bool alternateRows = true;

        private readonly List<Label> _sortIndicators = new();

        private List<TItem> _items = new();

        public SimpleTableView()
        {
            this.WithEditorStyleSheet(THEME_STYLE_SHEET);
            this.WithEditorStyleSheet(STYLE_SHEET_DARK, STYLE_SHEET_LIGHT);

            showBoundCollectionSize = true;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            AddToClassList(TableViewUssClassName);

            selectionType = SelectionType.None;
        }

        public List<TItem> Items
        {
            get => _items;

            set
            {
                if (value == null)
                {
                    _items.Clear();
                    return;
                }

                if (_items == value)
                {
                    return;
                }

                _items = value;
            }
        }

        public Column AddColumn(
              string title
            , float minWidth
            , Func<VisualElement> makeCell = null
            , Action<VisualElement, TItem> bindCell = null
            , ColumnHeader header = default
        )
        {
            var table = this;
            var column = new Column {
                title = title,
                minWidth = minWidth,
                stretchable = true,
            };

            if (makeCell != null)
            {
                column.makeCell = makeCell;
            }

            column.bindCell = (element, index) => {
                if ((uint)index >= (uint)_items.Count)
                {
                    return;
                }

                var parent = element.parent;
                parent.AddToClassList(TableRowUssClassName);

                var grandParent = parent.parent;
                grandParent.AddToClassList(TableRowUssClassName);

                grandParent.RemoveFromClassList(EvenRowUssClassName);
                grandParent.RemoveFromClassList(OddRowUssClassName);

                if (alternateRows)
                {
                    grandParent.AddToClassList(index % 2 == 0 ? EvenRowUssClassName : OddRowUssClassName);
                }
                else
                {
                    grandParent.AddToClassList(OddRowUssClassName);
                }

                if (column != columns[^1] && showDivider)
                {
                    parent.style.borderRightWidth = 1;
                }

                bindCell?.Invoke(element, _items[index]);
            };

            column.makeHeader = () => {

                var sortIndicator = new Label(GetSortIndicatorIcon(header.IsAscending));
                {
                    sortIndicator.style.display = DisplayStyle.None;
                    sortIndicator.AddToClassList(SortIndicatorUssClassName);

                    _sortIndicators.Add(sortIndicator);
                }

                var label = new Label(column.title);
                {
                    label.focusable = true;
                    label.AddToClassList(HeaderLabelUssClassName);
                    label.userData = new HeaderData {
                        table = table,
                        sortIndicator = sortIndicator,
                        onSort = header.OnSort,
                        isAscending = header.IsAscending,
                    };
                }

                var container = new VisualElement();
                {
                    container.AddToClassList(HeaderContainerUssClassName);
                    container.AddToClassList(TableHeaderUssClassName);
                    container.AddToClassList(TableRowUssClassName);

                    container.Add(label);
                    container.Add(sortIndicator);

                    if (header.OnSort != null)
                    {
                        label.RegisterCallback<PointerDownEvent>(HeaderLabel_OnPointerDown);
                    }
                }

                return container;
            };

            columns.Add(column);

            return column;
        }

        private static string GetSortIndicatorIcon(bool isAscending)
            => isAscending ? "▲" : "▼";

        private static void HeaderLabel_OnPointerDown(PointerDownEvent evt)
        {
            if (evt.currentTarget is not Label label || label.userData is not HeaderData header)
            {
                return;
            }

            var table = header.table;

            foreach (var indicator in table._sortIndicators)
            {
                indicator.style.display = DisplayStyle.None;
            }

            table._items.Sort((a, b) => header.isAscending ? header.onSort(a, b) : -header.onSort(a, b));

            header.isAscending = !header.isAscending;
            header.sortIndicator.text = GetSortIndicatorIcon(header.isAscending);
            header.sortIndicator.style.display = DisplayStyle.Flex;

            table.Rebuild();
        }

        private class HeaderData
        {
            public SimpleTableView<TItem> table;
            public Label sortIndicator;
            public Comparison<TItem> onSort;
            public bool isAscending;
        }

        public readonly record struct ColumnHeader(
              Comparison<TItem> OnSort
            , bool IsAscending = true
        );
    }
}

#endif
