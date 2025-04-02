// MIT License
//
// Copyright (c) 2023 Johan Steen
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

namespace EncosyTower.UIElements
{
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// SafeArea Container for UI Toolkit.
    /// </summary>
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class SafeArea : VisualElement
    {
        public static readonly string UssClassName = "safe-area";
        public static readonly string ContentUssClassName = $"{UssClassName}__content-content";

        private readonly VisualElement _contentContainer;

        public SafeArea()
        {
            AddToClassList(UssClassName);

            // By using absolute position instead of flex to fill the full screen, SafeArea containers can be stacked.
            style.position = Position.Absolute;
            style.top = 0;
            style.bottom = 0;
            style.left = 0;
            style.right = 0;
            pickingMode = PickingMode.Ignore;

            var container = _contentContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            container.AddToClassList(ContentUssClassName);
            container.style.flexGrow = 1;
            container.style.flexShrink = 0;

            hierarchy.Add(container);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public override VisualElement contentContainer
        {
            get => _contentContainer;
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute("collapse-margins")]
#endif
        public bool CollapseMargins { get; set; }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute("exclude-left")]
#endif
        public bool ExcludeLeft { get; set; }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute("exclude-right")]
#endif
        public bool ExcludeRight { get; set; }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute("exclude-top")]
#endif
        public bool ExcludeTop { get; set; }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute("exclude-bottom")]
#endif
        public bool ExcludeBottom { get; set; }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute("exclude-tvos")]
#endif
        public bool ExcludeTvOs { get; set; }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // As RuntimePanelUtils is not available in UIBuilder,
            // the handling is wrapped in a try/catch to avoid InvalidCastExceptions when working in UIBuilder.
            try
            {
                var safeArea = GetSafeAreaOffset();
                var margin = GetMarginOffset();

                if (CollapseMargins)
                {
                    _contentContainer.style.marginLeft = Mathf.Max(margin.Left, safeArea.Left) - margin.Left;
                    _contentContainer.style.marginRight = Mathf.Max(margin.Right, safeArea.Right) - margin.Right;
                    _contentContainer.style.marginTop = Mathf.Max(margin.Top, safeArea.Top) - margin.Top;
                    _contentContainer.style.marginBottom = Mathf.Max(margin.Bottom, safeArea.Bottom) - margin.Bottom;
                }
                else
                {
                    _contentContainer.style.marginLeft = safeArea.Left;
                    _contentContainer.style.marginRight = safeArea.Right;
                    _contentContainer.style.marginTop = safeArea.Top;
                    _contentContainer.style.marginBottom = safeArea.Bottom;
                }
            }
            catch (System.InvalidCastException)
            {
            }
        }

        // Convert screen safe area to panel space and get the offset values from the panel edges.
        private Offset GetSafeAreaOffset()
        {
            var safeArea = Screen.safeArea;
            var leftTop = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(safeArea.xMin, Screen.height - safeArea.yMax));
            var rightBottom = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(Screen.width - safeArea.xMax, safeArea.yMin));

#if UNITY_TVOS
            if (ExcludeTvOs)
            {
                return default;
            }
#endif

            // If the user has flagged an edge as excluded, set that edge to 0.
            return new Offset {
                Left = ExcludeLeft ? 0 : leftTop.x,
                Right = ExcludeRight ? 0 : rightBottom.x,
                Top = ExcludeTop ? 0 : leftTop.y,
                Bottom = ExcludeBottom ? 0 : rightBottom.y
            };
        }

        // Get the resolved margins from the inline style.
        private Offset GetMarginOffset()
        {
            return new Offset {
                Left = resolvedStyle.marginLeft,
                Right = resolvedStyle.marginRight,
                Top = resolvedStyle.marginTop,
                Bottom = resolvedStyle.marginBottom,
            };
        }

        private readonly record struct Offset(float Left, float Right, float Top, float Bottom);
    }
}

#if !UNITY_6000_0_OR_NEWER

namespace EncosyTower.UIElements
{
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// SafeArea Container for UI Toolkit.
    /// </summary>
    partial class SafeArea
    {
        public new class UxmlFactory : UxmlFactory<SafeArea, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlBoolAttributeDescription _collapseMarginsAttr = new() {
                name = "collapse-margins",
                defaultValue = true
            };

            private readonly UxmlBoolAttributeDescription _excludeLeftAttr = new() {
                name = "exclude-left",
                defaultValue = false
            };

            private readonly UxmlBoolAttributeDescription _excludeRightAttr = new() {
                name = "exclude-right",
                defaultValue = false
            };

            private readonly UxmlBoolAttributeDescription _excludeTopAttr = new() {
                name = "exclude-top",
                defaultValue = false
            };

            private readonly UxmlBoolAttributeDescription _excludeBottomAttr = new() {
                name = "exclude-bottom",
                defaultValue = false
            };

            private readonly UxmlBoolAttributeDescription _excludeTvOsAttr = new() {
                name = "exclude-tvos",
                defaultValue = false
            };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var ate = ve as SafeArea;
                ate.CollapseMargins = _collapseMarginsAttr.GetValueFromBag(bag, cc);
                ate.ExcludeLeft = _excludeLeftAttr.GetValueFromBag(bag, cc);
                ate.ExcludeRight = _excludeRightAttr.GetValueFromBag(bag, cc);
                ate.ExcludeTop = _excludeTopAttr.GetValueFromBag(bag, cc);
                ate.ExcludeBottom = _excludeBottomAttr.GetValueFromBag(bag, cc);
                ate.ExcludeTvOs = _excludeTvOsAttr.GetValueFromBag(bag, cc);
            }
        }
    }
}

#endif

