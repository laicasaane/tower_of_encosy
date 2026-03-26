#if UNITY_PHYSICS_3D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable, Binder]
    [Label("Box Collider", "Physics 3D")]
    public sealed partial class BoxColliderBinder : MonoBinder<BoxCollider>
    {
    }

    [Serializable, Binder]
    [Label("Center", "Box Collider")]
    public sealed partial class BoxColliderBindingCenter : MonoBindingProperty<BoxCollider>
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
    [Label("Size", "Box Collider")]
    public sealed partial class BoxColliderBindingSize : MonoBindingProperty<BoxCollider>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSize(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].size = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Is Trigger", "Box Collider")]
    public sealed partial class BoxColliderBindingIsTrigger : MonoBindingProperty<BoxCollider>
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
