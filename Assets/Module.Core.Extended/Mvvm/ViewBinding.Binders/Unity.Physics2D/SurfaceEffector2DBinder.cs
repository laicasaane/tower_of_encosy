using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Surface Effector 2D", "Physics 2D")]
    public sealed partial class SurfaceEffector2DBinder : MonoBinder<SurfaceEffector2D>
    {
    }

    [Serializable]
    [Label("Force Scale", "Surface Effector 2D")]
    public sealed partial class SurfaceEffector2DBindingForceScale : MonoBindingProperty<SurfaceEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceScale(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceScale = value;
            }
        }
    }

    [Serializable]
    [Label("Speed", "Surface Effector 2D")]
    public sealed partial class SurfaceEffector2DBindingSpeed : MonoBindingProperty<SurfaceEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpeed(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].speed = value;
            }
        }
    }

    [Serializable]
    [Label("Speed Variation", "Surface Effector 2D")]
    public sealed partial class SurfaceEffector2DBindingSpeedVariation : MonoBindingProperty<SurfaceEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpeedVariation(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].speedVariation = value;
            }
        }
    }

    [Serializable]
    [Label("Use Bounce", "Surface Effector 2D")]
    public sealed partial class SurfaceEffector2DBindingUseBounce : MonoBindingProperty<SurfaceEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseBounce(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useBounce = value;
            }
        }
    }

    [Serializable]
    [Label("Use Contact Force", "Surface Effector 2D")]
    public sealed partial class SurfaceEffector2DBindingUseContactForce : MonoBindingProperty<SurfaceEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseContactForce(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useContactForce = value;
            }
        }
    }

    [Serializable]
    [Label("Use Friction", "Surface Effector 2D")]
    public sealed partial class SurfaceEffector2DBindingUseFriction : MonoBindingProperty<SurfaceEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseFriction(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useFriction = value;
            }
        }
    }
}

