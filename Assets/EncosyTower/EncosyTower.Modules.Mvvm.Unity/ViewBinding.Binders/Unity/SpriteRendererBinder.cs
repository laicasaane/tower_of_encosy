using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable]
    [Label("Sprite Renderer")]
    public sealed partial class SpriteRendererBinder : MonoBinder<SpriteRenderer>
    {
    }

    [Serializable]
    [Label("Adaptive Mode Threshold", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingAdaptiveModeThreshold : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingColor : MonoBindingProperty<SpriteRenderer>, IBinder
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
    [Label("Draw Mode", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingDrawMode : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingFlipX : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingFlipY : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingMaskInteraction : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingSize : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingSprite : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingSpriteSortPoint : MonoBindingProperty<SpriteRenderer>, IBinder
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
    public sealed partial class SpriteRendererBindingTileMode : MonoBindingProperty<SpriteRenderer>, IBinder
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
