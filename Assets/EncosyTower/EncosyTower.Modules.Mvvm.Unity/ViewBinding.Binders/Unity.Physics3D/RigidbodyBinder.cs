using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Rigidbody", "Physics 3D")]
    public sealed partial class RigidbodyBinder : MonoBinder<Rigidbody>
    {
    }

    [Serializable]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use RigidbodyBindingAngularDamping instead")]
#else
    [Label("Angular Drag", "Rigidbody")]
#endif
    public sealed partial class RigidbodyBindingAngularDrag : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Angular Damping", "Rigidbody")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true
        , "EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics3D"
        , "EncosyTower.Modules.Mvvm.Unity"
        , "RigidbodyBindingAngularDrag"
    )]
    public sealed partial class RigidbodyBindingAngularDamping : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Angular Velocity", "Rigidbody")]
    public sealed partial class RigidbodyBindingAngularVelocity : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAngularVelocity(in Vector3 value)
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
    [Label("Automatic Center Of Mass", "Rigidbody")]
    public sealed partial class RigidbodyBindingAutomaticCenterOfMass : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutomaticCenterOfMass(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].automaticCenterOfMass = value;
            }
        }
    }

    [Serializable]
    [Label("Automatic Inertia Tensor", "Rigidbody")]
    public sealed partial class RigidbodyBindingAutomaticInertiaTensor : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutomaticInertiaTensor(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].automaticInertiaTensor = value;
            }
        }
    }

    [Serializable]
    [Label("Center Of Mass", "Rigidbody")]
    public sealed partial class RigidbodyBindingCenterOfMass : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCenterOfMass(in Vector3 value)
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
    [Label("Collision Detection Mode", "Rigidbody")]
    public sealed partial class RigidbodyBindingCollisionDetectionMode : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCollisionDetectionMode(CollisionDetectionMode value)
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
    [Label("Constraints", "Rigidbody")]
    public sealed partial class RigidbodyBindingConstraints : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConstraints(RigidbodyConstraints value)
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
    [Label("Detect Collisions", "Rigidbody")]
    public sealed partial class RigidbodyBindingDetectCollisions : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDetectCollisions(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].detectCollisions = value;
            }
        }
    }

    [Serializable]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use RigidbodyBindingLinearDamping instead")]
#else
    [Label("Drag", "Rigidbody")]
#endif
    public sealed partial class RigidbodyBindingDrag : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Linear Damping", "Rigidbody")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true
        , "EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics3D"
        , "EncosyTower.Modules.Mvvm.Unity"
        , "RigidbodyBindingDrag"
    )]
    public sealed partial class RigidbodyBindingLinearDamping : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Exclude Layers", "Rigidbody")]
    public sealed partial class RigidbodyBindingExcludeLayers : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Freeze Rotation", "Rigidbody")]
    public sealed partial class RigidbodyBindingFreezeRotation : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Include Layers", "Rigidbody")]
    public sealed partial class RigidbodyBindingIncludeLayers : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Inertia Tensor", "Rigidbody")]
    public sealed partial class RigidbodyBindingInertiaTensor : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInertiaTensor(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].inertiaTensor = value;
            }
        }
    }

    [Serializable]
    [Label("Inertia Tensor Rotation", "Rigidbody")]
    public sealed partial class RigidbodyBindingInertiaTensorRotation : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInertiaTensorRotation(in Quaternion value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].inertiaTensorRotation = value;
            }
        }
    }

    [Serializable]
    [Label("Interpolation", "Rigidbody")]
    public sealed partial class RigidbodyBindingInterpolation : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInterpolation(RigidbodyInterpolation value)
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
    [Label("Is Kinematic", "Rigidbody")]
    public sealed partial class RigidbodyBindingIsKinematic : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Mass", "Rigidbody")]
    public sealed partial class RigidbodyBindingMass : MonoBindingProperty<Rigidbody>, IBinder
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
    [Label("Max Angular Velocity", "Rigidbody")]
    public sealed partial class RigidbodyBindingMaxAngularVelocity : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxAngularVelocity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxAngularVelocity = value;
            }
        }
    }

    [Serializable]
    [Label("Max Depenetration Velocity", "Rigidbody")]
    public sealed partial class RigidbodyBindingMaxDepenetrationVelocity : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxDepenetrationVelocity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxDepenetrationVelocity = value;
            }
        }
    }

    [Serializable]
    [Label("Max Linear Velocity", "Rigidbody")]
    public sealed partial class RigidbodyBindingMaxLinearVelocity : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxLinearVelocity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxLinearVelocity = value;
            }
        }
    }

    [Serializable]
    [Label("Position", "Rigidbody")]
    public sealed partial class RigidbodyBindingPosition : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPosition(in Vector3 value)
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
    [Label("Rotation", "Rigidbody")]
    public sealed partial class RigidbodyBindingRotation : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRotation(in Quaternion value)
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
    [Label("Sleep Threshold", "Rigidbody")]
    public sealed partial class RigidbodyBindingSleepThreshold : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSleepThreshold(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sleepThreshold = value;
            }
        }
    }

    [Serializable]
    [Label("Solver Iterations", "Rigidbody")]
    public sealed partial class RigidbodyBindingSolverIterations : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSolverIterations(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].solverIterations = value;
            }
        }
    }

    [Serializable]
    [Label("Solver Velocity Iterations", "Rigidbody")]
    public sealed partial class RigidbodyBindingSolverVelocityIterations : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSolverVelocityIterations(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].solverVelocityIterations = value;
            }
        }
    }

    [Serializable]
    [Label("Use Gravity", "Rigidbody")]
    public sealed partial class RigidbodyBindingUseGravity : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseGravity(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useGravity = value;
            }
        }
    }

    [Serializable]
#if UNITY_6000_0_OR_NEWER
    [Obsolete("Use RigidbodyBindingLinearVelocity instead")]
#else
    [Label("Velocity", "Rigidbody")]
#endif
    public sealed partial class RigidbodyBindingVelocity : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetVelocity(in Vector3 value)
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
    [Label("Linear Velocity", "Rigidbody")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true
        , "EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics3D"
        , "EncosyTower.Modules.Mvvm.Unity"
        , "RigidbodyBindingVelocity"
    )]
    public sealed partial class RigidbodyBindingLinearVelocity : MonoBindingProperty<Rigidbody>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLinearVelocity(in Vector3 value)
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
