#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable, Binder]
    [Label("Sprite Renderer")]
    public sealed partial class SpriteRendererBinder : MonoBinder<SpriteRenderer>
    {
    }

    [Serializable, Binder]
    [Label("Adaptive Mode Threshold", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingAdaptiveModeThreshold : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Color", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingColor : MonoBindingProperty<SpriteRenderer>
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
    [Label("Draw Mode", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingDrawMode : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Flip X", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingFlipX : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Flip Y", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingFlipY : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Mask Interaction", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingMaskInteraction : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Size", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingSize : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Sprite", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingSprite : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Sprite Sort Point", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingSpriteSortPoint : MonoBindingProperty<SpriteRenderer>
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

    [Serializable, Binder]
    [Label("Tile Mode", "Sprite Renderer")]
    public sealed partial class SpriteRendererBindingTileMode : MonoBindingProperty<SpriteRenderer>
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
