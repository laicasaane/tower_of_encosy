#if UNITY_PHYSICS_2D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics2D
{

    [Serializable, Binder]
    [Label("Box Collider 2D", "Physics 2D")]
    public sealed partial class BoxCollider2DBinder : MonoBinder<BoxCollider2D>
    {
    }

    [Serializable, Binder]
    [Label("Auto Tiling", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingAutoTiling : MonoBindingProperty<BoxCollider2D>
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
    [Label("Edge Radius", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingEdgeRadius : MonoBindingProperty<BoxCollider2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetEdgeRadius(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].edgeRadius = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Enabled", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingEnabled : MonoBindingProperty<BoxCollider2D>
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
    [Label("Size", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingSize : MonoBindingProperty<BoxCollider2D>
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
    [Label("Callback Layers", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingCallbackLayers : MonoBindingProperty<BoxCollider2D>
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
    [Label("Contact Capture Layers", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingContactCaptureLayers : MonoBindingProperty<BoxCollider2D>
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
    [Label("Density", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingDensity : MonoBindingProperty<BoxCollider2D>
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
    [Label("Exclude Layers", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingExcludeLayers : MonoBindingProperty<BoxCollider2D>
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
    [Label("Force Receive Layers", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingForceReceiveLayers : MonoBindingProperty<BoxCollider2D>
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
    [Label("Force Send Layers", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingForceSendLayers : MonoBindingProperty<BoxCollider2D>
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
    [Label("Include Layers", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingIncludeLayers : MonoBindingProperty<BoxCollider2D>
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
    [Label("Is Trigger", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingIsTrigger : MonoBindingProperty<BoxCollider2D>
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
    [Label("Layer Override Priority", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingLayerOverridePriority : MonoBindingProperty<BoxCollider2D>
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
    [Label("Offset", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingOffset : MonoBindingProperty<BoxCollider2D>
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
    [Label("Shared Material", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingSharedMaterial : MonoBindingProperty<BoxCollider2D>
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
    [Obsolete("Use BoxCollider2DBindingCompositeOperation instead")]
#else
    [Label("Used By Composite", "Box Collider 2D")]
#endif
    public sealed partial class BoxCollider2DBindingUsedByComposite : MonoBindingProperty<BoxCollider2D>
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
    [Label("Composite Operation", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingCompositeOperation : MonoBindingProperty<BoxCollider2D>
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
    [Label("Used By Effector", "Box Collider 2D")]
    public sealed partial class BoxCollider2DBindingUsedByEffector : MonoBindingProperty<BoxCollider2D>
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
