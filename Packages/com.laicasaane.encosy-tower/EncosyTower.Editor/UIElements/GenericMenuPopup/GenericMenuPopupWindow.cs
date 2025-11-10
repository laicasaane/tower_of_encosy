#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EncosyTower.Pooling;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    internal partial class GenericMenuPopupWindow : EditorWindow
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/UIElements/GenericMenuPopup";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(GenericMenuPopup);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        public const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        public const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";

        public static readonly string MenuPopupUssClassName = "menu-popup";
        public static readonly string MenuTitleUssClassName = "menu-title";
        public static readonly string MenuTooltipUssClassName = "menu-tooltip";
        public static readonly string SearchFieldUssClassName = "search-field";
        public static readonly string ContentAreaUssClassName = "content-area";
        public static readonly string ScrollViewUssClassName = "scroll-view";
        public static readonly string SearchLabelUssClassName = "search-label";

        private static readonly GenericMenuPopup s_defaultPopup = new(new MenuItemNode(), "");

        private GenericMenuPopup _popup = s_defaultPopup;
        private MenuItemNode _currentNode;
        private string _tooltip;
        private string _search;
        private int _contentHeight;
        private float _height;

        private Label _menuTitle;
        private Label _menuTooltip;
        private ToolbarSearchField _searchField;
        private ScrollView _scrollView;

        public void Initialize([NotNull] GenericMenuPopup popup)
        {
            _popup = popup;
        }

        private void CreateGUI()
        {
            var popup = _popup ??= s_defaultPopup;
            _currentNode = popup.rootNode;

            rootVisualElement.WithEditorStyleSheet(THEME_STYLE_SHEET);
            rootVisualElement.WithEditorStyleSheet(STYLE_SHEET_DARK, STYLE_SHEET_LIGHT);
            rootVisualElement.AddToClassList(MenuPopupUssClassName);

            rootVisualElement.Clear();

            if (popup.showTitle)
            {
                DrawTitle(24);
            }

            if (popup.showSearch)
            {
                DrawSearch(30);
            }

            DrawScrollView();

            DrawMenuItems();

            if (popup.showTooltip)
            {
                DrawTooltip();
            }

            if (popup.resizeToContent)
            {
                _height = Mathf.Min(_contentHeight, popup.maxHeight);
            }

            _searchField?.Focus();
        }

        private void OnLostFocus()
        {
            if (this.IsValid())
            {
                Close();
            }
        }

        private void DrawTitle(int height)
        {
            _contentHeight += height;

            _menuTitle = new(_popup.title);
            _menuTitle.AddToClassList(MenuTitleUssClassName);

            rootVisualElement.Add(_menuTitle);
        }

        private void DrawSearch(int height)
        {
            _contentHeight += height;

            _searchField = new();
            _searchField.AddToClassList(SearchFieldUssClassName);
            _searchField.RegisterValueChangedCallback(SearchField_OnValueChaned);

            rootVisualElement.Add(_searchField);
        }

        private void DrawTooltip()
        {
            if (_menuTooltip == null)
            {
                _contentHeight += 20;

                _menuTooltip = new(_tooltip);
                _menuTooltip.AddToClassList(MenuTooltipUssClassName);

                rootVisualElement.Add(_menuTooltip);
            }
            else
            {
                _menuTooltip.text = _tooltip;
            }
        }

        private void DrawScrollView()
        {
            _scrollView = new();
            _scrollView.AddToClassList(ScrollViewUssClassName);

            rootVisualElement.Add(_scrollView);
        }

        private void DrawMenuItems()
        {
            _scrollView.Clear();

            if (string.IsNullOrWhiteSpace(_search) || _search.Length < 2)
            {
                DrawNodeTree();
            }
            else
            {
                DrawNodeSearch();
            }
        }

        private void DrawNodeTree()
        {
            _scrollView.Clear();

            if (_currentNode != _popup.rootNode)
            {
                var path = _currentNode.BuildPath(new StringBuilder()).ToString();
                var menuNode = new MenuNode(_currentNode, true);

                menuNode.OnClick += () => {
                    _currentNode = _currentNode.Parent;

                    DrawMenuItems();
                };

                menuNode.OnHover += () => {
                    _tooltip = menuNode.node?.content.tooltip;

                    DrawTooltip();
                };

                menuNode.OnExit += () => {
                    _tooltip = string.Empty;

                    DrawTooltip();
                };

                _scrollView.Add(menuNode);

                _contentHeight += 24;
            }

            foreach (var node in _currentNode.Nodes)
            {
                if (node.separator)
                {
                    _scrollView.Add(new VisualElement() { style = { height = 4 } });
                    _contentHeight += 4;
                    continue;
                }

                var menuNode = new MenuNode(node, false, node.Nodes.Count > 0);

                menuNode.OnClick += () => {
                    if (node.Nodes.Count > 0)
                    {
                        _currentNode = node;
                        DrawMenuItems();
                    }
                    else if (node.func != null || node.func2 != null)
                    {
                        node?.Execute();
                        Close();
                    }
                };

                menuNode.OnHover += () => {
                    _tooltip = menuNode.node?.content.tooltip;
                    DrawTooltip();
                };

                menuNode.OnExit += () => {
                    _tooltip = string.Empty;
                    DrawTooltip();
                };

                _scrollView.Add(menuNode);
                _contentHeight += 24;
            }
        }

        private void DrawNodeSearch()
        {
            _scrollView.Clear();

            using var _ = FasterListPool<MenuItemNode>.Get(out var searchResult);

            _popup.rootNode.Search(_search, searchResult);

            searchResult.Sort(static (n1, n2) => {
                var sb = new StringBuilder();
                var p1 = n1.Parent.BuildPath(sb.Clear()).ToString();
                var p2 = n2.Parent.BuildPath(sb.Clear()).ToString();

                return string.Equals(p1, p2, StringComparison.Ordinal)
                    ? string.CompareOrdinal(n1.Name, n2.Name)
                    : string.CompareOrdinal(p1, p2);
            });

            var lastPath = "";
            var sb = new StringBuilder();

            foreach (var node in searchResult)
            {
                var nodePath = node.Parent.BuildPath(sb.Clear()).ToString();

                if (string.Equals(nodePath, lastPath, StringComparison.Ordinal) == false)
                {
                    lastPath = nodePath;

                    var searchLabel = new Label(nodePath);
                    searchLabel.AddToClassList(SearchLabelUssClassName);

                    _scrollView.Add(searchLabel);
                    _contentHeight += 24;
                }

                var menuNode = new MenuNode(node);

                menuNode.OnClick += () => {
                    if (node.Nodes.Count > 0)
                    {
                        _currentNode = node;
                        DrawMenuItems();
                    }
                    else if (node.func != null || node.func2 != null)
                    {
                        node?.Execute();
                        Close();
                    }
                };

                menuNode.OnHover += () => {
                    _tooltip = menuNode.node?.content.tooltip;
                    DrawTooltip();
                };

                menuNode.OnExit += () => {
                    _tooltip = string.Empty;
                    DrawTooltip();
                };

                _scrollView.Add(menuNode);
                _contentHeight += 24;
            }

            if (searchResult.Count == 0)
            {
                _scrollView.Add(new Label("No result found for specified search."));
            }
        }

        private void SearchField_OnValueChaned(ChangeEvent<string> evt)
        {
            _search = evt.newValue;

            DrawMenuItems();
        }
    }
}

#endif
