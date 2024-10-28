using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Distance Joint 2D", "Physics 2D")]
    public sealed partial class DistanceJoint2DBinder : MonoBinder<DistanceJoint2D>
    {
    }

    [Serializable]
    [Label("Auto Configure Distance", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingAutoConfigureDistance : MonoBindingProperty<DistanceJoint2D>, IBinder
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

    [Serializable]
    [Label("Distance", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingDistance : MonoBindingProperty<DistanceJoint2D>, IBinder
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

    [Serializable]
    [Label("Max Distance Only", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingMaxDistanceOnly : MonoBindingProperty<DistanceJoint2D>, IBinder
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

    [Serializable]
    [Label("Anchor", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingAnchor : MonoBindingProperty<DistanceJoint2D>, IBinder
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
    [Label("Auto Configure Connected Anchor", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<DistanceJoint2D>, IBinder
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
    [Label("Connected Anchor", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingConnectedAnchor : MonoBindingProperty<DistanceJoint2D>, IBinder
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
    [Label("Break Action", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingBreakAction : MonoBindingProperty<DistanceJoint2D>, IBinder
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
    [Label("Break Force", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingBreakForce : MonoBindingProperty<DistanceJoint2D>, IBinder
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
    [Label("Break Torque", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingBreakTorque : MonoBindingProperty<DistanceJoint2D>, IBinder
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
    [Label("Connected Body", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingConnectedBody : MonoBindingProperty<DistanceJoint2D>, IBinder
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
    [Label("Enable Collision", "Distance Joint 2D")]
    public sealed partial class DistanceJoint2DBindingEnableCollision : MonoBindingProperty<DistanceJoint2D>, IBinder
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

