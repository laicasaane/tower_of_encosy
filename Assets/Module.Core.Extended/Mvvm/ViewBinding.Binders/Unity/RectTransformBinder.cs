using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable]
    [Label("Rect Transform")]
    public sealed partial class RectTransformBinder : MonoBinder<RectTransform>
    {
    }

    [Serializable]
    [Label("Anchored Position", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorPosition : MonoBindingProperty<RectTransform>, IBinder
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

    [Serializable]
    [Label("Anchored Position 3D", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorPosition3D : MonoBindingProperty<RectTransform>, IBinder
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

    [Serializable]
    [Label("Anchor Min", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorMin : MonoBindingProperty<RectTransform>, IBinder
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

    [Serializable]
    [Label("Anchor Max", "Rect Transform")]
    public sealed partial class RectTransformBindingAnchorMax : MonoBindingProperty<RectTransform>, IBinder
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

    [Serializable]
    [Label("Offset Min", "Rect Transform")]
    public sealed partial class RectTransformBindingOffsetMin : MonoBindingProperty<RectTransform>, IBinder
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

    [Serializable]
    [Label("Offset Max", "Rect Transform")]
    public sealed partial class RectTransformBindingOffsetMax : MonoBindingProperty<RectTransform>, IBinder
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

    [Serializable]
    [Label("Size Delta", "Rect Transform")]
    public sealed partial class RectTransformBindingSizeDelta : MonoBindingProperty<RectTransform>, IBinder
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

    [Serializable]
    [Label("Pivot", "Rect Transform")]
    public sealed partial class RectTransformBindingPivot : MonoBindingProperty<RectTransform>, IBinder
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
