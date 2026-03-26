#if UNITY_PHYSICS_3D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable, Binder]
    [Label("Character Controller", "Physics 3D")]
    public sealed partial class CharacterControllerBinder : MonoBinder<CharacterController>
    {
    }

    [Serializable, Binder]
    [Label("Center", "Character Controller")]
    public sealed partial class CharacterControllerBindingCenter : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCenter(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].center = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Detect Collisions", "Character Controller")]
    public sealed partial class CharacterControllerBindingDetectCollisions : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDetectCollisions(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].detectCollisions = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Enable Overlap Recovery", "Character Controller")]
    public sealed partial class CharacterControllerBindingEnableOverlapRecovery : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetEnableOverlapRecovery(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].enableOverlapRecovery = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Height", "Character Controller")]
    public sealed partial class CharacterControllerBindingHeight : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetHeight(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].height = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Min Move Distance", "Character Controller")]
    public sealed partial class CharacterControllerBindingMinMoveDistance : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMinMoveDistance(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].minMoveDistance = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Radius", "Character Controller")]
    public sealed partial class CharacterControllerBindingRadius : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRadius(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].radius = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Skin Width", "Character Controller")]
    public sealed partial class CharacterControllerBindingSkinWidth : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSkinWidth(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].skinWidth = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Slope Limit", "Character Controller")]
    public sealed partial class CharacterControllerBindingSlopeLimit : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSlopeLimit(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].slopeLimit = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Step Offset", "Character Controller")]
    public sealed partial class CharacterControllerBindingStepOffset : MonoBindingProperty<CharacterController>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetStepOffset(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].stepOffset = value;
            }
        }
    }
}

#endif
