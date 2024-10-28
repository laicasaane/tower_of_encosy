using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Capsule Collider", "Physics 3D")]
    public sealed partial class CapsuleColliderBinder : MonoBinder<CapsuleCollider>
    {
    }

    [Serializable]
    [Label("Center", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingCenter : MonoBindingProperty<CapsuleCollider>, IBinder
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
    [Label("Direction", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingDirection : MonoBindingProperty<CapsuleCollider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDirection(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].direction = value;
            }
        }
    }

    [Serializable]
    [Label("Height", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingHeight : MonoBindingProperty<CapsuleCollider>, IBinder
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
    [Label("Radius", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingRadius : MonoBindingProperty<CapsuleCollider>, IBinder
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
    [Label("Is Trigger", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingIsTrigger : MonoBindingProperty<CapsuleCollider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetIsTrigger(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].isTrigger = value;
            }
        }
    }
}
