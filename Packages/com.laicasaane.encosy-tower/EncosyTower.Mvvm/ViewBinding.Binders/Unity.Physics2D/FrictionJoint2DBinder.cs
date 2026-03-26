#if UNITY_PHYSICS_2D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable, Binder]
    [Label("Friction Joint 2D", "Physics 2D")]
    public sealed partial class FrictionJoint2DBinder : MonoBinder<FrictionJoint2D>
    {
    }

    [Serializable, Binder]
    [Label("Max Force", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingMaxForce : MonoBindingProperty<FrictionJoint2D>
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

    [Serializable, Binder]
    [Label("Max Torque", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingMaxTorque : MonoBindingProperty<FrictionJoint2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxTorque(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxTorque = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Anchor", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingAnchor : MonoBindingProperty<FrictionJoint2D>
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
    [Label("Auto Configure Connected Anchor", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<FrictionJoint2D>
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
    [Label("Connected Anchor", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingConnectedAnchor : MonoBindingProperty<FrictionJoint2D>
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
    [Label("Break Action", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingBreakAction : MonoBindingProperty<FrictionJoint2D>
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
    [Label("Break Force", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingBreakForce : MonoBindingProperty<FrictionJoint2D>
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
    [Label("Break Torque", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingBreakTorque : MonoBindingProperty<FrictionJoint2D>
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
    [Label("Connected Body", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingConnectedBody : MonoBindingProperty<FrictionJoint2D>
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
    [Label("Enable Collision", "Friction Joint 2D")]
    public sealed partial class FrictionJoint2DBindingEnableCollision : MonoBindingProperty<FrictionJoint2D>
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
