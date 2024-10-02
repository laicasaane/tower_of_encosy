#if UNITY_UGUI

using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Sprite Renderer")]
    public sealed partial class SpriteRendererBinder : MonoBinder<SpriteRenderer>
    {
    }

    [Serializable]
    [Label("Adaptive Mode Threshold", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingAdaptiveModeThreshold : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAdaptiveModeThreshold(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].adaptiveModeThreshold = value;
            }
        }
    }

    [Serializable]
    [Label("Color", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingColor : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetColor(Color value)
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
    [Label("Draw Mode", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingDrawMode : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDrawMode(SpriteDrawMode value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].drawMode = value;
            }
        }
    }

    [Serializable]
    [Label("Flip X", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingFlipX : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFlipX(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].flipX = value;
            }
        }
    }

    [Serializable]
    [Label("Flip Y", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingFlipY : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFlipY(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].flipY = value;
            }
        }
    }

    [Serializable]
    [Label("Mask Interaction", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingMaskInteraction : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaskInteraction(SpriteMaskInteraction value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maskInteraction = value;
            }
        }
    }

    [Serializable]
    [Label("Size", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingSize : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSize(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].size = value;
            }
        }
    }

    [Serializable]
    [Label("Sprite", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingSprite : MonoPropertyBinding<SpriteRenderer>, IBinder
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
    [Label("Sprite Sort Point", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingSpriteSortPoint : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpriteSortPoint(SpriteSortPoint value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].spriteSortPoint = value;
            }
        }
    }

    [Serializable]
    [Label("Tile Mode", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingTileMode : MonoPropertyBinding<SpriteRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTileMode(SpriteTileMode value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].tileMode = value;
            }
        }
    }
}

#endif
