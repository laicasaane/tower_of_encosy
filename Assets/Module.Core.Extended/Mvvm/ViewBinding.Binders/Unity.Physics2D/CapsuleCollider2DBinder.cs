using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Capsule Collider 2D", "Physics 2D")]
    public sealed partial class CapsuleCollider2DBinder : MonoBinder<CapsuleCollider2D>
    {
    }

    [Serializable]
    [Label("Enabled", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingEnabled : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Size", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingSize : MonoBindingProperty<CapsuleCollider2D>, IBinder
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

    [Serializable]
    [Label("Direction", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingDirection : MonoBindingProperty<CapsuleCollider2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDirection(CapsuleDirection2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].direction = value;
            }
        }
    }

    [Serializable]
    [Label("Callback Layers", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingCallbackLayers : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Contact Capture Layers", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingContactCaptureLayers : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Density", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingDensity : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Exclude Layers", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingExcludeLayers : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Force Receive Layers", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingForceReceiveLayers : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Force Send Layers", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingForceSendLayers : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Include Layers", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingIncludeLayers : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Is Trigger", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingIsTrigger : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Layer Override Priority", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingLayerOverridePriority : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Offset", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingOffset : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Shared Material", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingSharedMaterial : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Used By Composite", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingUsedByComposite : MonoBindingProperty<CapsuleCollider2D>, IBinder
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
    [Label("Used By Effector", "Capsule Collider 2D")]
    public sealed partial class CapsuleCollider2DBindingUsedByEffector : MonoBindingProperty<CapsuleCollider2D>, IBinder
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

