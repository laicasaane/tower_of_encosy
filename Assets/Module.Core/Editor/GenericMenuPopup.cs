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
using Module.Core.Collections;
using Module.Core.Pooling;
using UnityEditor;
using UnityEngine;

namespace Module.Core.Editor
{
    public class MenuItemNode
    {
        public GUIContent content;
        public GenericMenu.MenuFunction func;
        public GenericMenu.MenuFunction2 func2;
        public object userData;
        public bool separator;
        public bool on;

        public MenuItemNode(string p_name = "", MenuItemNode p_parent = null)
        {
            Name = p_name;
            Parent = p_parent;
            Nodes = new FasterList<MenuItemNode>();
        }

        public string Name { get; private set; }

        public MenuItemNode Parent { get; private set; }

        public FasterList<MenuItemNode> Nodes { get; private set; }

        public void Reset(string p_name = "", MenuItemNode p_parent = null)
        {
            content = null;
            func = null;
            func2 = null;
            userData = null;
            separator = false;
            on = false;
            Name = p_name;
            Parent = p_parent;
            Nodes.Clear();
        }

        public MenuItemNode CreateNode(string p_name)
        {
            var node = new MenuItemNode(p_name, this);
            Nodes.Add(node);
            return node;
        }

        // TODO Optimize
        public MenuItemNode GetOrCreateNode(string p_name)
        {
            return Nodes.Find(n => n.Name == p_name) ?? CreateNode(p_name);
        }

        public FasterList<MenuItemNode> Search(string p_search)
        {
            var result = FasterListPool<MenuItemNode>.Get();

            foreach (var node in Nodes)
            {
                if (node.Nodes.Count == 0 && node.ContainsInPath(p_search))
                {
                    result.Add(node);
                }

                var nodeResult = node.Search(p_search);
                result.AddRange(nodeResult);

                FasterListPool<MenuItemNode>.Release(nodeResult);
            }

            return result;
        }

        public bool ContainsInPath(string search)
        {
            if (Name.Contains(search, StringComparison.OrdinalIgnoreCase))
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
        public static GenericMenuPopup Get(GenericMenu p_menu, string p_title)
        {
            var popup = new GenericMenuPopup(p_menu, p_title);
            return popup;
        }

        public static GenericMenuPopup Show(GenericMenu p_menu, string p_title, Vector2 p_position)
        {
            var popup = new GenericMenuPopup(p_menu, p_title);
            PopupWindow.Show(new Rect(p_position.x, p_position.y, 0, 0), popup);
            return popup;
        }

        private GUIStyle _backStyle;

        public GUIStyle BackStyle
        {
            get
            {
                _backStyle ??= new GUIStyle(GUI.skin.button) {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 22,
                };

                return _backStyle;
            }
        }

        private GUIStyle _plusStyle;

        public GUIStyle PlusStyle
        {
            get
            {
                if (_plusStyle == null)
                {
                    _plusStyle = new GUIStyle {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 16,
                    };

                    _plusStyle.normal.textColor = Color.white;
                }

                return _plusStyle;
            }
        }

        private GUIStyle _titleStyle;

        public GUIStyle TitleStyle
        {
            get
            {
                _titleStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };

                return _titleStyle;
            }
        }

        private GUIStyle _tooltipStyle;

        public GUIStyle TooltipStyle
        {
            get
            {
                _tooltipStyle ??= new GUIStyle(EditorStyles.label) {
                    fontSize = 9,
                    wordWrap = true,
                };

                return _tooltipStyle;
            }
        }

        private GUIStyle _whiteBoxStyle;

        public GUIStyle WhiteBoxStyle
        {
            get
            {
                if (_whiteBoxStyle == null)
                {
                    _whiteBoxStyle = new GUIStyle("box");
                    _whiteBoxStyle.normal.background = Texture2D.whiteTexture;
                }

                return _whiteBoxStyle;
            }
        }

        private Vector2 _scrollPosition;
        private MenuItemNode _currentNode;
        private MenuItemNode _hoverNode;
        private string _search;
        private bool _repaint = false;
        private int _contentHeight;
        private bool _useScroll;

        public string title;
        public MenuItemNode rootNode;
        public int width = 200;
        public int height = 200;
        public int maxHeight = 300;
        public bool resizeToContent = false;
        public bool showOnStatus = true;
        public bool showSearch = true;
        public bool showTooltip = false;
        public bool showTitle = false;

        public GenericMenuPopup(MenuItemNode p_rootNode, string p_title)
        {
            title = p_title;
            showTitle = !string.IsNullOrWhiteSpace(title);
            _currentNode = rootNode = p_rootNode;
        }

        public GenericMenuPopup(GenericMenu p_menu, string p_title)
        {
            title = p_title;
            showTitle = !string.IsNullOrWhiteSpace(title);
            _currentNode = rootNode = GenerateMenuItemNodeTree(p_menu);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(width, height);
        }

