using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Point Effector 2D", "Physics 2D")]
    public sealed partial class PointEffector2DBinder : MonoBinder<PointEffector2D>
    {
    }

    [Serializable]
    [Label("Angular Drag", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingAngularDrag : MonoBindingProperty<PointEffector2D>, IBinder
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
    [Label("Distance Scale", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingDistanceScale : MonoBindingProperty<PointEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDistanceScale(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].distanceScale = value;
            }
        }
    }

    [Serializable]
    [Label("Drag", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingDrag : MonoBindingProperty<PointEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDrag(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].drag = value;
            }
        }
    }

    [Serializable]
    [Label("Force Magnitude", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingForceMagnitude : MonoBindingProperty<PointEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceMagnitude(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceMagnitude = value;
            }
        }
    }

    [Serializable]
    [Label("Force Mode", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingForceMode : MonoBindingProperty<PointEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceMode(EffectorForceMode2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceMode = value;
            }
        }
    }

    [Serializable]
    [Label("Force Source", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingForceSource : MonoBindingProperty<PointEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceSource(EffectorSelection2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceSource = value;
            }
        }
    }

    [Serializable]
    [Label("Force Target", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingForceTarget : MonoBindingProperty<PointEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceTarget(EffectorSelection2D value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceTarget = value;
            }
        }
    }

    [Serializable]
    [Label("Force Variation", "Point Effector 2D")]
    public sealed partial class PointEffector2DBindingForceVariation : MonoBindingProperty<PointEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceVariation(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceVariation = value;
            }
        }
    }
}

