#if UNITY_UGUI

using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Image", "UI")]
    public sealed partial class ImageBinder : MonoBinder<Image>
    {
    }

    [Serializable]
    [Label("Color", "Image")]
    public sealed partial class ImageBindingColor : MonoBindingProperty<Image>, IBinder
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
    [Label("Sprite", "Image")]
    public sealed partial class ImageBindingSprite : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSprite(Sprite value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sprite = value;
            }
        }
    }

    [Serializable]
    [Label("Sprite (Native Size)", "Image")]
    public sealed partial class ImageBindingSpriteNativeSize : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpriteNativeSize(Sprite value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];
                target.sprite = value;
                target.SetNativeSize();
            }
        }
    }

    [Serializable]
    [Label("Material", "Image")]
    public sealed partial class ImageBindingMaterial : MonoBindingProperty<Image>, IBinder
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
    [Label("Raycast Target", "Image")]
    public sealed partial class ImageBindingRaycastTarget : MonoBindingProperty<Image>, IBinder
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
    [Label("Maskable", "Image")]
    public sealed partial class ImageBindingMaskable : MonoBindingProperty<Image>, IBinder
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
    [Label("ImageType", "Image")]
    public sealed partial class ImageBindingImageType : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetImageType(Image.Type value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].type = value;
            }
        }
    }

    [Serializable]
    [Label("Use Sprite Mesh", "Image")]
    public sealed partial class ImageBindingUseSpriteMesh : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseSpriteMesh(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useSpriteMesh = value;
            }
        }
    }

    [Serializable]
    [Label("Preserve Aspect", "Image")]
    public sealed partial class ImageBindingPreserveAspect : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPreserveAspect(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].preserveAspect = value;
            }
        }
    }

    [Serializable]
    [Label("Pixels Per Unit Multiplier", "Image")]
    public sealed partial class ImageBindingPixelsPerUnitMultiplier : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPixelsPerUnitMultiplier(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].pixelsPerUnitMultiplier = value;
            }
        }
    }

    [Serializable]
    [Label("Fill Method", "Image")]
    public sealed partial class ImageBindingFillMethod : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFillMethod(Image.FillMethod value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].fillMethod = value;
            }
        }
    }

    [Serializable]
    [Label("Fill Origin", "Image")]
    public sealed partial class ImageBindingFillOrigin : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFillOrigin(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].fillOrigin = value;
            }
        }
    }

    [Serializable]
    [Label("Fill Amount", "Image")]
    public sealed partial class ImageBindingFillAmount : MonoBindingProperty<Image>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFillAmount(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].fillAmount = value;
            }
        }
    }
}

#endif
