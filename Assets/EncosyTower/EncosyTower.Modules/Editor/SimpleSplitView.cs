#if UNITY_EDITOR

// https://github.com/miguel12345/EditorGUISplitView/blob/master/Assets/EditorGUISplitView/Scripts/Editor/EditorGUISplitView.cs

using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor
{
    public class SimpleSplitView
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }

        private readonly Direction _splitDirection;
        private readonly bool _enabledScrollView;

        private float _splitNormalizedPosition;
        private bool _resize;
        private Vector2 _scrollPosition;
        private Rect _availableRect;
        private bool _isSplit;

        public SimpleSplitView(Direction splitDirection, float splitNormalizedPosition = 0.5f, bool enabledScrollView = true)
        {
            _splitNormalizedPosition = splitNormalizedPosition;
            _splitDirection = splitDirection;
            _enabledScrollView = enabledScrollView;
        }

        public void BeginSplitView()
        {
            var tempRect = _splitDirection == Direction.Horizontal
                ? EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true))
                : EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            if (tempRect.width > 0.0f)
            {
                _availableRect = tempRect;
            }

            if (_enabledScrollView)
            {
                _scrollPosition = _splitDirection == Direction.Horizontal
                    ? GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(_availableRect.width * _splitNormalizedPosition))
                    : GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(_availableRect.height * _splitNormalizedPosition));
            }
        }

        public void Split()
        {
            _isSplit = true;

            if (_enabledScrollView)
            {
                GUILayout.EndScrollView();
            }

            ResizeSplitFirstView();
        }

        public void EndSplitView()
        {
            if (_isSplit == false && _enabledScrollView)
            {
                GUILayout.EndScrollView();
            }

            if (_splitDirection == Direction.Horizontal)
            {
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.EndVertical();
            }
        }

        private void ResizeSplitFirstView()
        {
            var resizeHandleRect = _splitDirection == Direction.Horizontal
                ? new Rect(_availableRect.width * _splitNormalizedPosition, _availableRect.y, 2f, _availableRect.height)
                : new Rect(_availableRect.x, _availableRect.height * _splitNormalizedPosition, _availableRect.width, 2f);

            GUI.DrawTexture(resizeHandleRect, EditorGUIUtility.whiteTexture);

            EditorGUIUtility.AddCursorRect(resizeHandleRect,
                _splitDirection == Direction.Horizontal
                    ? MouseCursor.ResizeHorizontal
                    : MouseCursor.ResizeVertical
            );

            if (Event.current.type == EventType.MouseDown && resizeHandleRect.Contains(Event.current.mousePosition))
            {
                _resize = true;
            }

            if (_resize)
            {
                _splitNormalizedPosition = _splitDirection == Direction.Horizontal
                    ? Event.current.mousePosition.x / _availableRect.width
                    : Event.current.mousePosition.y / _availableRect.height;
            }

            if (Event.current.type == EventType.MouseUp)
            {
                _resize = false;
            }
        }
    }
}

#endif
