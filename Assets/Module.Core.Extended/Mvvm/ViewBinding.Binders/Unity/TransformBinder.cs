using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable]
    [Label("Transform")]
    public sealed partial class TransformBinder : MonoBinder<Transform>
    {
    }

    [Serializable]
    [Label("Position", "Transform")]
    public sealed partial class TransformBindingPosition : MonoBindingProperty<Transform>, IBinder
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

    [Serializable]
    [Label("Rotation", "Transform")]
    public sealed partial class TransformBindingRotation : MonoBindingProperty<Transform>, IBinder
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

    [Serializable]
    [Label("Euler Angles", "Transform")]
    public sealed partial class TransformBindingEulerAngles : MonoBindingProperty<Transform>, IBinder
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

    [Serializable]
    [Label("Local Position", "Transform")]
    public sealed partial class TransformBindingLocalPosition : MonoBindingProperty<Transform>, IBinder
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

    [Serializable]
    [Label("Local Rotation", "Transform")]
    public sealed partial class TransformBindingLocalRotation : MonoBindingProperty<Transform>, IBinder
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

    [Serializable]
    [Label("Local Euler Angles", "Transform")]
    public sealed partial class TransformBindingLocalEulerAngles : MonoBindingProperty<Transform>, IBinder
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

    [Serializable]
    [Label("Local Scale", "Transform")]
    public sealed partial class TransformBindingLocalScale : MonoBindingProperty<Transform>, IBinder
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
