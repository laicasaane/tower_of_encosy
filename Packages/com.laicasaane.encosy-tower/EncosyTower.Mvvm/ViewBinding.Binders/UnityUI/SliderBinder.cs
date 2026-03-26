#if UNITY_UGUI

#pragma warning disable CS0657, IDE0005

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable, Binder]
    [Label("Slider", "UI")]
    public sealed partial class SliderBinder : MonoBinder<Slider>
    {
    }

    [Serializable, Binder]
    [Label("Value", "Slider")]
    public sealed partial class SliderBindingValue : MonoBindingProperty<Slider>
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

    [Serializable, Binder]
    [Label("Interactable", "Slider")]
    public sealed partial class SliderBindingInteractable : MonoBindingProperty<Slider>
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

    [Serializable, Binder]
    [Label("On Value Changed", "Slider")]
    public sealed partial class SliderBindingOnValueChanged : MonoBindingCommand<Slider>
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

    [Serializable, Binder]
    [Label("Min Value", "Slider")]
    public sealed partial class SliderBindingMinValue : MonoBindingProperty<Slider>
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

    [Serializable, Binder]
    [Label("Max Value", "Slider")]
    public sealed partial class SliderBindingMaxValue : MonoBindingProperty<Slider>
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

    [Serializable, Binder]
    [Label("Whole Numbers", "Slider")]
    public sealed partial class SliderBindingWholeNumbers : MonoBindingProperty<Slider>
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
