using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Character Controller", "Physics 3D")]
    public sealed partial class CharacterControllerBinder : MonoBinder<CharacterController>
    {
    }

    [Serializable]
    [Label("Center", "Character Controller")]
    public sealed partial class CharacterControllerBindingCenter : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Detect Collisions", "Character Controller")]
    public sealed partial class CharacterControllerBindingDetectCollisions : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Enable Overlap Recovery", "Character Controller")]
    public sealed partial class CharacterControllerBindingEnableOverlapRecovery : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Height", "Character Controller")]
    public sealed partial class CharacterControllerBindingHeight : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Min Move Distance", "Character Controller")]
    public sealed partial class CharacterControllerBindingMinMoveDistance : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Radius", "Character Controller")]
    public sealed partial class CharacterControllerBindingRadius : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Skin Width", "Character Controller")]
    public sealed partial class CharacterControllerBindingSkinWidth : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Slope Limit", "Character Controller")]
    public sealed partial class CharacterControllerBindingSlopeLimit : MonoBindingProperty<CharacterController>, IBinder
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

    [Serializable]
    [Label("Step Offset", "Character Controller")]
    public sealed partial class CharacterControllerBindingStepOffset : MonoBindingProperty<CharacterController>, IBinder
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
