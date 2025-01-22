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
using System.Linq;
using EncosyTower.Modules.Editor.AssemblyDefs;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor
{
    public class CheckBoxWindowUiToolkit : EditorWindow
    {
        public delegate void OnOk(IReadOnlyList<IItemInfo> items);

        public delegate void OnOk2(IReadOnlyList<IItemInfo> items, object userData);

        public delegate void DrawItem(Rect rect, GUIContent label, int index, IItemInfo item);

        public delegate int ItemHeight();

        private const int ITEM_HEIGHT = 18;

        #region Initialization
        public static void OpenWindow(
              string title
            , IReadOnlyList<IItemInfo> items
            , OnOk onOk
            , OverrideCallbacks callbacks = default
        )
        {
            var window = GetWindow<CheckBoxWindowUiToolkit>();
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
            var window = GetWindow<CheckBoxWindowUiToolkit>();
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

            OpenGUI();
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
        #endregion

        private void OpenGUI()
        {
            ListView referenceList = null;

            List<IItemInfo> items = _items.ToList();

            if (items == null)
            {
                rootVisualElement.Add(new Label("No item"));
                return;
            }

            #region Toolbar
            Button selectAllButton = new Button(() => { foreach (var x in _items) x.IsChecked = true; })
            {
                style = { width = 80 },
                text = "Select all",
            };
            Button deselectAllButton = new Button(() => { foreach (var x in _items) x.IsChecked = false; })
            {
                style = { width = 80, marginLeft = 0 },
                text = "Deselect all"
            };
            ToolbarSearchField toolbarSearchField = new ToolbarSearchField()
            {
                style = { flexGrow = 1 }
            };
            toolbarSearchField.RegisterValueChangedCallback(evt => {
                _filter = evt.newValue;
                var hasFilter = string.IsNullOrWhiteSpace(_filter) == false;

                items.Clear();
                foreach (var item in _items)
                {
                    var name = item.Name ?? string.Empty;

                    if (item.Separator)
                    {
                        items.Add(item);
                        continue;
                    }
                    else if (hasFilter && string.IsNullOrEmpty(name) == false &&
                    UnityEditor.Search.FuzzySearch.FuzzyMatch(_filter, name) == false) continue;

                    items.Add(item);
                }

                if (referenceList != null) referenceList.Rebuild();
            });
            VisualElement toolField = new VisualElement()
            {
                style =
                {
                    minHeight = 25f,
                    maxHeight = 25f,
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    paddingTop = 2.5f,
                    paddingBottom = 2.5f
                }
            };
            toolField.Add(selectAllButton);
            toolField.Add(deselectAllButton);
            toolField.Add(toolbarSearchField);

            rootVisualElement.Add(toolField);
            #endregion
            #region ListView
            referenceList = new ListView() {
                style =
                {
                    flexGrow = 1
                },
                itemsSource = items,
                fixedItemHeight = 22.5f,
                makeItem = () => new ReferenceBar(string.Empty, 0, null),
                bindItem = (element, index) => {
                    var referenceBar = element as ReferenceBar;
                    referenceBar.Update(items[index].Name, index, items[index], items[index].Separator);
                }
            };

            rootVisualElement.Add(referenceList);
            #endregion
            #region Button
            Button okButton = new Button(() =>
            {
                if (_onOk != null) _onOk(_items);
                else _onOk2?.Invoke(_items, _userData);

                Close();
            })
            {
                style = { width = Length.Percent(50), marginRight = 2.5f },
                text = "OK",
            };
            Button cancelButton = new Button(Close)
            {
                style = { width = Length.Percent(50), marginLeft = 0 },
                text = "Cancel"
            };
            VisualElement buttonField = new VisualElement()
            {
                style =
                {
                    minHeight = 25f,
                    maxHeight = 25f,
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    paddingTop = 2.5f,
                    paddingBottom = 2.5f,
                    marginRight = 9
                }
            };
            buttonField.Add(okButton);
            buttonField.Add(cancelButton);

            rootVisualElement.Add(buttonField);
            #endregion
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

        private class ReferenceBar : VisualElement
        {
            public bool IsSeparator = false;
            public IItemInfo Item;

            private Toggle _toggle;
            private Label _label;
            private ObjectField _field;
            private VisualElement _bar;

            public ReferenceBar(string label, int index, IItemInfo item, bool isSeparator = false)
            {
                this.style.flexDirection = FlexDirection.Row;
                this.style.flexGrow = 1;
                this.style.alignItems = Align.Center;

                _toggle = new Toggle(string.Empty) { style = { maxWidth = 22.5f } };
                _toggle.RegisterValueChangedCallback(evt => RecheckItem(evt.newValue));

                this.Add(_toggle);

                _bar = new VisualElement() {
                    style = { flexGrow = 1, flexDirection = FlexDirection.Row, alignItems = Align.Center, paddingRight = 5, minHeight = 22.5f }
                };

                _label = new Label(label) { style = { width = Length.Percent(50) } };
                _field = new ObjectField(string.Empty) {
                    style = { width = Length.Percent(50), paddingRight = 0, paddingLeft = 0 },
                    objectType = typeof(AssemblyDefinitionAsset)
                };

                _bar.Add(_label);
                _bar.Add(_field);

                this.Add(_bar);

                Update(label, index, item, isSeparator);
            }

            public void Update(string label, int index, IItemInfo item, bool isSeparator)
            {
                IsSeparator = isSeparator;
                Item = item;

                if (item != null)
                {
                    if (IsSeparator)
                    {
                        this.focusable = false;

                        _toggle.style.display = DisplayStyle.None;
                        _field.style.display = DisplayStyle.None;

                        _label.text = ((AssemblyDefinitionInspector.ItemInfo)item).separatorText;
                        _label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        _label.style.unityTextAlign = TextAnchor.MiddleCenter;
                        _label.style.width = Length.Percent(100);

                        _bar.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                    }
                    else
                    {
                        this.focusable = true;

                        _toggle.style.display = DisplayStyle.Flex;
                        _toggle.value = item.IsChecked;

                        _label.text = label;
                        _label.style.unityTextAlign = TextAnchor.MiddleLeft;
                        _label.style.width = Length.Percent(50);

                        _field.style.display = DisplayStyle.Flex;
                        _field.value = ((AssemblyDefinitionInspector.ItemInfo)item).asset;

                        _bar.style.backgroundColor = Color.clear;
                    }
                }
            }

            private void RecheckItem(bool value)
            {
                Item.IsChecked = value;
            }
        }
    }
}

#endif
