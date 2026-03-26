#if UNITY_PHYSICS_2D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics2D
{

    [Serializable, Binder]
    [Label("Polygon Collider 2D", "Physics 2D")]
    public sealed partial class PolygonCollider2DBinder : MonoBinder<PolygonCollider2D>
    {
    }

    [Serializable, Binder]
    [Label("Points", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingPoints : MonoBindingProperty<PolygonCollider2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPoints(Vector2[] value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].points = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Path Count", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingPathCount : MonoBindingProperty<PolygonCollider2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPathCount(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].pathCount = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Use Delaunay Mesh", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingUseDelaunayMesh : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Enabled", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingEnabled : MonoBindingProperty<PolygonCollider2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetEnabled(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].enabled = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Auto Tiling", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingAutoTiling : MonoBindingProperty<PolygonCollider2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutoTiling(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].autoTiling = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Callback Layers", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingCallbackLayers : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Contact Capture Layers", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingContactCaptureLayers : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Density", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingDensity : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Exclude Layers", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingExcludeLayers : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Force Receive Layers", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingForceReceiveLayers : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Force Send Layers", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingForceSendLayers : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Include Layers", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingIncludeLayers : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Is Trigger", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingIsTrigger : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Layer Override Priority", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingLayerOverridePriority : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Offset", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingOffset : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Shared Material", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingSharedMaterial : MonoBindingProperty<PolygonCollider2D>
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
    [Obsolete("Use PolygonCollider2DBindingCompositeOperation instead")]
#else
    [Label("Used By Composite", "Polygon Collider 2D")]
#endif
    public sealed partial class PolygonCollider2DBindingUsedByComposite : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Composite Operation", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingCompositeOperation : MonoBindingProperty<PolygonCollider2D>
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
    [Label("Used By Effector", "Polygon Collider 2D")]
    public sealed partial class PolygonCollider2DBindingUsedByEffector : MonoBindingProperty<PolygonCollider2D>
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
