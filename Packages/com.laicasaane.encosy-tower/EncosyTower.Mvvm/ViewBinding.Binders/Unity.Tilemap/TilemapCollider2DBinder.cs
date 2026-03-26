#if UNITY_TILEMAP

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Tilemaps
{
    [Serializable, Binder]
    [Label("Tilemap Collider 2D", "Tilemap")]
    public sealed partial class TilemapCollider2DBinder : MonoBinder<TilemapCollider2D>
    {
    }

    [Serializable, Binder]
    [Label("Extrusion Factor", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingExtrusionFactor : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Maximum Tile Change Count", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingMaximumTileChangeCount : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Use Delaunay Mesh", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingUseDelaunayMesh : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Callback Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingCallbackLayers : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Contact Capture Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingContactCaptureLayers : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Density", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingDensity : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Exclude Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingExcludeLayers : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Force Receive Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingForceReceiveLayers : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Force Send Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingForceSendLayers : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Include Layers", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingIncludeLayers : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Is Trigger", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingIsTrigger : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Layer Override Priority", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingLayerOverridePriority : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Offset", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingOffset : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
    [Label("Shared Material", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingSharedMaterial : MonoBindingProperty<TilemapCollider2D>
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

    [Serializable, Binder]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use TilemapCollider2DBindingCompositeOperation instead")]
#else
    [Label("Used By Composite", "Tilemap Collider 2D")]
#endif
    public sealed partial class TilemapCollider2DBindingUsedByComposite : MonoBindingProperty<TilemapCollider2D>
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

#if UNITY_6000_0_OR_NEWER
    [Serializable, Binder]
    [Label("Composite Operation", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingCompositeOperation : MonoBindingProperty<TilemapCollider2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCompositeOperation(Collider2D.CompositeOperation value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].compositeOperation = value;
            }
        }
    }
#endif

    [Serializable, Binder]
    [Label("Used By Effector", "Tilemap Collider 2D")]
    public sealed partial class TilemapCollider2DBindingUsedByEffector : MonoBindingProperty<TilemapCollider2D>
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

#endif