        public override void OnGUI(Rect p_rect)
        {
            if (Event.current.type == EventType.Layout)
            {
                _useScroll = _contentHeight > maxHeight || (!resizeToContent && _contentHeight > height);
            }

            _contentHeight = 0;

            if (showTitle)
            {
                DrawTitle(new Rect(p_rect.x, p_rect.y, p_rect.width, 24));
            }

            if (showSearch)
            {
                DrawSearch(new Rect(p_rect.x + 5, p_rect.y + (showTitle ? 24 : 0), p_rect.width - 10, 30));
            }

            DrawMenuItems(new Rect(
                  p_rect.x + 5
                , p_rect.y + (showTitle ? 24 : 0) + (showSearch ? 22 : 0)
                , p_rect.width - 10
                , p_rect.height - (showTooltip ? 60 : 0) - (showTitle ? 24 : 0) - (showSearch ? 22 : 0)
            ));

            if (showTooltip)
            {
                DrawTooltip(new Rect(p_rect.x + 5, p_rect.y + p_rect.height - 58, p_rect.width - 10, 56));
            }

            if (resizeToContent)
            {
                height = Mathf.Min(_contentHeight, maxHeight);
            }

            EditorGUI.FocusTextInControl("Search");
        }

        private void DrawTitle(Rect p_rect)
        {
            _contentHeight += 24;

            GUI.Label(p_rect, title, TitleStyle);
        }

        private void DrawSearch(Rect p_rect)
        {
            _contentHeight += 30;

            GUI.SetNextControlName("Search");
            _search = GUI.TextField(p_rect, _search, EditorStyles.toolbarSearchField);
        }

        private void DrawTooltip(Rect p_rect)
        {
            _contentHeight += 60;

            if (_hoverNode == null
                || _hoverNode.content == null
                || string.IsNullOrWhiteSpace(_hoverNode.content.tooltip)
            )
            {
                return;
            }

            GUI.Label(p_rect, _hoverNode.content.tooltip, TooltipStyle);
        }

        private void DrawMenuItems(Rect p_rect)
        {
            GUILayout.BeginArea(p_rect);

            if (_useScroll)
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
            }

            GUILayout.BeginVertical();

            if (string.IsNullOrWhiteSpace(_search) || _search.Length < 2)
            {
                DrawNodeTree(p_rect);
            }
            else
            {
                DrawNodeSearch(p_rect);
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
            var search = rootNode.Search(_search);

            search.Sort(static (n1, n2) => {
                var sb = new StringBuilder();
                var p1 = n1.Parent.BuildPath(sb.Clear()).ToString();
                var p2 = n2.Parent.BuildPath(sb.Clear()).ToString();

                if (string.Equals(p1, p2, StringComparison.Ordinal))
                {
                    string.CompareOrdinal(n1.Name, n2.Name);
                }

                return string.CompareOrdinal(p1, p2);
            });

            var lastPath = "";
            var whiteBoxStyle = WhiteBoxStyle;
            var sb = new StringBuilder();

            foreach (var node in search)
            {
                var nodePath = node.Parent.BuildPath(sb.Clear()).ToString();

                if (string.Equals(nodePath, lastPath, StringComparison.Ordinal) == false)
                {
                    _contentHeight += 20;
                    GUILayout.Label(nodePath, GUILayout.Height(20));
                    lastPath = nodePath;

                    GUILayout.Space(4);
                    _contentHeight += 4;
                }

                _contentHeight += 20;

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
                    GUILayout.Label(node.Name, GUILayout.Height(20));
                }
                GUILayout.EndHorizontal();
                GUI.color = origColor;

                var nodeRect = GUILayoutUtility.GetLastRect();

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
                            else
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
            if (_currentNode != rootNode)
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

            foreach (var node in _currentNode.Nodes)
            {
                if (node.separator)
                {
                    GUILayout.Space(4);
                    _contentHeight += 4;
                    continue;
                }

                _contentHeight += 20;

                var origColor = GUI.color;
                GUI.color = _hoverNode == node ? Color.cyan : origColor;

                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Height(20));
                {
                    if (showOnStatus)
                    {
                        GUI.color = node.on ? new Color(0, .6f, .8f, .5f) : new Color(.2f, .2f, .2f, .2f);
                        GUILayout.Box("", whiteBoxStyle, GUILayout.Width(14), GUILayout.Height(14));
                    }

                    GUI.color = _hoverNode == node ? Color.cyan : origColor;

                    var labelStyle = node.Nodes.Count > 0 ? EditorStyles.boldLabel : EditorStyles.label;
                    GUILayout.Label(node.Name, labelStyle, GUILayout.Height(20));

                }
                GUILayout.EndHorizontal();
                GUI.color = origColor;

                var nodeRect = GUILayoutUtility.GetLastRect();

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
                            else
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
                    var lastRect = GUILayoutUtility.GetLastRect();
                    GUI.Label(new Rect(lastRect.x + lastRect.width - 20, lastRect.y + 1, 20, 20), "+", PlusStyle);
                }
            }
        }

        // TODO Possible type caching?
        public static MenuItemNode GenerateMenuItemNodeTree(GenericMenu p_menu)
        {
            var rootNode = new MenuItemNode();

            if (p_menu == null)
                return rootNode;

            var menuItems = TryGetMenuItems(p_menu, "menuItems") ?? TryGetMenuItems(p_menu, "m_MenuItems");

            foreach (var menuItem in menuItems)
            {
                var menuItemType = menuItem.GetType();
                var content = (GUIContent)menuItemType.GetField("content").GetValue(menuItem);

                bool separator = (bool)menuItemType.GetField("separator").GetValue(menuItem);
                string path = content.text;
                string[] splitPath = path.Split('/');
                MenuItemNode currentNode = rootNode;

                for (int i = 0; i < splitPath.Length; i++)
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

            static IEnumerable TryGetMenuItems(GenericMenu p_menu, string fieldName)
            {
                var menuItemsField = p_menu.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                return menuItemsField?.GetValue(p_menu) as IEnumerable;
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
