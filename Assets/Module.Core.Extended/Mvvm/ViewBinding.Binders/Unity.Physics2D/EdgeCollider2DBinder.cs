using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Edge Collider 2D", "Physics 2D")]
    public sealed partial class EdgeCollider2DBinder : MonoBinder<EdgeCollider2D>
    {
    }

    [Serializable]
    [Label("Adjacent End Point", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingAdjacentEndPoint : MonoBindingProperty<EdgeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAdjacentEndPoint(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].adjacentEndPoint = value;
            }
        }
    }

    [Serializable]
    [Label("Adjacent Start Point", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingAdjacentStartPoint : MonoBindingProperty<EdgeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAdjacentStartPoint(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].adjacentStartPoint = value;
            }
        }
    }

    [Serializable]
    [Label("Edge Radius", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingEdgeRadius : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Points", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingPoints : MonoBindingProperty<EdgeCollider2D>, IBinder
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

    [Serializable]
    [Label("Use Adjacent End Point", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingUseAdjacentEndPoint : MonoBindingProperty<EdgeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseAdjacentEndPoint(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useAdjacentEndPoint = value;
            }
        }
    }

    [Serializable]
    [Label("Enabled", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingEnabled : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Callback Layers", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingCallbackLayers : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Contact Capture Layers", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingContactCaptureLayers : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Density", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingDensity : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Exclude Layers", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingExcludeLayers : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Force Receive Layers", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingForceReceiveLayers : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Force Send Layers", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingForceSendLayers : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Include Layers", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingIncludeLayers : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Is Trigger", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingIsTrigger : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Layer Override Priority", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingLayerOverridePriority : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Offset", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingOffset : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Shared Material", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingSharedMaterial : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Used By Composite", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingUsedByComposite : MonoBindingProperty<EdgeCollider2D>, IBinder
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
    [Label("Used By Effector", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingUsedByEffector : MonoBindingProperty<EdgeCollider2D>, IBinder
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

    [Serializable]
    [Label("Use Adjacent Start Point", "Edge Collider 2D")]
    public sealed partial class EdgeCollider2DBindingUseAdjacentStartPoint : MonoBindingProperty<EdgeCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseAdjacentStartPoint(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useAdjacentStartPoint = value;
            }
        }
    }
}

