#if UNITY_PHYSICS_3D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable, Binder]
    [Label("Spring Joint", "Physics 3D")]
    public sealed partial class SpringJointBinder : MonoBinder<SpringJoint>
    {
    }

    [Serializable, Binder]
    [Label("Damper", "Spring Joint")]
    public sealed partial class SpringJointBindingDamper : MonoBindingProperty<SpringJoint>
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

    [Serializable, Binder]
    [Label("Max Distance", "Spring Joint")]
    public sealed partial class SpringJointBindingMaxDistance : MonoBindingProperty<SpringJoint>
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

    [Serializable, Binder]
    [Label("Min Distance", "Spring Joint")]
    public sealed partial class SpringJointBindingMinDistance : MonoBindingProperty<SpringJoint>
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

    [Serializable, Binder]
    [Label("Spring", "Spring Joint")]
    public sealed partial class SpringJointBindingSpring : MonoBindingProperty<SpringJoint>
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

    [Serializable, Binder]
    [Label("Tolerance", "Spring Joint")]
    public sealed partial class SpringJointBindingTolerance : MonoBindingProperty<SpringJoint>
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

#endif
