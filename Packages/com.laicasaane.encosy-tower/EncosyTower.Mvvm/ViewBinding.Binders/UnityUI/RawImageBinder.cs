#if UNITY_UGUI

#pragma warning disable CS0657, IDE0005

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable, Binder]
    [Label("Raw Image", "UI")]
    public sealed partial class RawImageBinder : MonoBinder<RawImage>
    {
    }

    [Serializable, Binder]
    [Label("Color", "Raw Image")]
    public sealed partial class RawImageBindingColor : MonoBindingProperty<RawImage>
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

    [Serializable, Binder]
    [Label("Texture", "Raw Image")]
    public sealed partial class RawImageBindingTexture : MonoBindingProperty<RawImage>
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

    [Serializable, Binder]
    [Label("Texture (Native Size)", "Raw Image")]
    public sealed partial class RawImageBindingTextureNativeSize : MonoBindingProperty<RawImage>
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

    [Serializable, Binder]
    [Label("Material", "Raw Image")]
    public sealed partial class RawImageBindingMaterial : MonoBindingProperty<RawImage>
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

    [Serializable, Binder]
    [Label("Raycast Target", "Raw Image")]
    public sealed partial class RawImageBindingRaycastTarget : MonoBindingProperty<RawImage>
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

    [Serializable, Binder]
    [Label("Maskable", "Raw Image")]
    public sealed partial class RawImageBindingMaskable : MonoBindingProperty<RawImage>
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

    [Serializable, Binder]
    [Label("UV Rect", "Raw Image")]
    public sealed partial class RawImageBindingUVRect : MonoBindingProperty<RawImage>
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
