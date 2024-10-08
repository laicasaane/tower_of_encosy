using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Platform Effector 2D", "Physics 2D")]
    public sealed partial class PlatformEffector2DBinder : MonoBinder<PlatformEffector2D>
    {
    }

    [Serializable]
    [Label("Rotational Offset", "Platform Effector 2D")]
    public sealed partial class PlatformEffector2DBindingRotationalOffset : MonoBindingProperty<PlatformEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRotationalOffset(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].rotationalOffset = value;
            }
        }
    }

    [Serializable]
    [Label("Side Arc", "Platform Effector 2D")]
    public sealed partial class PlatformEffector2DBindingSideArc : MonoBindingProperty<PlatformEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSideArc(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sideArc = value;
            }
        }
    }

    [Serializable]
    [Label("Surface Arc", "Platform Effector 2D")]
    public sealed partial class PlatformEffector2DBindingSurfaceArc : MonoBindingProperty<PlatformEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSurfaceArc(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].surfaceArc = value;
            }
        }
    }

    [Serializable]
    [Label("Use One Way", "Platform Effector 2D")]
    public sealed partial class PlatformEffector2DBindingUseOneWay : MonoBindingProperty<PlatformEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseOneWay(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useOneWay = value;
            }
        }
    }

    [Serializable]
    [Label("Use One Way Grouping", "Platform Effector 2D")]
    public sealed partial class PlatformEffector2DBindingUseOneWayGrouping : MonoBindingProperty<PlatformEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseOneWayGrouping(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useOneWayGrouping = value;
            }
        }
    }

    [Serializable]
    [Label("Use Side Bounce", "Platform Effector 2D")]
    public sealed partial class PlatformEffector2DBindingUseSideBounce : MonoBindingProperty<PlatformEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseSideBounce(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useSideBounce = value;
            }
        }
    }

    [Serializable]
    [Label("Use Side Friction", "Platform Effector 2D")]
    public sealed partial class PlatformEffector2DBindingUseSideFriction : MonoBindingProperty<PlatformEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseSideFriction(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useSideFriction = value;
            }
        }
    }
}

