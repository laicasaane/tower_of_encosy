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

using System.Text;
using EncosyTower.Collections;
using EncosyTower.Pooling;
using EncosyTower.Search;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor
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
            return Nodes.Find(n => n.Name == name).TryGetValue(out var node)
                ? node
                : CreateNode(name, tooltip);
        }

        public void Search(string search, FasterList<MenuItemNode> result)
        {
            foreach (var node in Nodes)
            {
                if (node.Nodes.Count == 0 && node.ContainsInPath(search))
                {
                    result.Add(node);
                }

                using (FasterListPool<MenuItemNode>.Get(out var searchResult))
                {
                    node.Search(search, searchResult);
                    result.AddRange(searchResult.AsReadOnlySpan());
                }
            }
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

        private readonly struct SearchValidator : ISearchValidator<MenuItemNode>
        {
            public bool IsSearchable(MenuItemNode item)
                => true;

            public string GetSearchableString(MenuItemNode item)
                => item.Name;
        }
    }
}

#endif
