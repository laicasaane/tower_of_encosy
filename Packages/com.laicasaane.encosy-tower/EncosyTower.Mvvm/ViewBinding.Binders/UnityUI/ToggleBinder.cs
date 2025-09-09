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
    [Serializable]
    [Label("Toggle", "UI")]
    public sealed partial class ToggleBinder : MonoBinder<Toggle>
    {
    }

    [Serializable]
    [Label("Is On", "Toggle")]
    public sealed partial class ToggleBindingIsOn : MonoBindingProperty<Toggle>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetIsOn(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].SetIsOnWithoutNotify(value);
            }
        }
    }

    [Serializable]
    [Label("Interactable", "Toggle")]
    public sealed partial class ToggleBindingInteractable : MonoBindingProperty<Toggle>, IBinder
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
    [Label("On Value Changed", "Toggle")]
    public sealed partial class ToggleBindingOnValueChanged : MonoBindingCommand<Toggle>, IBinder
    {
        private readonly UnityAction<bool> _command;

        public ToggleBindingOnValueChanged()
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
        partial void OnValueChanged(bool value);
    }
}

#endif
