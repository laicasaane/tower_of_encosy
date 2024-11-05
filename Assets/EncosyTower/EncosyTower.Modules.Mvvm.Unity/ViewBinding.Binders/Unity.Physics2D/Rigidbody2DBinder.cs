using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Rigidbody 2D", "Physics 2D")]
    public sealed partial class Rigidbody2DBinder : MonoBinder<Rigidbody2D>
    {
    }

    [Serializable]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use Rigidbody2DBindingAngularDamping instead")]
#else
    [Label("Angular Drag", "Rigidbody 2D")]
#endif
    public sealed partial class Rigidbody2DBindingAngularDrag : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAngularDrag(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].angularDrag = value;
            }
        }
    }

#if UNITY_6000_0_OR_NEWER
    [Serializable]
    [Label("Angular Damping", "Rigidbody 2D")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true
        , "EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D"
        , "EncosyTower.Modules.Mvvm.Unity"
        , "Rigidbody2DBindingAngularDrag"
    )]
    public sealed partial class Rigidbody2DBindingAngularDamping : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAngularDamping(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].angularDamping = value;
            }
        }
    }
#endif

    [Serializable]
    [Label("Angular Velocity", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingAngularVelocity : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAngularVelocity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].angularVelocity = value;
            }
        }
    }

    [Serializable]
    [Label("Body Type", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingBodyType : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetBodyType(RigidbodyType2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].bodyType = value;
            }
        }
    }

    [Serializable]
    [Label("Center Of Mass", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingCenterOfMass : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCenterOfMass(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].centerOfMass = value;
            }
        }
    }

    [Serializable]
    [Label("Collision Detection Mode", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingCollisionDetectionMode : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCollisionDetectionMode(CollisionDetectionMode2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].collisionDetectionMode = value;
            }
        }
    }

    [Serializable]
    [Label("Constraints", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingConstraints : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConstraints(RigidbodyConstraints2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].constraints = value;
            }
        }
    }

    [Serializable]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use Rigidbody2DBindingLinearDamping instead")]
#else
    [Label("Drag", "Rigidbody 2D")]
#endif
    public sealed partial class Rigidbody2DBindingDrag : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDrag(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].drag = value;
            }
        }
    }

#if UNITY_6000_0_OR_NEWER
    [Serializable]
    [Label("Linear Damping", "Rigidbody 2D")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true
        , "EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D"
        , "EncosyTower.Modules.Mvvm.Unity"
        , "Rigidbody2DBindingDrag"
    )]
    public sealed partial class Rigidbody2DBindingLinearDamping : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLinearDamping(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].linearDamping = value;
            }
        }
    }
#endif

    [Serializable]
    [Label("Exclude Layers", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingExcludeLayers : MonoBindingProperty<Rigidbody2D>, IBinder
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
    [Label("Freeze Rotation", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingFreezeRotation : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFreezeRotation(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].freezeRotation = value;
            }
        }
    }

    [Serializable]
    [Label("Gravity Scale", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingGravityScale : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetGravityScale(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].gravityScale = value;
            }
        }
    }

    [Serializable]
    [Label("Include Layers", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingIncludeLayers : MonoBindingProperty<Rigidbody2D>, IBinder
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
    [Label("Inertia", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingInertia : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInertia(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].inertia = value;
            }
        }
    }

    [Serializable]
    [Label("Interpolation", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingInterpolation : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInterpolation(RigidbodyInterpolation2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].interpolation = value;
            }
        }
    }

    [Serializable]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use Rigidbody2DBindingBodyType instead")]
#else
    [Label("Is Kinematic", "Rigidbody 2D")]
#endif
    public sealed partial class Rigidbody2DBindingIsKinematic : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetIsKinematic(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].isKinematic = value;
            }
        }
    }

    [Serializable]
    [Label("Mass", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingMass : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMass(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].mass = value;
            }
        }
    }

    [Serializable]
    [Label("Position", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingPosition : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPosition(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].position = value;
            }
        }
    }

    [Serializable]
    [Label("Rotation", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingRotation : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRotation(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].rotation = value;
            }
        }
    }

    [Serializable]
    [Label("Shared Material", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingSharedMaterial : MonoBindingProperty<Rigidbody2D>, IBinder
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
    [Label("Simulated", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingSimulated : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSimulated(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].simulated = value;
            }
        }
    }

    [Serializable]
    [Label("Sleep Mode", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingSleepMode : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSleepMode(RigidbodySleepMode2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sleepMode = value;
            }
        }
    }

    [Serializable]
    [Label("Total Force", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingTotalForce : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTotalForce(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].totalForce = value;
            }
        }
    }

    [Serializable]
    [Label("Total Torque", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingTotalTorque : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTotalTorque(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].totalTorque = value;
            }
        }
    }

    [Serializable]
    [Label("Use Auto Mass", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingUseAutoMass : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseAutoMass(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useAutoMass = value;
            }
        }
    }

    [Serializable]
    [Label("Use Full Kinematic Contacts", "Rigidbody 2D")]
    public sealed partial class Rigidbody2DBindingUseFullKinematicContacts : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseFullKinematicContacts(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useFullKinematicContacts = value;
            }
        }
    }

    [Serializable]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use Rigidbody2DBindingLinearVelocity instead")]
#else
    [Label("Velocity", "Rigidbody 2D")]
#endif
    public sealed partial class Rigidbody2DBindingVelocity : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetVelocity(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].velocity = value;
            }
        }
    }

#if UNITY_6000_0_OR_NEWER
    [Serializable]
    [Label("Linear Velocity", "Rigidbody 2D")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true
        , "EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D"
        , "EncosyTower.Modules.Mvvm.Unity"
        , "Rigidbody2DBindingVelocity"
    )]
    public sealed partial class Rigidbody2DBindingLinearVelocity : MonoBindingProperty<Rigidbody2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLinearVelocity(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].linearVelocity = value;
            }
        }
    }
#endif
}
