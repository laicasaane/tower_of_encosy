#if UNITY_UGUI

using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Horizontal Layout Group", "UI")]
    public sealed partial class HorizontalLayoutGroupBinder : MonoBinder<HorizontalLayoutGroup>
    {
    }

    [Serializable]
    [Label("Spacing", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingSpacing : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Child Alignment", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingChildAlignment : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Reverse Arrangement", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingReverseArrangement : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Control Child Width", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingControlChildWidth : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Control Child Height", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingControlChildHeight : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Use Child Scale Width", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingUseChildScaleWidth : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Use Child Scale Height", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingUseChildScaleHeight : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Child Force Expand Width", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingChildForceExpandWidth : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
    [Label("Child Force Expand Height", "Horizontal Layout Group")]
    public sealed partial class HorizontalLayoutGroupBindingChildForceExpandHeight : MonoBindingProperty<HorizontalLayoutGroup>, IBinder
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
