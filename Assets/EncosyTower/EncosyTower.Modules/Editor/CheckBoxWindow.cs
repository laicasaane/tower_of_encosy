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

        public delegate void DrawItemOverride(Rect rect, GUIContent label, int index, IItemInfo item);

        public delegate int ItemHeightOverride();

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
        private GUIStyle _hoverStyle;
        private GUIStyle[] _alternatingRowStyles;

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
            {
                ColorUtility.TryParseHtmlString("#404040", out var darkColor);
                ColorUtility.TryParseHtmlString("#ABABAB", out var lightColor);

                _alternatingRowStyles = new[]
                {
                    CreateGUIStyle(new Color(1, 1, 1, 0f)),
                    CreateGUIStyle(EditorGUIUtility.isProSkin ? darkColor : lightColor),
                };
            }

            {
                ColorUtility.TryParseHtmlString("#2C5D87", out var darkColor);
                ColorUtility.TryParseHtmlString("#3A72B0", out var lightColor);
                _hoverStyle = CreateGUIStyle(EditorGUIUtility.isProSkin ? darkColor : lightColor);
            }
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

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                var length = items.Count;
                var filter = _filter;
                var itemLabel = _itemLabel;
                var hoverStyle = _hoverStyle;
                var alternatingRowStyles = _alternatingRowStyles;
                var mouseY = (int)Event.current.mousePosition.y;
                var drawItemOverride = _callbacks.DrawItem;
                var itemHeight = _callbacks.ItemHeight?.Invoke() ?? ITEM_HEIGHT;
                var y = 0;

                for (var i = 0; i < length; i++)
                {
                    var item = items[i];
                    var name = item.Name;
                    itemLabel.text = name;

                    if (name.Contains(filter, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        continue;
                    }

                    var isHovering = (mouseY / itemHeight) == y;
                    var style = isHovering ? hoverStyle : alternatingRowStyles[i % 2];
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
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    y++;
                }
            }
            EditorGUILayout.EndScrollView();

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

        private static GUIStyle CreateGUIStyle(Color color)
        {
            var background = new Texture2D( 1, 1 );
            background.SetPixel(0, 0, color);
            background.Apply();

            var style = new GUIStyle();
            style.normal.background = background;

            return style;
        }

        public interface IItemInfo
        {
            string Name { get; }

            bool IsChecked { get; set; }
        }

        public sealed class ItemInfo : IItemInfo
        {
            public string Name { get; }

            public bool IsChecked { get; set; }

            public ItemInfo(string name, bool isChecked)
            {
                Name = name;
                IsChecked = isChecked;
            }
        }

        public record struct OverrideCallbacks(
              DrawItemOverride DrawItem = null
            , ItemHeightOverride ItemHeight = null
        );
    }
}

#endif
