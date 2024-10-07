using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Tilemaps
{
    [Serializable]
    [Label("Tilemap", "Tilemap")]
    public sealed partial class TilemapBinder : MonoBinder<Tilemap>
    {
    }

    [Serializable]
    [Label("Animation Frame Rate", "Tilemap")]
    public sealed partial class TilemapAnimationFrameRate : MonoBindingProperty<Tilemap>, IBinder
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
    public sealed partial class TilemapColor : MonoBindingProperty<Tilemap>, IBinder
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
    [Label("Orientation", "Tilemap")]
    public sealed partial class TilemapOrientation : MonoBindingProperty<Tilemap>, IBinder
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
    public sealed partial class TilemapOrientationMatrix : MonoBindingProperty<Tilemap>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOrientationMatrix(Matrix4x4 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].orientationMatrix = value;
            }
        }
    }

    [Serializable]
    [Label("Origin", "Tilemap")]
    public sealed partial class TilemapOrigin : MonoBindingProperty<Tilemap>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOrigin(Vector3Int value)
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
    public sealed partial class TilemapSize : MonoBindingProperty<Tilemap>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSize(Vector3Int value)
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
    public sealed partial class TilemapTileAnchor : MonoBindingProperty<Tilemap>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTileAnchor(Vector3 value)
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
