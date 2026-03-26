#if UNITY_PHYSICS_3D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable, Binder]
    [Label("Sphere Collider", "Physics 3D")]
    public sealed partial class SphereColliderBinder : MonoBinder<SphereCollider>
    {
    }

    [Serializable, Binder]
    [Label("Center", "Sphere Collider")]
    public sealed partial class SphereColliderBindingCenter : MonoBindingProperty<SphereCollider>
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
    [Label("Radius", "Sphere Collider")]
    public sealed partial class SphereColliderBindingRadius : MonoBindingProperty<SphereCollider>
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
    [Label("Is Trigger", "Sphere Collider")]
    public sealed partial class SphereColliderBindingIsTrigger : MonoBindingProperty<SphereCollider>
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
