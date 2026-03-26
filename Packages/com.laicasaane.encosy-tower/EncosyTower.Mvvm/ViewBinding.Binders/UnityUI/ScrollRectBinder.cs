#if UNITY_UGUI

#pragma warning disable CS0657, IDE0005

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable, Binder]
    [Label("Scroll Rect", "UI")]
    public sealed partial class ScrollRectBinder : MonoBinder<ScrollRect>
    {
    }

    [Serializable, Binder]
    [Label("On Value Changed", "Scroll Rect")]
    public sealed partial class ScrollRectBindingOnValueChanged : MonoBindingCommand<ScrollRect>
    {
        private readonly UnityAction<Vector2> _command;

        public ScrollRectBindingOnValueChanged()
        {
            _command = OnValueChanged;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onValueChanged.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onValueChanged.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnValueChanged(Vector2 value);
    }

    [Serializable, Binder]
    [Label("Elasticity", "Scroll Rect")]
    public sealed partial class ScrollRectBindingElasticity : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetElasticity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].elasticity = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Inertia", "Scroll Rect")]
    public sealed partial class ScrollRectBindingInertia : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInertia(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].inertia = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Deceleration Rate", "Scroll Rect")]
    public sealed partial class ScrollRectBindingDecelerationRate : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDecelerationRate(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].decelerationRate = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Scroll Sensitivity", "Scroll Rect")]
    public sealed partial class ScrollRectBindingScrollSensitivity : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetScrollSensitivity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].scrollSensitivity = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Movement Type", "Scroll Rect")]
    public sealed partial class ScrollRectBindingMovementType : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMovementType(ScrollRect.MovementType value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].movementType = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Vertical", "Scroll Rect")]
    public sealed partial class ScrollRectBindingVertical : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetVertical(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].vertical = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Vertical Scrollbar Visibility", "Scroll Rect")]
    public sealed partial class ScrollRectBindingVerticalScrollbarVisibility : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetVerticalScrollbarVisibility(ScrollRect.ScrollbarVisibility value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].verticalScrollbarVisibility = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Vertical Normalized Position", "Scroll Rect")]
    public sealed partial class ScrollRectBindingVerticalNormalizedPosition : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetVerticalNormalizedPosition(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].verticalNormalizedPosition = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Horizontal", "Scroll Rect")]
    public sealed partial class ScrollRectBindingHorizontal : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetHorizontal(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].horizontal = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Horizontal Scrollbar Visibility", "Scroll Rect")]
    public sealed partial class ScrollRectBindingHorizontalScrollbarVisibility : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetHorizontalScrollbarVisibility(ScrollRect.ScrollbarVisibility value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].horizontalScrollbarVisibility = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Horizontal Normalized Position", "Scroll Rect")]
    public sealed partial class ScrollRectBindingHorizontalNormalizedPosition : MonoBindingProperty<ScrollRect>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetHorizontalNormalizedPosition(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].horizontalNormalizedPosition = value;
            }
        }
    }
}

#endif
