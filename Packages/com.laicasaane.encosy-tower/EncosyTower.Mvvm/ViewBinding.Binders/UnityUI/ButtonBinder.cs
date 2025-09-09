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
    [Label("Button", "UI")]
    public sealed partial class ButtonBinder : MonoBinder<Button>
    {
    }

    [Serializable]
    [Label("Interactable", "Button")]
    public sealed partial class ButtonBindingInteractable : MonoBindingProperty<Button>, IBinder
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
    [Label("On Click", "Button")]
    public sealed partial class ButtonBindingOnClick : MonoBindingCommand<Button>, IBinder
    {
        private readonly UnityAction _command;

        public ButtonBindingOnClick()
        {
            _command = OnClick;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onClick.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onClick.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnClick();
    }
}

#endif
