#if UNITY_EDITOR

#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE1006 // Naming Styles

// https://github.com/redclock/SimpleEditorTableView/blob/main/SimpleEditorTableView.cs

// Copyright (c) 2024 Red Games
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLRDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USER OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace EncosyTower.Modules.Editor
{
    public class SimpleTableView<TData>
    {
        private MultiColumnHeaderState _multiColumnHeaderState;
        private MultiColumnHeader _multiColumnHeader;
        private MultiColumnHeaderState.Column[] _columns;
        private readonly Color _lighterColor = Color.white * 0.3f;
        private readonly Color _darkerColor = Color.white * 0.1f;

        private Vector2 _scrollPosition;
        private bool _columnResized;
        private bool _sortingDirty;

        public delegate void DrawItem(Rect rect, TData item);

        public class ColumnDef
        {
            internal MultiColumnHeaderState.Column _column;
            internal DrawItem _onDraw;
            internal Comparison<TData> _onSort;

            public ColumnDef SetMaxWidth(float maxWidth)
            {
                _column.maxWidth = maxWidth;
                return this;
            }

            public ColumnDef SetTooltip(string tooltip)
            {
                _column.headerContent.tooltip = tooltip;
                return this;
            }

            public ColumnDef SetAutoResize(bool autoResize)
            {
                _column.autoResize = autoResize;
                return this;
            }

            public ColumnDef SetAllowToggleVisibility(bool allow)
            {
                _column.allowToggleVisibility = allow;
                return this;
            }

            public ColumnDef SetSorting(Comparison<TData> onSort)
            {
                _onSort = onSort;
                _column.canSort = true;
                return this;
            }
        }

        private readonly List<ColumnDef> _columnDefs = new List<ColumnDef>();

        public void ClearColumns()
        {
            _columnDefs.Clear();
            _columnResized = true;
        }

        public ColumnDef AddColumn(string title, int minWidth, DrawItem onDrawItem)
        {
            ColumnDef columnDef = new ColumnDef()
            {
                _column = new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = minWidth,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent(title),
                    headerTextAlignment = TextAlignment.Left,
                },
                _onDraw = onDrawItem
            };

            _columnDefs.Add(columnDef);
            _columnResized = true;
            return columnDef;
        }

        private void ReBuild()
        {
            _columns = _columnDefs.Select(static def => def._column).ToArray();
            _multiColumnHeaderState = new MultiColumnHeaderState(_columns);
            _multiColumnHeader = new MultiColumnHeader(_multiColumnHeaderState);
            _multiColumnHeader.visibleColumnsChanged += static multiColumnHeader => multiColumnHeader.ResizeToFit();
            _multiColumnHeader.sortingChanged += _ => _sortingDirty = true;
            _multiColumnHeader.ResizeToFit();
            _columnResized = false;
        }

        public void ResizeToFit()
            => _multiColumnHeader?.ResizeToFit();

        public void DrawTableGUI(
              TData[] data
            , float maxHeight = float.MaxValue
            , float rowHeight = -1
            , float rowWidth = -1
        )
        {
            if (_multiColumnHeader == null || _columnResized)
                ReBuild();

            if (rowWidth < 0)
                rowWidth = _multiColumnHeaderState.widthOfAllVisibleColumns;

            if (rowHeight < 0)
                rowHeight = EditorGUIUtility.singleLineHeight;

            Rect headerRect = GUILayoutUtility.GetRect(rowWidth, rowHeight);
            _multiColumnHeader!.OnGUI(headerRect, xScroll: 0.0f);

            var sumWidth = rowWidth;
            var sumHeight = rowHeight * data.Length + GUI.skin.horizontalScrollbar.fixedHeight;

            UpdateSorting(data);

            Rect scrollViewPos = GUILayoutUtility.GetRect(0, sumWidth, 0, maxHeight);
            Rect viewRect = new Rect(0, 0, sumWidth, sumHeight);

            _scrollPosition = GUI.BeginScrollView(
                position: scrollViewPos,
                scrollPosition: _scrollPosition,
                viewRect: viewRect,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false
            );

            EditorGUILayout.BeginVertical();

            for (var row = 0; row < data.Length; row++)
            {
                Rect rowRect = new Rect(0, rowHeight * row, rowWidth, rowHeight);

                EditorGUI.DrawRect(rect: rowRect, color: row % 2 == 0 ? _darkerColor : _lighterColor);

                for (var col = 0; col < _columns.Length; col++)
                {
                    if (_multiColumnHeader.IsColumnVisible(col))
                    {
                        var visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(col);
                        Rect cellRect = _multiColumnHeader.GetCellRect(visibleColumnIndex, rowRect);
                        _columnDefs[col]._onDraw(cellRect, data[row]);
                    }
                }
            }

            EditorGUILayout.EndVertical();
            GUI.EndScrollView(handleScrollWheel: true);
        }

        private void UpdateSorting(TData[] data)
        {
            if (_sortingDirty)
            {
                var sortIndex = _multiColumnHeader.sortedColumnIndex;
                if (sortIndex >= 0)
                {
                    var sortCompare = _columnDefs[sortIndex]._onSort;
                    var ascending = _multiColumnHeader.IsSortedAscending(sortIndex);

                    Array.Sort(data, ((a, b) => {
                        var r = sortCompare(a, b);
                        return ascending ? r : -r;
                    }));
                }

                _sortingDirty = false;
            }
        }
    }
}

#endif
