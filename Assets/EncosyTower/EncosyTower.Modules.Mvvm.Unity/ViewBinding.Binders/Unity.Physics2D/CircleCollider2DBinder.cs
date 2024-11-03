using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Circle Collider 2D", "Physics 2D")]
    public sealed partial class CircleCollider2DBinder : MonoBinder<CircleCollider2D>
    {
    }

    [Serializable]
    [Label("Radius", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingRadius : MonoBindingProperty<CircleCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRadius(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].radius = value;
            }
        }
    }

    [Serializable]
    [Label("Enabled", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingEnabled : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Callback Layers", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingCallbackLayers : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Contact Capture Layers", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingContactCaptureLayers : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Density", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingDensity : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Exclude Layers", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingExcludeLayers : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Force Receive Layers", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingForceReceiveLayers : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Force Send Layers", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingForceSendLayers : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Include Layers", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingIncludeLayers : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Is Trigger", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingIsTrigger : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Layer Override Priority", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingLayerOverridePriority : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Offset", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingOffset : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Label("Shared Material", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingSharedMaterial : MonoBindingProperty<CircleCollider2D>, IBinder
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
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use CircleCollider2DBindingCompositeOperation instead")]
#else
    [Label("Used By Composite", "Circle Collider 2D")]
#endif
    public sealed partial class CircleCollider2DBindingUsedByComposite : MonoBindingProperty<CircleCollider2D>, IBinder
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
    [Serializable]
    [Label("Composite Operation", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingCompositeOperation : MonoBindingProperty<CircleCollider2D>, IBinder
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

    [Serializable]
    [Label("Used By Effector", "Circle Collider 2D")]
    public sealed partial class CircleCollider2DBindingUsedByEffector : MonoBindingProperty<CircleCollider2D>, IBinder
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

