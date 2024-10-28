#if UNITY_UGUI

using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Raw Image", "UI")]
    public sealed partial class RawImageBinder : MonoBinder<RawImage>
    {
    }

    [Serializable]
    [Label("Color", "Raw Image")]
    public sealed partial class RawImageBindingColor : MonoBindingProperty<RawImage>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetColor(in Color value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].color = value;
            }
        }
    }

    [Serializable]
    [Label("Texture", "Raw Image")]
    public sealed partial class RawImageBindingTexture : MonoBindingProperty<RawImage>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTexture(Texture value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].texture = value;
            }
        }
    }

    [Serializable]
    [Label("Texture (Native Size)", "Raw Image")]
    public sealed partial class RawImageBindingTextureNativeSize : MonoBindingProperty<RawImage>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTextureNativeSize(Texture value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];
                target.texture = value;
                target.SetNativeSize();
            }
        }
    }

    [Serializable]
    [Label("Material", "Raw Image")]
    public sealed partial class RawImageBindingMaterial : MonoBindingProperty<RawImage>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaterial(Material value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].material = value;
            }
        }
    }

    [Serializable]
    [Label("Raycast Target", "Raw Image")]
    public sealed partial class RawImageBindingRaycastTarget : MonoBindingProperty<RawImage>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRaycastTarget(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].raycastTarget = value;
            }
        }
    }

    [Serializable]
    [Label("Maskable", "Raw Image")]
    public sealed partial class RawImageBindingMaskable : MonoBindingProperty<RawImage>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaskable(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maskable = value;
            }
        }
    }

    [Serializable]
    [Label("UV Rect", "Raw Image")]
    public sealed partial class RawImageBindingUVRect : MonoBindingProperty<RawImage>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUVRect(in Rect value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].uvRect = value;
            }
        }
    }
}

#endif
