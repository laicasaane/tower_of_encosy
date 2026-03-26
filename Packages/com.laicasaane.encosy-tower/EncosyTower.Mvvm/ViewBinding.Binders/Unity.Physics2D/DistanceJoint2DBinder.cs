#if UNITY_PHYSICS_2D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable, Binder]
    [Label("Distance Joint 2D", "Physics 2D")]
    public sealed partial class DistanceJoint2DBinder : MonoBinder<DistanceJoint2D>
    {
    }

    [Serializable, Binder]
    [Label("Auto Configure Distance", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingAutoConfigureDistance : MonoBindingProperty<DistanceJoint2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutoConfigureDistance(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].autoConfigureDistance = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Distance", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingDistance : MonoBindingProperty<DistanceJoint2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDistance(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].distance = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Max Distance Only", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingMaxDistanceOnly : MonoBindingProperty<DistanceJoint2D>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxDistanceOnly(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxDistanceOnly = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Anchor", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingAnchor : MonoBindingProperty<DistanceJoint2D>
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
    [Label("Auto Configure Connected Anchor", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<DistanceJoint2D>
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
    [Label("Connected Anchor", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingConnectedAnchor : MonoBindingProperty<DistanceJoint2D>
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
    [Label("Break Action", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingBreakAction : MonoBindingProperty<DistanceJoint2D>
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
    [Label("Break Force", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingBreakForce : MonoBindingProperty<DistanceJoint2D>
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
    [Label("Break Torque", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingBreakTorque : MonoBindingProperty<DistanceJoint2D>
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
    [Label("Connected Body", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingConnectedBody : MonoBindingProperty<DistanceJoint2D>
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
    [Label("Enable Collision", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingEnableCollision : MonoBindingProperty<DistanceJoint2D>
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
