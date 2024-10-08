using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Fixed Joint", "Physics 3D")]
    public sealed partial class FixedJointBinder : MonoBinder<FixedJoint>
    {
    }

    [Serializable]
    [Label("Tag", "Fixed Joint")]
    public sealed partial class FixedJointBindingTag : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTag(string value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].tag = value;
            }
        }
    }

    [Serializable]
    [Label("Anchor", "Fixed Joint")]
    public sealed partial class FixedJointBindingAnchor : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAnchor(in Vector3 value)
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
    [Label("Auto Configure Connected Anchor", "Fixed Joint")]
    public sealed partial class FixedJointBindingAutoConfigureConnectedAnchor : MonoBindingProperty<FixedJoint>, IBinder
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
    [Label("Axis", "Fixed Joint")]
    public sealed partial class FixedJointBindingAxis : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAxis(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].axis = value;
            }
        }
    }

    [Serializable]
    [Label("Break Force", "Fixed Joint")]
    public sealed partial class FixedJointBindingBreakForce : MonoBindingProperty<FixedJoint>, IBinder
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
    [Label("Break Torque", "Fixed Joint")]
    public sealed partial class FixedJointBindingBreakTorque : MonoBindingProperty<FixedJoint>, IBinder
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
    [Label("Connected Anchor", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedAnchor : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConnectedAnchor(in Vector3 value)
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
    [Label("Connected Articulation Body", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedArticulationBody : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConnectedArticulationBody(ArticulationBody value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].connectedArticulationBody = value;
            }
        }
    }

    [Serializable]
    [Label("Connected Body", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedBody : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConnectedBody(Rigidbody value)
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
    [Label("Connected Mass Scale", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedMassScale : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConnectedMassScale(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].connectedMassScale = value;
            }
        }
    }


    [Serializable]
    [Label("Enable Collision", "Fixed Joint")]
    public sealed partial class FixedJointBindingEnableCollision : MonoBindingProperty<FixedJoint>, IBinder
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

    [Serializable]
    [Label("Enable Preprocessing", "Fixed Joint")]
    public sealed partial class FixedJointBindingEnablePreprocessing : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetEnablePreprocessing(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].enablePreprocessing = value;
            }
        }
    }

    [Serializable]
    [Label("Mass Scale", "Fixed Joint")]
    public sealed partial class FixedJointBindingMassScale : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMassScale(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].massScale = value;
            }
        }
    }

    [Serializable]
    [Label("Hide Flags", "Fixed Joint")]
    public sealed partial class FixedJointBindingHideFlags : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetHideFlags(HideFlags value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].hideFlags = value;
            }
        }
    }

    [Serializable]
    [Label("Name", "Fixed Joint")]
    public sealed partial class FixedJointBindingName : MonoBindingProperty<FixedJoint>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetName(string value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].name = value;
            }
        }
    }
}
