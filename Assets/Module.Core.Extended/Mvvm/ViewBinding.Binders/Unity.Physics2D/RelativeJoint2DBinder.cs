using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Relative Joint 2D", "Physics 2D")]
    public sealed partial class RelativeJoint2DBinder : MonoBinder<RelativeJoint2D>
    {
    }

    [Serializable]
    [Label("Angular Offset", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingAngularOffset : MonoBindingProperty<RelativeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAngularOffset(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].angularOffset = value;
            }
        }
    }

    [Serializable]
    [Label("Auto Configure Offset", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingAutoConfigureOffset : MonoBindingProperty<RelativeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutoConfigureOffset(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].autoConfigureOffset = value;
            }
        }
    }

    [Serializable]
    [Label("Correction Scale", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingCorrectionScale : MonoBindingProperty<RelativeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCorrectionScale(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].correctionScale = value;
            }
        }
    }

    [Serializable]
    [Label("Linear Offset", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingLinearOffset : MonoBindingProperty<RelativeJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLinearOffset(Vector2 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].linearOffset = value;
            }
        }
    }

    [Serializable]
    [Label("Max Force", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingMaxForce : MonoBindingProperty<RelativeJoint2D>, IBinder
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
    [Label("Max Torque", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingMaxTorque : MonoBindingProperty<RelativeJoint2D>, IBinder
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

    [Serializable]
    [Label("Break Action", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingBreakAction : MonoBindingProperty<RelativeJoint2D>, IBinder
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
    [Label("Break Force", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingBreakForce : MonoBindingProperty<RelativeJoint2D>, IBinder
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
    [Label("Break Torque", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingBreakTorque : MonoBindingProperty<RelativeJoint2D>, IBinder
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
    [Label("Connected Body", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingConnectedBody : MonoBindingProperty<RelativeJoint2D>, IBinder
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
    [Label("Enable Collision", "Relative Joint 2D")]
    public sealed partial class RelativeJoint2DBindingEnableCollision : MonoBindingProperty<RelativeJoint2D>, IBinder
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

