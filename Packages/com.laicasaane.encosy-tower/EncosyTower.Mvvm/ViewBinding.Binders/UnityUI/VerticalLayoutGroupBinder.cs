#if UNITY_UGUI

#pragma warning disable CS0657, IDE0005

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Vertical Layout Group", "UI")]
    public sealed partial class VerticalLayoutGroupBinder : MonoBinder<VerticalLayoutGroup>
    {
    }

    [Serializable]
    [Label("Spacing", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingSpacing : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpacing(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].spacing = value;
            }
        }
    }

    [Serializable]
    [Label("Child Alignment", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingChildAlignment : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetChildAlignment(TextAnchor value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].childAlignment = value;
            }
        }
    }

    [Serializable]
    [Label("Reverse Arrangement", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingReverseArrangement : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetReverseArrangement(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].reverseArrangement = value;
            }
        }
    }

    [Serializable]
    [Label("Control Child Width", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingControlChildWidth : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetControlChildWidth(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].childControlWidth = value;
            }
        }
    }

    [Serializable]
    [Label("Control Child Height", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingControlChildHeight : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetControlChildHeight(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].childControlHeight = value;
            }
        }
    }

    [Serializable]
    [Label("Use Child Scale Width", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingUseChildScaleWidth : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseChildScaleWidth(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].childScaleWidth = value;
            }
        }
    }

    [Serializable]
    [Label("Use Child Scale Height", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingUseChildScaleHeight : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseChildScaleHeight(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].childScaleHeight = value;
            }
        }
    }

    [Serializable]
    [Label("Child Force Expand Width", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingChildForceExpandWidth : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetChildForceExpandWidth(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].childForceExpandWidth = value;
            }
        }
    }

    [Serializable]
    [Label("Child Force Expand Height", "Vertical Layout Group")]
    public sealed partial class VerticalLayoutGroupBindingChildForceExpandHeight : MonoBindingProperty<VerticalLayoutGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetChildForceExpandHeight(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].childForceExpandHeight = value;
            }
        }
    }
}

#endif
