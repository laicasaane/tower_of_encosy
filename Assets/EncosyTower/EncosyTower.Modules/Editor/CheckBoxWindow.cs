// https://github.com/baba-s/Kogane.CheckBoxWindow

// MIT License
//
// Copyright (c) 2022 baba_s
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace EncosyTower.Modules.Editor
{
    public class CheckBoxWindow : EditorWindow
    {
        public delegate void OnOk(IReadOnlyList<IItemInfo> items);

        public delegate void OnOk2(IReadOnlyList<IItemInfo> items, object userData);

        public delegate void DrawItem(Rect rect, GUIContent label, int index, IItemInfo item);

        public delegate int ItemHeight();

        private const int ITEM_HEIGHT = 18;

        public static void OpenWindow(
              string title
            , IReadOnlyList<IItemInfo> items
            , OnOk onOk
            , OverrideCallbacks callbacks = default
        )
        {
            var window = GetWindow<CheckBoxWindow>();
            window.Initialize(title, items, onOk, null, null, callbacks);
            window.ShowAuxWindow();
        }

        public static void OpenWindow(
              string title
            , IReadOnlyList<IItemInfo> items
            , object userData
            , OnOk2 onOk2
            , OverrideCallbacks callbacks = default
        )
        {
            var window = GetWindow<CheckBoxWindow>();
            window.Initialize(title, items, null, userData, onOk2, callbacks);
            window.ShowAuxWindow();
        }

        private IReadOnlyList<IItemInfo> _items;
        private OnOk _onOk;
        private OnOk2 _onOk2;
        private object _userData;
        private OverrideCallbacks _callbacks;

        private SearchField _searchField;
        private string _filter = string.Empty;
        private Vector2 _scrollPosition;
        private GUIStyle _hoverBackStyle;
        private GUIStyle _separatorBackStyle;
        private GUIStyle[] _altBackStyles;

        private readonly GUIContent _itemLabel = new();

        private void Initialize(
              string title
            , IReadOnlyList<IItemInfo> items
            , OnOk onOk = null
            , object userData = null
            , OnOk2 onOk2 = null
            , OverrideCallbacks callbacks = default
        )
        {
            titleContent = new(title);
            wantsMouseMove = true;
            _items = items ?? Array.Empty<ItemInfo>();
            _onOk = onOk;
            _onOk2 = onOk2;
            _userData = userData;
            _callbacks = callbacks;
        }

        protected void OnEnable()
        {
            _separatorBackStyle = new GUIStyle().WithNormalBackground(EditorAPI.GetColor("#4D4D4D", "#AEAEAE"));
            _hoverBackStyle = new GUIStyle().WithNormalBackground(EditorAPI.GetColor("#2C5D87", "#3A72B0"));
            _altBackStyles = new[] {
                new GUIStyle().WithNormalBackground(EditorAPI.GetColor("#383838", "#C8C8C8")),
                new GUIStyle().WithNormalBackground(EditorAPI.GetColor("#3F3F3F", "#CACACA")),
            };
        }

        protected void OnGUI()
        {
            var items = _items;

            if (items == null)
            {
                EditorGUILayout.LabelField("No item");
                return;
            }

            _searchField ??= new();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Select all", GUILayout.Width(80)))
                {
                    foreach (var x in _items)
                    {
                        x.IsChecked = true;
                    }
                }

                if (GUILayout.Button("Deselect all", GUILayout.Width(80)))
                {
                    foreach (var x in _items)
                    {
                        x.IsChecked = false;
                    }
                }

                _filter = _searchField.OnToolbarGUI(_filter);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6f);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                var length = items.Count;
                var filter = _filter;
                var itemLabel = _itemLabel;
                var hoverBackStyle = _hoverBackStyle;
                var altBackStyles = _altBackStyles;
                var separatorBackStyle = _separatorBackStyle;
                var mouseY = (int)Event.current.mousePosition.y;
                var drawItemOverride = _callbacks.DrawItem;
                var drawSeparatorOverride = _callbacks.DrawSeparator;
                var itemHeight = _callbacks.ItemHeight?.Invoke() ?? ITEM_HEIGHT;
                var hasFilter = string.IsNullOrWhiteSpace(filter) == false;
                var y = 0;

                for (var i = 0; i < length; i++)
                {
                    var item = items[i];
                    var name = item.Name ?? string.Empty;
                    itemLabel.text = name;

                    if (hasFilter
                        && string.IsNullOrEmpty(name) == false
                        && UnityEditor.Search.FuzzySearch.FuzzyMatch(filter, name) == false
                    )
                    {
                        continue;
                    }

                    if (item.Separator)
                    {
                        var style = separatorBackStyle;
                        style.fixedHeight = itemHeight;

                        var rect = EditorGUILayout.BeginHorizontal(style);
                        EditorGUILayout.BeginVertical();

                        if (drawSeparatorOverride != null)
                        {
                            drawSeparatorOverride(rect, itemLabel, i, item);
                        }
                        else
                        {
                            EditorGUILayout.Space(itemHeight);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        var isHovering = (mouseY / itemHeight) == y;
                        var style = isHovering ? hoverBackStyle : altBackStyles[i % 2];
                        style.fixedHeight = itemHeight;

                        var rect = EditorGUILayout.BeginHorizontal(style);
                        EditorGUILayout.BeginVertical();
                        {
                            if (drawItemOverride != null)
                            {
                                drawItemOverride(rect, itemLabel, i, item);
                            }
                            else
                            {
                                GUILayout.FlexibleSpace();
                                item.IsChecked = EditorGUILayout.ToggleLeft(itemLabel, item.IsChecked);
                                GUILayout.FlexibleSpace();
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    y++;
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6f);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("OK"))
                {
                    if (_onOk != null)
                    {
                        _onOk(_items);
                    }
                    else
                    {
                        _onOk2?.Invoke(_items, _userData);
                    }

                    Close();
                }

                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public interface IItemInfo
        {
            string Name { get; }

            bool IsChecked { get; set; }

            bool Separator { get; }
        }

        public sealed class ItemInfo : IItemInfo
        {
            public string Name { get; }

            public bool IsChecked { get; set; }

            public bool Separator { get; set; }

            public ItemInfo(string name, bool isChecked, bool separator = false)
            {
                Name = name;
                IsChecked = isChecked;
                Separator = separator;
            }
        }

        public record struct OverrideCallbacks(
              DrawItem DrawItem = null
            , ItemHeight ItemHeight = null
            , DrawItem DrawSeparator = null

        );
    }
}

#endif
