using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Spring Joint", "Physics 3D")]
    public sealed partial class SpringJointBinder : MonoBinder<SpringJoint>
    {
    }

    [Serializable]
    [Label("Damper", "Spring Joint")]
    public sealed partial class SpringJointBindingDamper : MonoBindingProperty<SpringJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDamper(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].damper = value;
            }
        }
    }

    [Serializable]
    [Label("Max Distance", "Spring Joint")]
    public sealed partial class SpringJointBindingMaxDistance : MonoBindingProperty<SpringJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxDistance(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxDistance = value;
            }
        }
    }

    [Serializable]
    [Label("Min Distance", "Spring Joint")]
    public sealed partial class SpringJointBindingMinDistance : MonoBindingProperty<SpringJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMinDistance(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].minDistance = value;
            }
        }
    }

    [Serializable]
    [Label("Spring", "Spring Joint")]
    public sealed partial class SpringJointBindingSpring : MonoBindingProperty<SpringJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpring(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].spring = value;
            }
        }
    }

    [Serializable]
    [Label("Tolerance", "Spring Joint")]
    public sealed partial class SpringJointBindingTolerance : MonoBindingProperty<SpringJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTolerance(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].tolerance = value;
            }
        }
    }
}
