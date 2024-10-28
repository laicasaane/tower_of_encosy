using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.Unity.Physics2D
{
    [Serializable]
    [Label("Area Effector 2D", "Physics 2D")]
    public sealed partial class AreaEffector2DBinder : MonoBinder<AreaEffector2D>
    {
    }

    [Serializable]
    [Label("Angular Drag", "Area Effector 2D")]
    public sealed partial class AreaEffector2DBindingAngularDrag : MonoBindingProperty<AreaEffector2D>, IBinder
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
    [Label("Drag", "Area Effector 2D")]
    public sealed partial class AreaEffector2DBindingDrag : MonoBindingProperty<AreaEffector2D>, IBinder
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
    [Label("Force Angle", "Area Effector 2D")]
    public sealed partial class AreaEffector2DBindingForceAngle : MonoBindingProperty<AreaEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetForceAngle(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].forceAngle = value;
            }
        }
    }

    [Serializable]
    [Label("Force Magnitude", "Area Effector 2D")]
    public sealed partial class AreaEffector2DBindingForceMagnitude : MonoBindingProperty<AreaEffector2D>, IBinder
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
    [Label("Force Target", "Area Effector 2D")]
    public sealed partial class AreaEffector2DBindingForceTarget : MonoBindingProperty<AreaEffector2D>, IBinder
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
    [Label("Force Variation", "Area Effector 2D")]
    public sealed partial class AreaEffector2DBindingForceVariation : MonoBindingProperty<AreaEffector2D>, IBinder
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

    [Serializable]
    [Label("Use Global Angle", "Area Effector 2D")]
    public sealed partial class AreaEffector2DBindingUseGlobalAngle : MonoBindingProperty<AreaEffector2D>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetUseGlobalAngle(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].useGlobalAngle = value;
            }
        }
    }
}

