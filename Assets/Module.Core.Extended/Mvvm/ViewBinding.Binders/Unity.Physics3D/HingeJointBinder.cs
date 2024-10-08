using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Hinge Joint", "Physics 3D")]
    public sealed partial class HingeJointBinder : MonoBinder<HingeJoint>
    {
    }

    [Serializable]
    [Label("Extended Limits", "Hinge Joint")]
    public sealed partial class HingeJointBindingExtendedLimits : MonoBindingProperty<HingeJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetExtendedLimits(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].extendedLimits = value;
            }
        }
    }

    [Serializable]
    [Label("Limits", "Hinge Joint")]
    public sealed partial class HingeJointBindingLimits : MonoBindingProperty<HingeJoint>, IBinder
    {
#if !UNION_SIZE_32_BYTES
        public HingeJointBindingLimits()
        {
            Logging.DevLoggerAPI.LogException(new NotSupportedException(
                "Hinge Joint Limits binding property requires the symbol UNION_SIZE_32_BYTES to be defined"
            ));
        }
#else
        [BindingProperty]
        [field: HideInInspector]
        private void SetLimits(in JointLimits value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].limits = value;
            }
        }
#endif
    }

    [Serializable]
    [Label("Motor", "Hinge Joint")]
    public sealed partial class HingeJointBindingMotor : MonoBindingProperty<HingeJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMotor(in JointMotor value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].motor = value;
            }
        }
    }

    [Serializable]
    [Label("Spring", "Hinge Joint")]
    public sealed partial class HingeJointBindingSpring : MonoBindingProperty<HingeJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpring(in JointSpring value)
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
    [Label("Use Acceleration", "Hinge Joint")]
    public sealed partial class HingeJointBindingUseAcceleration : MonoBindingProperty<HingeJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseAcceleration(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useAcceleration = value;
            }
        }
    }

    [Serializable]
    [Label("Use Limits", "Hinge Joint")]
    public sealed partial class HingeJointBindingUseLimits : MonoBindingProperty<HingeJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseLimits(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useLimits = value;
            }
        }
    }

    [Serializable]
    [Label("Use Motor", "Hinge Joint")]
    public sealed partial class HingeJointBindingUseMotor : MonoBindingProperty<HingeJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseMotor(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useMotor = value;
            }
        }
    }

    [Serializable]
    [Label("Use Spring", "Hinge Joint")]
    public sealed partial class HingeJointBindingUseSpring : MonoBindingProperty<HingeJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseSpring(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useSpring = value;
            }
        }
    }
}
