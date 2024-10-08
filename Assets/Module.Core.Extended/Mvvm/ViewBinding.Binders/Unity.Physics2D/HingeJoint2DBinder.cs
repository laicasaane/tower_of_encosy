using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Hinge Joint 2D", "Physics 2D")]
    public sealed partial class HingeJoint2DBinder : MonoBinder<HingeJoint2D>
    {
    }

    [Serializable]
    [Label("Limits", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingLimits : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLimits(JointAngleLimits2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].limits = value;
            }
        }
    }

    [Serializable]
    [Label("Motor", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingMotor : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMotor(JointMotor2D value)
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
    [Label("Use Limits", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingUseLimits : MonoBindingProperty<HingeJoint2D>, IBinder
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
    [Label("Use Motor", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingUseMotor : MonoBindingProperty<HingeJoint2D>, IBinder
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
    [Label("Anchor", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingAnchor : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAnchor(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].anchor = value;
            }
        }
    }

    [Serializable]
    [Label("Auto Configure Connected Anchor", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutoConfigureConnectedAnchor(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].autoConfigureConnectedAnchor = value;
            }
        }
    }

    [Serializable]
    [Label("Connected Anchor", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingConnectedAnchor : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConnectedAnchor(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].connectedAnchor = value;
            }
        }
    }

    [Serializable]
    [Label("Break Action", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingBreakAction : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetBreakAction(JointBreakAction2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].breakAction = value;
            }
        }
    }

    [Serializable]
    [Label("Break Force", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingBreakForce : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetBreakForce(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].breakForce = value;
            }
        }
    }

    [Serializable]
    [Label("Break Torque", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingBreakTorque : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetBreakTorque(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].breakTorque = value;
            }
        }
    }

    [Serializable]
    [Label("Connected Body", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingConnectedBody : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConnectedBody(Rigidbody2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].connectedBody = value;
            }
        }
    }

    [Serializable]
    [Label("Enable Collision", "Hinge Joint 2D")]
    public sealed partial class HingeJoint2DBindingEnableCollision : MonoBindingProperty<HingeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetEnableCollision(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].enableCollision = value;
            }
        }
    }
}

