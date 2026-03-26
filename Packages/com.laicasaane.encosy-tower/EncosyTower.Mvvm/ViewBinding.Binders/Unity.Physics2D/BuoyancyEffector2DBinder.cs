#if UNITY_PHYSICS_2D

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable, Binder]
    [Label("Buoyancy Effector 2D", "Physics 2D")]
    public sealed partial class BuoyancyEffector2DBinder : MonoBinder<BuoyancyEffector2D>
    {
    }

    [Serializable, Binder]
    [Label("Angular Drag", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingAngularDrag : MonoBindingProperty<BuoyancyEffector2D>
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

    [Serializable, Binder]
    [Label("Density", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingDensity : MonoBindingProperty<BuoyancyEffector2D>
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

    [Serializable, Binder]
    [Label("Flow Angle", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingFlowAngle : MonoBindingProperty<BuoyancyEffector2D>
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

    [Serializable, Binder]
    [Label("Flow Magnitude", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingFlowMagnitude : MonoBindingProperty<BuoyancyEffector2D>
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

    [Serializable, Binder]
    [Label("Flow Variation", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingFlowVariation : MonoBindingProperty<BuoyancyEffector2D>
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

    [Serializable, Binder]
    [Label("Linear Drag", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingLinearDrag : MonoBindingProperty<BuoyancyEffector2D>
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

    [Serializable, Binder]
    [Label("Surface Level", "Buoyancy Effector 2D")]
    public sealed partial class BuoyancyEffector2DBindingSurfaceLevel : MonoBindingProperty<BuoyancyEffector2D>
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

#endif
