using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Target Joint 2D", "Physics 2D")]
    public sealed partial class TargetJoint2DBinder : MonoBinder<TargetJoint2D>
    {
    }

    [Serializable]
    [Label("Anchor", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingAnchor : MonoBindingProperty<TargetJoint2D>, IBinder
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
    [Label("Auto Configure Target", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingAutoConfigureTarget : MonoBindingProperty<TargetJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutoConfigureTarget(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].autoConfigureTarget = value;
            }
        }
    }

    [Serializable]
    [Label("Damping Ratio", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingDampingRatio : MonoBindingProperty<TargetJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDampingRatio(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].dampingRatio = value;
            }
        }
    }

    [Serializable]
    [Label("Frequency", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingFrequency : MonoBindingProperty<TargetJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFrequency(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].frequency = value;
            }
        }
    }

    [Serializable]
    [Label("Max Force", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingMaxForce : MonoBindingProperty<TargetJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxForce(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxForce = value;
            }
        }
    }

    [Serializable]
    [Label("Target", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingTarget : MonoBindingProperty<TargetJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTarget(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].target = value;
            }
        }
    }

    [Serializable]
    [Label("Break Action", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingBreakAction : MonoBindingProperty<TargetJoint2D>, IBinder
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
    [Label("Break Force", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingBreakForce : MonoBindingProperty<TargetJoint2D>, IBinder
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
    [Label("Break Torque", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingBreakTorque : MonoBindingProperty<TargetJoint2D>, IBinder
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
    [Label("Connected Body", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingConnectedBody : MonoBindingProperty<TargetJoint2D>, IBinder
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
    [Label("Enable Collision", "Target Joint 2D")]
    public sealed partial class TargetJoint2DBindingEnableCollision : MonoBindingProperty<TargetJoint2D>, IBinder
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

