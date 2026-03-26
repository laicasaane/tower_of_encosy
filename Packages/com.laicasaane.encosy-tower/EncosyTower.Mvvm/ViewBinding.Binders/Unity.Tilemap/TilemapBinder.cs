#if UNITY_TILEMAP

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using EncosyTower.Variants;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Tilemaps
{
    [Serializable, Binder]
    [Label("Tilemap", "Tilemap")]
    public sealed partial class TilemapBinder : MonoBinder<Tilemap>
    {
    }

    [Serializable, Binder]
    [Label("Animation Frame Rate", "Tilemap")]
    public sealed partial class TilemapBindingAnimationFrameRate : MonoBindingProperty<Tilemap>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAnimationFrameRate(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].animationFrameRate = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Color", "Tilemap")]
    public sealed partial class TilemapBindingColor : MonoBindingProperty<Tilemap>
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
    [Label("Orientation", "Tilemap")]
    public sealed partial class TilemapBindingOrientation : MonoBindingProperty<Tilemap>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOrientation(Tilemap.Orientation value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].orientation = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Orientation Matrix", "Tilemap")]
    public sealed partial class TilemapBindingOrientationMatrix : MonoBindingProperty<Tilemap>
    {
        partial void OnBeforeConstructor()
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (VariantData.BYTE_COUNT >= 64)
            {
                return;
            }
#pragma warning restore CS0162 // Unreachable code detected

            ThrowNotSupported();
        }

        [BindingProperty]
        [field: HideInInspector]
        private void SetOrientationMatrix(in Matrix4x4 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].orientationMatrix = value;
            }
        }

        private static void ThrowNotSupported()
        {
            Logging.StaticLogger.LogException(new NotSupportedException(
                "Tilemap Orientation Matrix binding property requires the symbol VARIANT_64_BYTES or higher to be defined"
            ));
        }
    }

    [Serializable, Binder]
    [Label("Origin", "Tilemap")]
    public sealed partial class TilemapBindingOrigin : MonoBindingProperty<Tilemap>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOrigin(in Vector3Int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].origin = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Size", "Tilemap")]
    public sealed partial class TilemapBindingSize : MonoBindingProperty<Tilemap>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSize(in Vector3Int value)
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
    [Label("Tile Anchor", "Tilemap")]
    public sealed partial class TilemapBindingTileAnchor : MonoBindingProperty<Tilemap>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTileAnchor(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].tileAnchor = value;
            }
        }
    }
}

#endif
