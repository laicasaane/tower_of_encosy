#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    public class SimpleSplitView : VisualElement
    {
        public enum Direction
        {
            Horizontal,
            Vertical,
        }

        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/UIElements/SimpleSplitView";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(SimpleSplitView);

        private const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
        private const string STYLE_SHEET_DARK = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Dark.uss";
        private const string STYLE_SHEET_LIGHT = $"{STYLE_SHEETS_PATH}/{FILE_NAME}_Light.uss";

        public static readonly string FirstPaneUssClassName = "first-pane";
        public static readonly string SecondPaneUssClassName = "second-pane";
        public static readonly string DragLineUssClassName = "drag-line";

        public readonly VisualElement FirstPane;
        public readonly VisualElement SecondPane;
        public readonly VisualElement DragLine;

        private readonly Direction _direction;
        private readonly float _splitNormalizedPosition;

        private float _initialPaneSize;
        private float _firstPaneLeft;
        private bool _isPointerCaptured;

        public SimpleSplitView(Direction splitDirection, float splitNormalizedPosition = 0.5f)
        {
            this.WithEditorStyleSheet(THEME_STYLE_SHEET);
            this.WithEditorStyleSheet(STYLE_SHEET_DARK, STYLE_SHEET_LIGHT);

            _direction = splitDirection;
            _splitNormalizedPosition = splitNormalizedPosition;

            FirstPane = new VisualElement { name = "FirstPane" };
            FirstPane.AddToClassList(FirstPaneUssClassName);

            SecondPane = new VisualElement { name = "SecondPane" };
            SecondPane.AddToClassList(SecondPaneUssClassName);

            DragLine = new VisualElement { name = "DragLine" };

            Add(FirstPane);
            Add(DragLine);
            Add(SecondPane);

            style.flexDirection = _direction == Direction.Horizontal ? FlexDirection.Row : FlexDirection.Column;

            DragLine.AddToClassList(DragLineUssClassName);
            DragLine.AddToClassList(_direction.ToString().ToLower());

            DragLine.RegisterCallback<PointerDownEvent>(OnMouseDown);
            DragLine.RegisterCallback<PointerMoveEvent>(OnMouseMove);
            DragLine.RegisterCallback<PointerUpEvent>(OnMouseUp);

            UpdatePaneSizes();
        }

        private void UpdatePaneSizes()
        {
            if (_direction == Direction.Horizontal)
            {
                FirstPane.style.width = new Length(_splitNormalizedPosition * 100, LengthUnit.Percent);
                SecondPane.style.width = new Length((1 - _splitNormalizedPosition) * 100, LengthUnit.Percent);
            }
        }

        private void OnMouseDown(PointerDownEvent evt)
        {
            var mousePos = evt.originalMousePosition;
            var resolvedStyle = FirstPane.resolvedStyle;

            _initialPaneSize = _direction == Direction.Vertical ? mousePos.y : mousePos.x;
            _firstPaneLeft = _direction == Direction.Vertical ? resolvedStyle.height : resolvedStyle.width;

            DragLine.CapturePointer(evt.pointerId);

            _isPointerCaptured = true;
        }

        private void OnMouseMove(PointerMoveEvent evt)
        {
            if (_isPointerCaptured == false)
            {
                return;
            }

            var mousePos = evt.originalMousePosition;
            var currentPointerPosition = _direction == Direction.Vertical ? mousePos.y : mousePos.x;
            var delta = currentPointerPosition - _initialPaneSize;
            var finalHeight = Mathf.Max(0, _firstPaneLeft + delta);

            var style = FirstPane.style;

            if (_direction == Direction.Vertical)
            {
                style.height = new StyleLength(finalHeight);
            }
            else
            {
                style.minWidth = new StyleLength(finalHeight);
                style.maxWidth = new StyleLength(finalHeight);
            }
        }

        private void OnMouseUp(PointerUpEvent evt)
        {
            if (_isPointerCaptured == false)
            {
                return;
            }

            DragLine.ReleasePointer(evt.pointerId);

            _isPointerCaptured = false;
        }
    }
}

#endif
