using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Constant Force", "Physics 3D")]
    public sealed partial class ConstantForceBinder : MonoBinder<ConstantForce>
    {
    }

    [Serializable]
    [Label("Force", "Constant Force")]
    public sealed partial class ConstantForceBindingForce : MonoBindingProperty<ConstantForce>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForce(Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].force = value;
            }
        }
    }

    [Serializable]
    [Label("Relative Force", "Constant Force")]
    public sealed partial class ConstantForceBindingRelativeForce : MonoBindingProperty<ConstantForce>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRelativeForce(Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].relativeForce = value;
            }
        }
    }

    [Serializable]
    [Label("Relative Torque", "Constant Force")]
    public sealed partial class ConstantForceBindingRelativeTorque : MonoBindingProperty<ConstantForce>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRelativeTorque(Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].relativeTorque = value;
            }
        }
    }

    [Serializable]
    [Label("Torque", "Constant Force")]
    public sealed partial class ConstantForceBindingTorque : MonoBindingProperty<ConstantForce>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTorque(Vector3 value)
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