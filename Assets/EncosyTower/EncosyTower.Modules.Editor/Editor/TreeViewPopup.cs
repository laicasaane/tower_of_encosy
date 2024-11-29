#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Modules.Logging;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace EncosyTower.Modules.Editor
{
    public class TreeViewPopup : PopupWindowContent
    {
        [SerializeField] private TreeViewState _treeViewState = new();

        public float width = 200f;
        public float height = 400f;
        public object data;
        public Action<object, IList<int>> onApplySelectedIds;

        private readonly GUIContent _title;
        private readonly bool _showTitle;
        private string _search;
        private GUIStyle _titleStyle;
        private GUIContent _applyLabel;
        private GUIContent _resetLabel;

        public TreeViewPopup(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _title = GUIContent.none;
                _showTitle = false;
            }
            else
            {
                _title = new(title);
                _showTitle = true;
            }
        }

        public TreeViewPopup(GUIContent title)
        {
            _title = title;
            _showTitle = title != null;
        }

        public GUIStyle TitleStyle => _titleStyle ??= new GUIStyle(EditorStyles.boldLabel) {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };

        public GUIContent ApplyLabel => _applyLabel ??= new GUIContent("Apply", "Apply Selected Items");

        public GUIContent ResetLabel => _resetLabel ??= new GUIContent("Reset", "Reset Selection");

        public TreeViewState TreeViewState => _treeViewState;

        public TreeView Tree { get; set; }

        public void Show(float x, float y)
            => Show(new Vector2(x, y));

        public void Show(Vector2 position)
        {
            if (Tree == null)
            {
                DevLoggerAPI.LogError("Cannot show an empty tree!");
                return;
            }

            PopupWindow.Show(new Rect(position.x, position.y, 0, 0), this);
        }

        public override Vector2 GetWindowSize()
            => new(Mathf.Max(width, 200f), Mathf.Max(height, 400));

        public override void OnGUI(Rect rect)
        {
            if (Tree == null)
            {
                return;
            }

            DrawTitle(rect);

            rect.y += 24;
            rect.height -= 24;

            DrawToolbar(rect);

            rect.y += 20;
            rect.height -= 20;

            DrawSearch(rect);

            rect.y += 40;
            rect.height -= 40;

            Tree.OnGUI(rect);

            EditorGUI.FocusTextInControl("Search");
        }

        private void DrawTitle(Rect rect)
        {
            if (_showTitle == false)
            {
                return;
            }

            rect = new Rect(rect.x, rect.y, rect.width, 24);

            GUI.Label(rect, _title, TitleStyle);
        }

        private void DrawToolbar(Rect rect)
        {
            rect = new Rect(rect.x, rect.y, rect.width, 20);

            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            {
                var guiEnabled = GUI.enabled;
                GUI.enabled = Tree.HasSelection();

                if (GUILayout.Button(ApplyLabel, EditorStyles.miniButtonMid))
                {
                    onApplySelectedIds?.Invoke(data, Tree.GetSelection());
                    base.editorWindow.Close();
                }

                GUI.enabled = guiEnabled;

                if (GUILayout.Button(ResetLabel, EditorStyles.miniButtonMid))
                {
                    Tree.state.selectedIDs.Clear();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawSearch(Rect rect)
        {
            rect = new Rect(rect.x + 5, rect.y, rect.width - 10, 30);
            rect.y += (EditorStyles.toolbarSearchField.fixedHeight) / 2f;

            GUI.SetNextControlName("Search");

            var search = GUI.TextField(rect, _search, EditorStyles.toolbarSearchField);

            if (string.Equals(search, _search, StringComparison.Ordinal) == false)
            {
                Tree.searchString = _search = search;
            }
        }
    }
}

#endif
