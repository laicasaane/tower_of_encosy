using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Buoyancy Effector 2D", "Physics 2D")]
    public sealed partial class BuoyancyEffector2DBinder : MonoBinder<BuoyancyEffector2D>
    {
    }

    [Serializable]
    [Label("Angular Drag", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingAngularDrag : MonoBindingProperty<BuoyancyEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAngularDrag(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].angularDrag = value;
            }
        }
    }

    [Serializable]
    [Label("Density", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingDensity : MonoBindingProperty<BuoyancyEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDensity(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].density = value;
            }
        }
    }

    [Serializable]
    [Label("Flow Angle", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingFlowAngle : MonoBindingProperty<BuoyancyEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFlowAngle(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].flowAngle = value;
            }
        }
    }

    [Serializable]
    [Label("Flow Magnitude", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingFlowMagnitude : MonoBindingProperty<BuoyancyEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFlowMagnitude(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].flowMagnitude = value;
            }
        }
    }

    [Serializable]
    [Label("Flow Variation", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingFlowVariation : MonoBindingProperty<BuoyancyEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFlowVariation(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].flowVariation = value;
            }
        }
    }

    [Serializable]
    [Label("Linear Drag", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingLinearDrag : MonoBindingProperty<BuoyancyEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLinearDrag(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].linearDrag = value;
            }
        }
    }

    [Serializable]
    [Label("Surface Level", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingSurfaceLevel : MonoBindingProperty<BuoyancyEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSurfaceLevel(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].surfaceLevel = value;
            }
        }
    }
}

