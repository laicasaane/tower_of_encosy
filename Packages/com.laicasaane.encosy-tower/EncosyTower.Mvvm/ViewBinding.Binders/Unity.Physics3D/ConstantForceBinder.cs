#if UNITY_PHYSICS_3D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable, Binder]
    [Label("Constant Force", "Physics 3D")]
    public sealed partial class ConstantForceBinder : MonoBinder<ConstantForce>
    {
    }

    [Serializable, Binder]
    [Label("Force", "Constant Force")]
    public sealed partial class ConstantForceBindingForce : MonoBindingProperty<ConstantForce>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForce(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].force = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Relative Force", "Constant Force")]
    public sealed partial class ConstantForceBindingRelativeForce : MonoBindingProperty<ConstantForce>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRelativeForce(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].relativeForce = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Relative Torque", "Constant Force")]
    public sealed partial class ConstantForceBindingRelativeTorque : MonoBindingProperty<ConstantForce>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRelativeTorque(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].relativeTorque = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Torque", "Constant Force")]
    public sealed partial class ConstantForceBindingTorque : MonoBindingProperty<ConstantForce>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTorque(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].torque = value;
            }
        }
    }
}

#endif
