using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Slider Joint 2D", "Physics 2D")]
    public sealed partial class SliderJoint2DBinder : MonoBinder<SliderJoint2D>
    {
    }

    [Serializable]
    [Label("Angle", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingAngle : MonoBindingProperty<SliderJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAngle(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].angle = value;
            }
        }
    }

    [Serializable]
    [Label("Auto Configure Angle", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingAutoConfigureAngle : MonoBindingProperty<SliderJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutoConfigureAngle(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].autoConfigureAngle = value;
            }
        }
    }

    [Serializable]
    [Label("Limits", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingLimits : MonoBindingProperty<SliderJoint2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLimits(JointTranslationLimits2D value)
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
    [Label("Motor", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingMotor : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Use Limits", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingUseLimits : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Use Motor", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingUseMotor : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Anchor", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingAnchor : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Auto Configure Connected Anchor", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingAutoConfigureConnectedAnchor : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Connected Anchor", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingConnectedAnchor : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Break Action", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingBreakAction : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Break Force", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingBreakForce : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Break Torque", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingBreakTorque : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Connected Body", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingConnectedBody : MonoBindingProperty<SliderJoint2D>, IBinder
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
    [Label("Enable Collision", "Slider Joint 2D")]
    public sealed partial class SliderJoint2DBindingEnableCollision : MonoBindingProperty<SliderJoint2D>, IBinder
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

