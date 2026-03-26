#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable, Binder]
    [Label("Rect Transform")]
    public sealed partial class RectTransformBinder : MonoBinder<RectTransform>
    {
    }

    [Serializable, Binder]
    [Label("Anchored Position", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorPosition : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAnchoredPosition(in Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].anchoredPosition = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Anchored Position 3D", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorPosition3D : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAnchoredPosition3D(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].anchoredPosition3D = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Anchor Min", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorMin : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAnchorMin(in Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].anchorMin = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Anchor Max", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorMax : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAnchorMax(in Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].anchorMax = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Offset Min", "Rect Transform")]
    public sealed partial class RectTransformBindingOffsetMin : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOffsetMin(in Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].offsetMin = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Offset Max", "Rect Transform")]
    public sealed partial class RectTransformBindingOffsetMax : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOffsetMax(in Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].offsetMax = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Size Delta", "Rect Transform")]
    public sealed partial class RectTransformBindingSizeDelta : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSizeDelta(in Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sizeDelta = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Pivot", "Rect Transform")]
    public sealed partial class RectTransformBindingPivot : MonoBindingProperty<RectTransform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPivot(in Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].pivot = value;
            }
        }
    }
}
