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
using EncosyTower.Search;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    public class CheckBoxWindow : EditorWindow
    {
        public static readonly string ListViewUssClassName = "list-view";
        public static readonly string SelectButtonUssClassName = "select-button";
        public static readonly string SearchFieldUssClassName = "search-field";
        public static readonly string ToolFieldUssClassName = "tool-field";
        public static readonly string AcceptButtonUssClassName = "accept-button";
        public static readonly string CancelButtonUssClassName = "cancel-button";
        public static readonly string ButtonFieldUssClassName = "button-field";
        public static readonly string ItemAsHeaderUssClassName = "item-as-header";

        public delegate void OnOk(IReadOnlyList<IItemInfo> items);

        public delegate void OnOk2(IReadOnlyList<IItemInfo> items, object userData);

        public delegate VisualElement OnMakeItem();

        public delegate void OnBindItem(VisualElement element, int index, IReadOnlyList<IItemInfo> items);

        public delegate void OnCreateWindow(CheckBoxWindow window);

        public delegate int ItemHeight();

        [SerializeField] private ThemeStyleSheet _themeStyleSheet;
        [SerializeField] private StyleSheet _darkThemeStyleSheet;
        [SerializeField] private StyleSheet _lightThemeStyleSheet;

        private readonly List<IItemInfo> _itemInfoList = new();

        private IReadOnlyList<IItemInfo> _items;
        private OnMakeItem _onMakeItem;
        private OnBindItem _onBindItem;
        private OnOk _onOk;
        private OnOk2 _onOk2;
        private object _userData;

        private ListView _listView;

        public static void OpenWindow(
              string title
            , IReadOnlyList<IItemInfo> items
            , OnOk onOk
            , OnMakeItem onMakeItem
            , OnBindItem onBindItem = null
            , OnCreateWindow onCreateWindow = null
        )
        {
            var window = GetWindow<CheckBoxWindow>();
            window.Initialize(title, items, onMakeItem, onBindItem, onOk, null, null, onCreateWindow);
            window.ShowAuxWindow();
        }

        public static void OpenWindow(
              string title
            , IReadOnlyList<IItemInfo> items
            , object userData
            , OnOk2 onOk2
            , OnMakeItem onMakeItem
            , OnBindItem onBindItem = null
            , OnCreateWindow onCreateWindow = null
        )
        {
            var window = GetWindow<CheckBoxWindow>();
            window.Initialize(title, items, onMakeItem, onBindItem, null, userData, onOk2, onCreateWindow);
            window.ShowAuxWindow();
        }

        private void Initialize(
              string title
            , IReadOnlyList<IItemInfo> items
            , OnMakeItem onMakeItem
            , OnBindItem onBindItem = null
            , OnOk onOk = null
            , object userData = null
            , OnOk2 onOk2 = null
            , OnCreateWindow onCreateWindow = null
        )
        {
            titleContent = new(title);
            wantsMouseMove = true;

            _items = items ?? Array.Empty<ItemInfo>();
            _onMakeItem = onMakeItem;
            _onBindItem = onBindItem;
            _onOk = onOk;
            _onOk2 = onOk2;
            _userData = userData;

            OnCreateGUI(onCreateWindow);
        }

        private void OnCreateGUI(OnCreateWindow onCreateWindow)
        {
            rootVisualElement.WithStyleSheet(_themeStyleSheet);
            rootVisualElement.WithEditorStyleSheet(_darkThemeStyleSheet, _lightThemeStyleSheet);

            _listView = null;

            _itemInfoList.Clear();
            _itemInfoList.AddRange(_items);

            if (_itemInfoList == null)
            {
                rootVisualElement.Add(new Label("No item"));
                return;
            }

            var selectButton = new Button(SelectButton_OnClick) {
                text = "Select all",
            };

            selectButton.AddToClassList(SelectButtonUssClassName);

            var deselectButton = new Button(DeselectButton_OnClick) {
                text = "Deselect all"
            };

            deselectButton.AddToClassList(SelectButtonUssClassName);

            var searchField = new ToolbarSearchField();
            searchField.AddToClassList(SearchFieldUssClassName);
            searchField.RegisterValueChangedCallback(SearchField_OnValueChanged);

            var toolField = new VisualElement();
            toolField.AddToClassList(ToolFieldUssClassName);
            toolField.Add(selectButton);
            toolField.Add(deselectButton);
            toolField.Add(searchField);

            rootVisualElement.Add(toolField);

            _listView = new() {
                itemsSource = _itemInfoList,
                fixedItemHeight = 22.5f,
                makeItem = ListView_OnMakeItem,
                bindItem = ListView_OnBindItem,
            };

            _listView.AddToClassList(ListViewUssClassName);

            rootVisualElement.Add(_listView);

            var acceptButton = new Button(AcceptButton_OnClick) {
                text = "OK",
            };

            acceptButton.AddToClassList(AcceptButtonUssClassName);

            var cancelButton = new Button(CancelButton_OnClick) {
                text = "Cancel"
            };

            cancelButton.AddToClassList(CancelButtonUssClassName);

            var buttonField = new VisualElement();
            buttonField.AddToClassList(ButtonFieldUssClassName);
            buttonField.Add(acceptButton);
            buttonField.Add(cancelButton);

            rootVisualElement.Add(buttonField);

            onCreateWindow?.Invoke(this);
        }

        private void SearchField_OnValueChanged(ChangeEvent<string> evt)
        {
            FuzzySearchAPI.Search(_items, evt.newValue, _itemInfoList, new SearchValidator());
            _listView?.Rebuild();
        }

        private void SelectButton_OnClick()
        {
            foreach (var item in _items)
            {
                item.IsChecked = true;
            }
        }

        private void DeselectButton_OnClick()
        {
            foreach (var item in _items)
            {
                item.IsChecked = false;
            }
        }

        private void AcceptButton_OnClick()
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

        private void CancelButton_OnClick()
        {
            Close();
        }

        private VisualElement ListView_OnMakeItem()
            => _onMakeItem();

        private void ListView_OnBindItem(VisualElement element, int index)
            => _onBindItem?.Invoke(element, index, _itemInfoList);

        public interface IItemInfo
        {
            string Name { get; }

            bool IsChecked { get; set; }

            bool IsHeader { get; }
        }

        public sealed class ItemInfo : IItemInfo
        {
            public string Name { get; }

            public bool IsChecked { get; set; }

            public bool IsHeader { get; set; }

            public ItemInfo(string name, bool isChecked, bool isHeader = false)
            {
                Name = name;
                IsChecked = isChecked;
                IsHeader = isHeader;
            }
        }

        private readonly struct SearchValidator : ISearchValidator<IItemInfo>
        {
            public bool IsSearchable(IItemInfo item)
                => item.IsHeader;

            public string GetSearchableString(IItemInfo item)
                => item.Name;
        }
    }
}

#endif
