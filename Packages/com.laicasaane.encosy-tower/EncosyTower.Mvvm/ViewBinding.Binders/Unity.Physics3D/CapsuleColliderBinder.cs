#if UNITY_PHYSICS_3D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable, Binder]
    [Label("Capsule Collider", "Physics 3D")]
    public sealed partial class CapsuleColliderBinder : MonoBinder<CapsuleCollider>
    {
    }

    [Serializable, Binder]
    [Label("Center", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingCenter : MonoBindingProperty<CapsuleCollider>
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
    [Label("Direction", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingDirection : MonoBindingProperty<CapsuleCollider>
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

    [Serializable, Binder]
    [Label("Height", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingHeight : MonoBindingProperty<CapsuleCollider>
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
    [Label("Radius", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingRadius : MonoBindingProperty<CapsuleCollider>
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
    [Label("Is Trigger", "Capsule Collider")]
    public sealed partial class CapsuleColliderBindingIsTrigger : MonoBindingProperty<CapsuleCollider>
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

#endif
