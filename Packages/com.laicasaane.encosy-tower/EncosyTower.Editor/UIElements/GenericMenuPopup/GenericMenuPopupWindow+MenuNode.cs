#if UNITY_EDITOR

using System;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    partial class GenericMenuPopupWindow
    {
        private class MenuNode : VisualElement
        {
            public static readonly string MenuNodeUssClassName = "menu-node";
            public static readonly string NodeTitleUssClassName = "node-title";
            public static readonly string PlusSignUssClassName = "plus-sign";

            public MenuItemNode node;

            public event Action OnClick;
            public event Action OnHover;
            public event Action OnExit;

            private readonly Label _nodeTitle;
            private readonly Label _plusSign;

            public MenuNode(MenuItemNode node, bool isRoot = false, bool showPlus = false)
            {
                this.node = node;

                AddToClassList(MenuNodeUssClassName);

                if (showPlus)
                {
                    AddToClassList($"{MenuNodeUssClassName}-path");
                }

                if (isRoot)
                {
                    AddToClassList($"{MenuNodeUssClassName}-root");
                }

                _nodeTitle = new(isRoot ? $"/{node.Name}" : node.Name);
                _nodeTitle.AddToClassList(NodeTitleUssClassName);

                if (showPlus)
                {
                    _nodeTitle.AddToClassList($"{NodeTitleUssClassName}-path");
                }

                if (isRoot)
                {
                    _nodeTitle.AddToClassList($"{NodeTitleUssClassName}-root");
                }

                Add(_nodeTitle);

                if (showPlus)
                {
                    _plusSign = new("+");
                    _plusSign.AddToClassList(PlusSignUssClassName);

                    Add(_plusSign);
                }

                RegisterCallback<PointerDownEvent>(MenuNode_OnClick);
                RegisterCallback<PointerEnterEvent>(MenuNode_OnHover);
                RegisterCallback<PointerLeaveEvent>(MenuNode_OnExit);
            }

            private void MenuNode_OnClick(PointerDownEvent evt)
            {
                OnClick?.Invoke();
                evt.StopPropagation();
            }

            private void MenuNode_OnHover(PointerEnterEvent evt)
            {
                OnHover?.Invoke();
                evt.StopPropagation();
            }

            private void MenuNode_OnExit(PointerLeaveEvent evt)
            {
                OnExit?.Invoke();
                evt.StopPropagation();
            }
        }
    }
}

#endif
