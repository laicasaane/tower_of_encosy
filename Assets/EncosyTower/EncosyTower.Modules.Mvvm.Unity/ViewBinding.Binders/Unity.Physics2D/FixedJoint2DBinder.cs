using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Fixed Joint 2D", "Physics 2D")]
    public sealed partial class FixedJoint2DBinder : MonoBinder<FixedJoint2D>
    {
    }

    [Serializable]
    [Label("Damping Ratio", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingDampingRatio : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Frequency", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingFrequency : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Anchor", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingAnchor : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Auto Configure Connected Anchor", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Connected Anchor", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingConnectedAnchor : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Break Action", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingBreakAction : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Break Force", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingBreakForce : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Break Torque", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingBreakTorque : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Connected Body", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingConnectedBody : MonoBindingProperty<FixedJoint2D>, IBinder
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
    [Label("Enable Collision", "Fixed Joint 2D")]
    public sealed partial class FixedJoint2DBindingEnableCollision : MonoBindingProperty<FixedJoint2D>, IBinder
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

