#if UNITY_PHYSICS_2D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable, Binder]
    [Label("Wheel Joint 2D", "Physics 2D")]
    public sealed partial class WheelJoint2DBinder : MonoBinder<WheelJoint2D>
    {
    }

    [Serializable, Binder]
    [Label("Motor", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingMotor : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Suspension", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingSuspension : MonoBindingProperty<WheelJoint2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSuspension(in JointSuspension2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].suspension = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Use Motor", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingUseMotor : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Anchor", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingAnchor : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Auto Configure Connected Anchor", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Connected Anchor", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingConnectedAnchor : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Break Action", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingBreakAction : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Break Force", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingBreakForce : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Break Torque", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingBreakTorque : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Connected Body", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingConnectedBody : MonoBindingProperty<WheelJoint2D>
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

    [Serializable, Binder]
    [Label("Enable Collision", "Wheel Joint 2D")]
    public sealed partial class WheelJoint2DBindingEnableCollision : MonoBindingProperty<WheelJoint2D>
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

#endif
