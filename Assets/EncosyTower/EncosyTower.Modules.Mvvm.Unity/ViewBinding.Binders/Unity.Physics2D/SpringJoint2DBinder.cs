using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Spring Joint 2D", "Physics 2D")]
    public sealed partial class SpringJoint2DBinder : MonoBinder<SpringJoint2D>
    {
    }

    [Serializable]
    [Label("Auto Configure Distance", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingAutoConfigureDistance : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Damping Ratio", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingDampingRatio : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Distance", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingDistance : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Frequency", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingFrequency : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Anchor", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingAnchor : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Auto Configure Connected Anchor", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Connected Anchor", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingConnectedAnchor : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Break Action", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingBreakAction : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Break Force", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingBreakForce : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Break Torque", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingBreakTorque : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Connected Body", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingConnectedBody : MonoBindingProperty<SpringJoint2D>, IBinder
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
    [Label("Enable Collision", "Spring Joint 2D")]
    public sealed partial class SpringJoint2DBindingEnableCollision : MonoBindingProperty<SpringJoint2D>, IBinder
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

