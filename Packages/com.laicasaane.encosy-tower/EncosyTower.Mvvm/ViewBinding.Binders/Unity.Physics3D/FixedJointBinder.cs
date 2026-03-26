#if UNITY_PHYSICS_3D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable, Binder]
    [Label("Fixed Joint", "Physics 3D")]
    public sealed partial class FixedJointBinder : MonoBinder<FixedJoint>
    {
    }

    [Serializable, Binder]
    [Label("Tag", "Fixed Joint")]
    public sealed partial class FixedJointBindingTag : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Anchor", "Fixed Joint")]
    public sealed partial class FixedJointBindingAnchor : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Auto Configure Connected Anchor", "Fixed Joint")]
    public sealed partial class FixedJointBindingAutoConfigureConnectedAnchor : MonoBindingProperty<FixedJoint>
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
    [Label("Axis", "Fixed Joint")]
    public sealed partial class FixedJointBindingAxis : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Break Force", "Fixed Joint")]
    public sealed partial class FixedJointBindingBreakForce : MonoBindingProperty<FixedJoint>
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
    [Label("Break Torque", "Fixed Joint")]
    public sealed partial class FixedJointBindingBreakTorque : MonoBindingProperty<FixedJoint>
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
    [Label("Connected Anchor", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedAnchor : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Connected Articulation Body", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedArticulationBody : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Connected Body", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedBody : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Connected Mass Scale", "Fixed Joint")]
    public sealed partial class FixedJointBindingConnectedMassScale : MonoBindingProperty<FixedJoint>
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


    [Serializable, Binder]
    [Label("Enable Collision", "Fixed Joint")]
    public sealed partial class FixedJointBindingEnableCollision : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Enable Preprocessing", "Fixed Joint")]
    public sealed partial class FixedJointBindingEnablePreprocessing : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Mass Scale", "Fixed Joint")]
    public sealed partial class FixedJointBindingMassScale : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Hide Flags", "Fixed Joint")]
    public sealed partial class FixedJointBindingHideFlags : MonoBindingProperty<FixedJoint>
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

    [Serializable, Binder]
    [Label("Name", "Fixed Joint")]
    public sealed partial class FixedJointBindingName : MonoBindingProperty<FixedJoint>
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

#endif
