#if ENABLE_TILEMAP

using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using EncosyTower.Modules.Unions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Tilemaps
{
    [Serializable]
    [Label("Tilemap", "Tilemap")]
    public sealed partial class TilemapBinder : MonoBinder<Tilemap>
    {
    }

    [Serializable]
    [Label("Animation Frame Rate", "Tilemap")]
    public sealed partial class TilemapBindingAnimationFrameRate : MonoBindingProperty<Tilemap>, IBinder
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

    [Serializable]
    [Label("Color", "Tilemap")]
    public sealed partial class TilemapBindingColor : MonoBindingProperty<Tilemap>, IBinder
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
    [Label("Orientation", "Tilemap")]
    public sealed partial class TilemapBindingOrientation : MonoBindingProperty<Tilemap>, IBinder
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

    [Serializable]
    [Label("Orientation Matrix", "Tilemap")]
    public sealed partial class TilemapBindingOrientationMatrix : MonoBindingProperty<Tilemap>, IBinder
    {
        partial void OnBeforeConstructor()
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (UnionData.BYTE_COUNT >= 64)
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
            Logging.RuntimeLoggerAPI.LogException(new NotSupportedException(
                "Tilemap Orientation Matrix binding property requires the symbol UNION_64_BYTES or higher to be defined"
            ));
        }
    }

    [Serializable]
    [Label("Origin", "Tilemap")]
    public sealed partial class TilemapBindingOrigin : MonoBindingProperty<Tilemap>, IBinder
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

    [Serializable]
    [Label("Size", "Tilemap")]
    public sealed partial class TilemapBindingSize : MonoBindingProperty<Tilemap>, IBinder
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

    [Serializable]
    [Label("Tile Anchor", "Tilemap")]
    public sealed partial class TilemapBindingTileAnchor : MonoBindingProperty<Tilemap>, IBinder
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
