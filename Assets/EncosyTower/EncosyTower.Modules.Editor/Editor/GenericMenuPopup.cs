#if UNITY_EDITOR

// https://github.com/pshtif/GenericMenuPopup/tree/main/Assets/BinaryEgo/Editor/Scripts/Popup

// MIT License
//
// Copyright (c) 2021 Peter @sHTiF Stefcek
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
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

/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Pooling;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor
{
    public class MenuItemNode
    {
        public GUIContent content;
        public GenericMenu.MenuFunction func;
        public GenericMenu.MenuFunction2 func2;
        public object userData;
        public bool separator;
        public bool on;

        public MenuItemNode(string name = "", string tooltip = "", MenuItemNode parent = null)
        {
            Name = name;
            Parent = parent;
            Nodes = new FasterList<MenuItemNode>();
            content = new(name, tooltip);
        }

        public string Name { get; private set; }

        public MenuItemNode Parent { get; private set; }

        public FasterList<MenuItemNode> Nodes { get; private set; }

        public void Reset(string name = "", MenuItemNode parent = null)
        {
            content = null;
            func = null;
            func2 = null;
            userData = null;
            separator = false;
            on = false;
            Name = name;
            Parent = parent;
            Nodes.Clear();
        }

        public MenuItemNode CreateNode(string name, string tooltip = "")
        {
            var node = new MenuItemNode(name, tooltip, this);
            Nodes.Add(node);
            return node;
        }

        // TODO Optimize
        public MenuItemNode GetOrCreateNode(string name, string tooltip = "")
        {
            return Nodes.Find(n => n.Name == name) ?? CreateNode(name, tooltip);
        }

        public FasterList<MenuItemNode> Search(string search)
        {
            var result = FasterListPool<MenuItemNode>.Get();

            foreach (var node in Nodes)
            {
                if (node.Nodes.Count == 0 && node.ContainsInPath(search))
                {
                    result.Add(node);
                }

                var nodeResult = node.Search(search);
                result.AddRange(nodeResult);

                FasterListPool<MenuItemNode>.Release(nodeResult);
            }

            return result;
        }

        public bool ContainsInPath(string search)
        {
            if (UnityEditor.Search.FuzzySearch.FuzzyMatch(search, Name))
            {
                return true;
            }

            return Parent?.ContainsInPath(search) ?? false;
        }

        public string GetPath()
        {
            return BuildPath(new StringBuilder()).ToString();
        }

        public StringBuilder BuildPath(StringBuilder result)
        {
            Parent?.BuildPath(result).Append('/');
            result.Append(Name);
            return result;
        }

        public void Execute()
        {
            if (func != null)
            {
                func?.Invoke();
            }
            else
            {
                func2?.Invoke(userData);
            }
        }
    }

    public class GenericMenuPopup : PopupWindowContent
    {
        public static GenericMenuPopup Get(GenericMenu menu, string title)
        {
            var popup = new GenericMenuPopup(menu, title);
            return popup;
        }

        public static GenericMenuPopup Show(GenericMenu menu, string title, Vector2 position)
        {
            var popup = new GenericMenuPopup(menu, title);
            PopupWindow.Show(new Rect(position.x, position.y, 0, 0), popup);
            return popup;
        }

        private GUIStyle _backStyle;

        public GUIStyle BackStyle
        {
            get
            {
                return _backStyle ??= new GUIStyle(GUI.skin.button) {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 22,
                };
            }
        }

        private GUIStyle _plusStyle;

        public GUIStyle PlusStyle
        {
            get
            {
                return _plusStyle ??= new GUIStyle {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 16,
                    normal = { textColor = Color.white }
                };
            }
        }

        private GUIStyle _titleStyle;

        public GUIStyle TitleStyle
        {
            get
            {
                return _titleStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };
            }
        }

        private GUIStyle _tooltipStyle;

        public GUIStyle TooltipStyle
        {
            get
            {
                return _tooltipStyle ??= new GUIStyle(EditorStyles.label) {
                    fontSize = 9,
                    wordWrap = true,
                    richText = true,
                };
            }
        }

        private GUIStyle _whiteBoxStyle;

        public GUIStyle WhiteBoxStyle
        {
            get
            {
                return _whiteBoxStyle ??= new GUIStyle("box") {
                    normal = { background = Texture2D.whiteTexture }
                };
            }
        }

        private GUIStyle _labelStyle;

        public GUIStyle LabelStyle
        {
            get
            {
                return _labelStyle ??= new GUIStyle(EditorStyles.label) {
                    richText = true,
                    wordWrap = true,
                };
            }
        }

        private GUIStyle _boldLabelStyle;

        public GUIStyle BoldLabelStyle
        {
            get
            {
                _boldLabelStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                    richText = true,
                    wordWrap = true,
                };

                return _boldLabelStyle;
            }
        }

        private Vector2 _scrollPosition;
        private MenuItemNode _currentNode;
        private MenuItemNode _hoverNode;
        private string _search;
        private bool _repaint = false;
        private int _contentHeight;
        private bool _useScroll;

        public readonly string Title;
        public readonly MenuItemNode RootNode;

        public int width = 200;
        public int height = 200;
        public int maxHeight = 300;
        public bool resizeToContent = false;
        public bool showOnStatus = true;
        public bool showSearch = true;
        public bool showTooltip = false;
        public bool showTitle = false;

        public GenericMenuPopup(MenuItemNode rootNode, string title)
        {
            this.Title = title;
            showTitle = !string.IsNullOrWhiteSpace(this.Title);
            _currentNode = this.RootNode = rootNode;
        }

        public GenericMenuPopup(GenericMenu menu, string title)
        {
            this.Title = title;
            showTitle = !string.IsNullOrWhiteSpace(this.Title);
            _currentNode = RootNode = GenerateMenuItemNodeTree(menu);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(width, height);
        }

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type == EventType.Layout)
            {
                _useScroll = _contentHeight > maxHeight || (!resizeToContent && _contentHeight > height);
            }

            _contentHeight = 0;

            if (showTitle)
            {
                DrawTitle(new Rect(rect.x, rect.y, rect.width, 24));
            }

            if (showSearch)
            {
                DrawSearch(new Rect(rect.x + 5, rect.y + (showTitle ? 24 : 0), rect.width - 10, 30));
            }

            DrawMenuItems(new Rect(
                  rect.x + 5
                , rect.y + (showTitle ? 24 : 0) + (showSearch ? 30 : 0)
                , rect.width - 10
                , rect.height - (showTooltip ? 60 : 0) - (showTitle ? 24 : 0) - (showSearch ? 30 : 0)
            ));

            if (showTooltip)
            {
                DrawTooltip(new Rect(rect.x + 5, rect.y + rect.height - 58, rect.width - 10, 56));
            }

            if (resizeToContent)
            {
                height = Mathf.Min(_contentHeight, maxHeight);
            }

            EditorGUI.FocusTextInControl("Search");
        }

        private void DrawTitle(Rect rect)
        {
            _contentHeight += 24;

            GUI.Label(rect, Title, TitleStyle);
        }

        private void DrawSearch(Rect rect)
        {
            _contentHeight += 30;

            GUI.SetNextControlName("Search");
            _search = GUI.TextField(rect, _search, EditorStyles.toolbarSearchField);
        }

        private void DrawTooltip(Rect rect)
        {
            _contentHeight += 58;

            if (_hoverNode == null
                || _hoverNode.content == null
                || string.IsNullOrWhiteSpace(_hoverNode.content.tooltip)
            )
            {
                return;
            }

            GUI.Label(rect, _hoverNode.content.tooltip, TooltipStyle);
        }

        private void DrawMenuItems(Rect rect)
        {
            GUILayout.BeginArea(rect);

            if (_useScroll)
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
            }

            GUILayout.BeginVertical();

            if (string.IsNullOrWhiteSpace(_search) || _search.Length < 2)
            {
                DrawNodeTree(rect);
            }
            else
            {
                DrawNodeSearch(rect);
            }

            GUILayout.EndVertical();

            if (_useScroll)
            {
                GUILayout.EndScrollView();
                GUILayout.Space(10);
                _contentHeight += 10;
            }

            GUILayout.EndArea();
        }

        private void DrawNodeSearch(Rect _)
        {
            var search = RootNode.Search(_search);

            search.Sort(static (n1, n2) => {
                var sb = new StringBuilder();
                var p1 = n1.Parent.BuildPath(sb.Clear()).ToString();
                var p2 = n2.Parent.BuildPath(sb.Clear()).ToString();

                return string.Equals(p1, p2, StringComparison.Ordinal)
                    ? string.CompareOrdinal(n1.Name, n2.Name)
                    : string.CompareOrdinal(p1, p2);
            });

            var lastPath = "";
            var whiteBoxStyle = WhiteBoxStyle;
            var labelStyle = LabelStyle;
            var boldLabelStyle = BoldLabelStyle;
            var sb = new StringBuilder();

            foreach (var node in search)
            {
                var nodePath = node.Parent.BuildPath(sb.Clear()).ToString();

                if (string.Equals(nodePath, lastPath, StringComparison.Ordinal) == false)
                {
                    _contentHeight += 20;
                    GUILayout.Label(nodePath, boldLabelStyle, GUILayout.Height(20));
                    lastPath = nodePath;

                    GUILayout.Space(4);
                    _contentHeight += 4;
                }

                var origColor = GUI.color;
                GUI.color = _hoverNode == node ? Color.cyan : origColor;

                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Height(20));
                {
                    if (showOnStatus)
                    {
                        GUI.color = node.on ? new Color(0, .6f, .8f) : new Color(.2f, .2f, .2f);
                        GUILayout.Box("", whiteBoxStyle, GUILayout.Width(14), GUILayout.Height(14));
                    }

                    GUI.color = _hoverNode == node ? Color.cyan : origColor;
                    GUILayout.Label(node.Name, labelStyle, GUILayout.Height(20));
                }
                GUILayout.EndHorizontal();
                GUI.color = origColor;

                var nodeRect = GUILayoutUtility.GetLastRect();
                _contentHeight += Mathf.CeilToInt(nodeRect.height + 4f);

                if (Event.current.isMouse)
                {
                    if (nodeRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            if (node.Nodes.Count > 0)
                            {
                                _currentNode = node;
                                _repaint = true;
                            }
                            else if (node.func != null || node.func2 != null)
                            {
                                node.Execute();
                                base.editorWindow.Close();
                            }

                            break;
                        }

                        if (_hoverNode != node)
                        {
                            _hoverNode = node;
                            _repaint = true;
                        }
                    }
                    else if (_hoverNode == node)
                    {
                        _hoverNode = null;
                        _repaint = true;
                    }
                }
            }

            if (search.Count == 0)
            {
                GUILayout.Label("No result found for specified search.");
            }

            FasterListPool<MenuItemNode>.Release(search);
        }

        private void DrawNodeTree(Rect _)
        {
            if (_currentNode != RootNode)
            {
                _contentHeight += 20;

                var path = _currentNode.BuildPath(new StringBuilder()).ToString();

                if (GUILayout.Button(path, BackStyle, GUILayout.Height(20)))
                {
                    _currentNode = _currentNode.Parent;
                }

                GUILayout.Space(4);
                _contentHeight += 4;
            }

            var whiteBoxStyle = WhiteBoxStyle;
            var labelStyle = LabelStyle;
            var boldLabelStyle = BoldLabelStyle;

            foreach (var node in _currentNode.Nodes)
            {
                if (node.separator)
                {
                    GUILayout.Space(4);
                    _contentHeight += 4;
                    continue;
                }

                var origColor = GUI.color;
                GUI.color = _hoverNode == node ? Color.cyan : origColor;

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    if (showOnStatus)
                    {
                        GUI.color = node.on ? new Color(0, .6f, .8f, .5f) : new Color(.2f, .2f, .2f, .2f);
                        GUILayout.Box("", whiteBoxStyle, GUILayout.Width(14), GUILayout.Height(14));
                    }

                    GUI.color = _hoverNode == node ? Color.cyan : origColor;

                    var nodeLabelStyle = node.Nodes.Count > 0 ? boldLabelStyle : labelStyle;
                    GUILayout.Label(node.Name, nodeLabelStyle);

                }
                GUILayout.EndHorizontal();
                GUI.color = origColor;

                var nodeRect = GUILayoutUtility.GetLastRect();
                _contentHeight += Mathf.CeilToInt(nodeRect.height + 4f);

                if (Event.current.isMouse)
                {
                    if (nodeRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            if (node.Nodes.Count > 0)
                            {
                                _currentNode = node;
                                _repaint = true;
                            }
                            else if (node.func != null || node.func2 != null)
                            {
                                node.Execute();
                                base.editorWindow.Close();
                            }

                            break;
                        }

                        if (_hoverNode != node)
                        {
                            _hoverNode = node;
                            _repaint = true;
                        }
                    }
                    else if (_hoverNode == node)
                    {
                        _hoverNode = null;
                        _repaint = true;
                    }
                }

                if (node.Nodes.Count > 0)
                {
                    GUI.Label(new Rect(nodeRect.x + nodeRect.width - 20, nodeRect.y - 1, 20, 20), "+", PlusStyle);
                }
            }
        }

        // TODO Possible type caching?
        public static MenuItemNode GenerateMenuItemNodeTree(GenericMenu menu)
        {
            var rootNode = new MenuItemNode();

            if (menu == null)
                return rootNode;

            var menuItems = TryGetMenuItems(menu, "menuItems") ?? TryGetMenuItems(menu, "m_MenuItems");

            foreach (var menuItem in menuItems)
            {
                var menuItemType = menuItem.GetType();
                var content = (GUIContent)menuItemType.GetField("content").GetValue(menuItem);

                var separator = (bool)menuItemType.GetField("separator").GetValue(menuItem);
                var path = content.text;
                var splitPath = path.Split('/');
                MenuItemNode currentNode = rootNode;

                for (var i = 0; i < splitPath.Length; i++)
                {
                    currentNode = (i < splitPath.Length - 1)
                        ? currentNode.GetOrCreateNode(splitPath[i])
                        : currentNode.CreateNode(splitPath[i]);
                }

                if (separator)
                {
                    currentNode.separator = true;
                }
                else
                {
                    currentNode.content = content;
                    currentNode.func = (GenericMenu.MenuFunction)menuItemType.GetField("func").GetValue(menuItem);
                    currentNode.func2 = (GenericMenu.MenuFunction2)menuItemType.GetField("func2").GetValue(menuItem);
                    currentNode.userData = menuItemType.GetField("userData").GetValue(menuItem);
                    currentNode.on = (bool)menuItemType.GetField("on").GetValue(menuItem);
                }
            }

            return rootNode;

            static IEnumerable TryGetMenuItems(GenericMenu menu, string fieldName)
            {
                var menuItemsField = menu.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                return menuItemsField?.GetValue(menu) as IEnumerable;
            }
        }

        public void Show(float x, float y)
            => Show(new Vector2(x, y));

        public void Show(Vector2 position)
        {
            PopupWindow.Show(new Rect(position.x, position.y, 0, 0), this);
        }

        void OnEditorUpdate()
        {
            if (_repaint)
            {
                _repaint = false;
                base.editorWindow.Repaint();
            }
        }

        public override void OnOpen()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        public override void OnClose()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
    }
}

#endif
