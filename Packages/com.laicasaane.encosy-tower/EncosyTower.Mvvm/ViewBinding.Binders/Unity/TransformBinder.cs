#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable, Binder]
    [Label("Transform")]
    public sealed partial class TransformBinder : MonoBinder<Transform>
    {
    }

    [Serializable, Binder]
    [Label("Position", "Transform")]
    public sealed partial class TransformBindingPosition : MonoBindingProperty<Transform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPosition(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].position = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Rotation", "Transform")]
    public sealed partial class TransformBindingRotation : MonoBindingProperty<Transform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRotation(in Quaternion value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].rotation = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Euler Angles", "Transform")]
    public sealed partial class TransformBindingEulerAngles : MonoBindingProperty<Transform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetEulerAngles(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].eulerAngles = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Local Position", "Transform")]
    public sealed partial class TransformBindingLocalPosition : MonoBindingProperty<Transform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLocalPosition(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].localPosition = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Local Rotation", "Transform")]
    public sealed partial class TransformBindingLocalRotation : MonoBindingProperty<Transform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLocalRotation(in Quaternion value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].localRotation = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Local Euler Angles", "Transform")]
    public sealed partial class TransformBindingLocalEulerAngles : MonoBindingProperty<Transform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLocalEulerAngles(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].localEulerAngles = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Local Scale", "Transform")]
    public sealed partial class TransformBindingLocalScale : MonoBindingProperty<Transform>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLocalScale(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].localScale = value;
            }
        }
    }
}
