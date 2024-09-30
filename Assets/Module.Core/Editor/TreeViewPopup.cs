#if UNITY_EDITOR

using System;
using Module.Core.Logging;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Module.Core.Editor
{
    public class TreeViewPopup : PopupWindowContent
    {
        [SerializeField] private TreeViewState _treeViewState = new();

        public float width = 200f;
        public float height = 400f;

        private string _title;
        private bool _showTitle;
        private string _search;
        private GUIStyle _titleStyle;
        private GUIContent _applyLabel;
        private GUIContent _resetLabel;

        public TreeViewPopup(string title)
        {
            Title = title;
        }

        public GUIStyle TitleStyle => _titleStyle ??= new GUIStyle(EditorStyles.boldLabel) {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };

        public GUIContent ApplyLabel => _applyLabel ??= new GUIContent("Apply", "Apply Selected Items");

        public GUIContent ResetLabel => _resetLabel ??= new GUIContent("Reset", "Reset Selection");

        public TreeViewState TreeViewState => _treeViewState;

        public TreeView Tree { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                _showTitle = !string.IsNullOrWhiteSpace(value);
            }
        }

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

            rect.y += 30;
            rect.height -= 30;

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

            GUI.Label(rect, Title, TitleStyle);
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
