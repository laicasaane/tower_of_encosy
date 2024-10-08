#if UNITY_UGUI

using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Scroll Bar", "UI")]
    public sealed partial class ScrollbarBinder : MonoBinder<Scrollbar>
    {
    }

    [Serializable]
    [Label("Interactable", "Scroll Bar")]
    public sealed partial class ScrollbarBindingInteractable : MonoBindingProperty<Scrollbar>, IBinder
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
    [Label("Value", "Scroll Bar")]
    public sealed partial class ScrollbarBindingValue : MonoBindingProperty<Scrollbar>, IBinder
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
    [Label("On Value Changed", "Scroll Bar")]
    public sealed partial class ScrollbarBindingOnValueChanged : MonoBindingCommand<Scrollbar>, IBinder
    {
        private readonly UnityAction<float> _command;

        public ScrollbarBindingOnValueChanged()
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
    [Label("Size", "Scroll Bar")]
    public sealed partial class ScrollbarBindingSize : MonoBindingProperty<Scrollbar>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSize(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].size = value;
            }
        }
    }

    [Serializable]
    [Label("Number Of Steps", "Scroll Bar")]
    public sealed partial class ScrollbarBindingNumberOfSteps : MonoBindingProperty<Scrollbar>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetNumberOfSteps(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].numberOfSteps = value;
            }
        }
    }
}

#endif
