#if UNITY_UGUI

using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Slider", "UI")]
    public sealed partial class SliderBinder : MonoBinder<Slider>
    {
    }

    [Serializable]
    [Label("Value", "Slider")]
    public sealed partial class SliderBindingValue : MonoBindingProperty<Slider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetValue(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].SetValueWithoutNotify(value);
            }
        }
    }

    [Serializable]
    [Label("Interactable", "Slider")]
    public sealed partial class SliderBindingInteractable : MonoBindingProperty<Slider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInteractable(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].interactable = value;
            }
        }
    }

    [Serializable]
    [Label("On Value Changed", "Slider")]
    public sealed partial class SliderBindingOnValueChanged : MonoBindingCommand<Slider>, IBinder
    {
        private readonly UnityAction<float> _command;

        public SliderBindingOnValueChanged()
        {
            _command = OnValueChanged;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onValueChanged.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onValueChanged.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnValueChanged(float value);
    }

    [Serializable]
    [Label("Min Value", "Slider")]
    public sealed partial class SliderBindingMinValue : MonoBindingProperty<Slider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMinValue(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].minValue = value;
            }
        }
    }

    [Serializable]
    [Label("Max Value", "Slider")]
    public sealed partial class SliderBindingMaxValue : MonoBindingProperty<Slider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxValue(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxValue = value;
            }
        }
    }

    [Serializable]
    [Label("Whole Numbers", "Slider")]
    public sealed partial class SliderBindingWholeNumbers : MonoBindingProperty<Slider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetWholeNumbers(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].wholeNumbers = value;
            }
        }
    }
}

#endif
