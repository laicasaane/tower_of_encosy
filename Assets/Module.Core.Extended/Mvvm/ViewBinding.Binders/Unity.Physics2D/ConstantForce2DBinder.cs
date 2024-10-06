using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Constant Force 2D", "Physics 2D")]
    public sealed partial class ConstantForce2DBinder : MonoBinder<ConstantForce2D>
    {
    }

    [Serializable]
    [Label("Force", "Constant Force 2D")]
    public sealed partial class ConstantForce2DBindingForce : MonoBindingProperty<ConstantForce2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForce(Vector2 value)
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
    [Label("Relative Force", "Constant Force 2D")]
    public sealed partial class ConstantForce2DBindingRelativeForce : MonoBindingProperty<ConstantForce2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRelativeForce(Vector2 value)
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
    [Label("Torque", "Constant Force 2D")]
    public sealed partial class ConstantForce2DBindingTorque : MonoBindingProperty<ConstantForce2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTorque(float value)
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
