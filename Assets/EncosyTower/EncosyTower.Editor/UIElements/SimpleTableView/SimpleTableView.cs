#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    public class SimpleTableView<TData> : MultiColumnListView
    {
        public const float DEFAULT_ROW_HEIGHT = 20;

        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/UIElements/SimpleTableView";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(SimpleTableView<TData>);

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

        public float rowHeight = 0;
        public bool showDivider = true;

        private List<TData> _data = new();

        private readonly List<ColumnDef> _columns = new();
        private readonly List<Label> _sortIndicators = new();

        public SimpleTableView()
        {
            this.ApplyEditorStyleSheet(THEME_STYLE_SHEET);
            this.ApplyEditorStyleSheet(STYLE_SHEET_DARK, STYLE_SHEET_LIGHT);

            showBoundCollectionSize = true;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            AddToClassList(TableViewUssClassName);

            selectionType = SelectionType.None;
        }

        public void ClearColumn()
        {
            _data.Clear();
            itemsSource = null;
            Rebuild();
        }

        public ColumnDef AddColumn(
              string title
            , float minWidth
            , Func<VisualElement> makeCell
            , Action<VisualElement, TData> bindCell
        )
        {
            var column = new ColumnDef {
                _title = title,
                _minWidth = minWidth,
                _makeCell = makeCell,
                _bindCell = bindCell
            };

            _columns.Add(column);

            return column;
        }

        public void SetData([NotNull] List<TData> data)
        {
            itemsSource = _data = data;
        }

        public void DrawTable()
        {
            columns.Clear();

            foreach (var columnDef in _columns)
            {
                var column = CreateColumn(columnDef);

                CreateHeader(column, columnDef);

                columns.Add(column);
            }
        }

        private Column CreateColumn(ColumnDef columnDef)
        {
            var column = new Column {
                title = columnDef._title,
                minWidth = columnDef._minWidth,
                stretchable = true,
                makeCell = columnDef._makeCell,
            };

            column.bindCell = (element, index) => {
                if (index < _data.Count)
                {
                    var parent = element.parent;
                    var grandParent = parent.parent;
                    grandParent.AddToClassList(TableRowUssClassName);
                    grandParent.AddToClassList(index % 2 == 0 ? EvenRowUssClassName : OddRowUssClassName);

                    parent.AddToClassList(TableRowUssClassName);

                    if (rowHeight != 0)
                    {
                        parent.style.minHeight = rowHeight;
                    }

                    if (column != columns[^1] && showDivider)
                    {
                        parent.style.borderRightWidth = 1;
                    }

                    columnDef._bindCell(element, _data[index]);
                }
            };

            return column;
        }

        private void CreateHeader(Column column, ColumnDef columnDef)
        {
            column.makeHeader = () => {
                var headerLabel = new Label(columnDef._title);
                {
                    headerLabel.focusable = true;
                    headerLabel.style.unityTextAlign = columnDef._headerAlignment;
                    headerLabel.AddToClassList(HeaderLabelUssClassName);
                }

                var sortIndicator = new Label("▲");
                {
                    sortIndicator.AddToClassList(SortIndicatorUssClassName);
                    sortIndicator.style.display = DisplayStyle.None;
                    _sortIndicators.Add(sortIndicator);
                }

                var headerContainer = new VisualElement();
                {
                    headerContainer.AddToClassList(HeaderContainerUssClassName);
                    headerContainer.AddToClassList(TableHeaderUssClassName);
                    headerContainer.AddToClassList(TableRowUssClassName);

                    if (rowHeight != 0)
                    {
                        headerContainer.style.minHeight = rowHeight;
                    }

                    headerContainer.Add(headerLabel);
                    headerContainer.Add(sortIndicator);

                    if (columnDef._onSort != null)
                    {
                        headerLabel.RegisterCallback<PointerDownEvent>(_ => {
                            foreach (var indicator in _sortIndicators)
                            {
                                indicator.style.display = DisplayStyle.None;
                            }

                            _data.Sort((a, b) => columnDef._isAscending
                                ? columnDef._onSort(a, b)
                                : -columnDef._onSort(a, b));

                            columnDef._isAscending = !columnDef._isAscending;
                            sortIndicator.text = columnDef._isAscending ? "▲" : "▼";
                            sortIndicator.style.display = DisplayStyle.Flex;

                            Rebuild();
                        });
                    }
                }

                return headerContainer;
            };
        }

        public class ColumnDef
        {
            internal string _title;
            internal float _minWidth;
            internal float _maxWidth;
            internal string _tooltip;
            internal bool _autoResize;
            internal TextAnchor _headerAlignment = TextAnchor.MiddleLeft;

            internal Func<VisualElement> _makeCell;
            internal Action<VisualElement, TData> _bindCell;
            internal Comparison<TData> _onSort;
            internal bool _isAscending = true;

            public ColumnDef SetMaxWidth(float maxWidth)
            {
                _maxWidth = maxWidth;
                return this;
            }

            public ColumnDef SetTooltip(string tooltip)
            {
                _tooltip = tooltip;
                return this;
            }

            public ColumnDef SetAutoResize(bool autoResize)
            {
                _autoResize = autoResize;
                return this;
            }

            public ColumnDef SetHeaderAlignment(TextAnchor alignment)
            {
                _headerAlignment = alignment;
                return this;
            }

            public ColumnDef SetSorting(Comparison<TData> sortFunction)
            {
                _onSort = sortFunction;
                return this;
            }
        }
    }
}

#endif
