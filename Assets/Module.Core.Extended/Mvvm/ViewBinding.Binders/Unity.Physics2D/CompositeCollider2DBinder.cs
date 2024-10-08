using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Composite Collider 2D", "Physics 2D")]
    public sealed partial class CompositeCollider2DBinder : MonoBinder<CompositeCollider2D>
    {
    }

    [Serializable]
    [Label("Edge Radius", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingEdgeRadius : MonoBindingProperty<CompositeCollider2D>, IBinder
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

    [Serializable]
    [Label("Generation Type", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingGenerationType : MonoBindingProperty<CompositeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetGenerationType(CompositeCollider2D.GenerationType value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].generationType = value;
            }
        }
    }

    [Serializable]
    [Label("Geometry Type", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingGeometryType : MonoBindingProperty<CompositeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetGeometryType(CompositeCollider2D.GeometryType value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].geometryType = value;
            }
        }
    }

    [Serializable]
    [Label("Offset Distance", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingOffsetDistance : MonoBindingProperty<CompositeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOffsetDistance(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].offsetDistance = value;
            }
        }
    }

    [Serializable]
    [Label("Use Delaunay Mesh", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingUseDelaunayMesh : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Vertex Distance", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingVertexDistance : MonoBindingProperty<CompositeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetVertexDistance(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].vertexDistance = value;
            }
        }
    }

    [Serializable]
    [Label("Enabled", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingEnabled : MonoBindingProperty<CompositeCollider2D>, IBinder
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

    [Serializable]
    [Label("Callback Layers", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingCallbackLayers : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Contact Capture Layers", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingContactCaptureLayers : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Density", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingDensity : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Exclude Layers", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingExcludeLayers : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Force Receive Layers", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingForceReceiveLayers : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Force Send Layers", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingForceSendLayers : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Include Layers", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingIncludeLayers : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Is Trigger", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingIsTrigger : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Layer Override Priority", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingLayerOverridePriority : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Offset", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingOffset : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Shared Material", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingSharedMaterial : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Used By Composite", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingUsedByComposite : MonoBindingProperty<CompositeCollider2D>, IBinder
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
    [Label("Used By Effector", "Composite Collider 2D")]
    public sealed partial class CompositeCollider2DBindingUsedByEffector : MonoBindingProperty<CompositeCollider2D>, IBinder
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

