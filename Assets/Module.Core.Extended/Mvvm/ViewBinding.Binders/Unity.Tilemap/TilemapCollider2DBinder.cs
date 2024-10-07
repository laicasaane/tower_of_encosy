using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Tilemaps
{
    [Serializable]
    [Label("Tilemap Collider 2D", "Tilemap")]
    public sealed partial class TilemapCollider2DBinder : MonoBinder<TilemapCollider2D>
    {
    }

    [Serializable]
    [Label("Extrusion Factor", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DExtrusionFactor : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetExtrusionFactor(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].extrusionFactor = value;
            }
        }
    }

    [Serializable]
    [Label("Maximum Tile Change Count", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DMaximumTileChangeCount : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaximumTileChangeCount(uint value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maximumTileChangeCount = value;
            }
        }
    }

    [Serializable]
    [Label("Use Delaunay Mesh", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DUseDelaunayMesh : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseDelaunayMesh(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useDelaunayMesh = value;
            }
        }
    }

    [Serializable]
    [Label("Callback Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DCallbackLayers : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCallbackLayers(LayerMask value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].callbackLayers = value;
            }
        }
    }

    [Serializable]
    [Label("Contact Capture Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DContactCaptureLayers : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetContactCaptureLayers(LayerMask value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].contactCaptureLayers = value;
            }
        }
    }

    [Serializable]
    [Label("Density", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DDensity : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDensity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].density = value;
            }
        }
    }

    [Serializable]
    [Label("Exclude Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DExcludeLayers : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetExcludeLayers(LayerMask value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].excludeLayers = value;
            }
        }
    }

    [Serializable]
    [Label("Force Receive Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DForceReceiveLayers : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceReceiveLayers(LayerMask value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceReceiveLayers = value;
            }
        }
    }

    [Serializable]
    [Label("Force Send Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DForceSendLayers : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceSendLayers(LayerMask value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceSendLayers = value;
            }
        }
    }

    [Serializable]
    [Label("Include Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DIncludeLayers : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetIncludeLayers(LayerMask value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].includeLayers = value;
            }
        }
    }

    [Serializable]
    [Label("Is Trigger", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DIsTrigger : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetIsTrigger(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].isTrigger = value;
            }
        }
    }

    [Serializable]
    [Label("Layer Override Priority", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DLayerOverridePriority : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLayerOverridePriority(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].layerOverridePriority = value;
            }
        }
    }

    [Serializable]
    [Label("Offset", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DOffset : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOffset(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].offset = value;
            }
        }
    }

    [Serializable]
    [Label("Shared Material", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DSharedMaterial : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSharedMaterial(PhysicsMaterial2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sharedMaterial = value;
            }
        }
    }

    [Serializable]
    [Label("Used By Composite", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DUsedByComposite : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUsedByComposite(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].usedByComposite = value;
            }
        }
    }

    [Serializable]
    [Label("Used By Effector", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DUsedByEffector : MonoBindingProperty<TilemapCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUsedByEffector(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].usedByEffector = value;
            }
        }
    }
}
