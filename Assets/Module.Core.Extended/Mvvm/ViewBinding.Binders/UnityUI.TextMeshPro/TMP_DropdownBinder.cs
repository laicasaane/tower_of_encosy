#if UNITY_TEXTMESHPRO

using System;
using System.Collections.Generic;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.UnityUI.TextMeshPro
{
    [Serializable]
    [Label("TMP Dropdown", "TextMeshPro")]
    public sealed partial class TMP_DropdownBinder : MonoBinder<TMP_Dropdown>
    {
    }

    [Serializable]
    [Label("Value", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingValue : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetValue(int value)
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
    [Label("Caption Text", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingCaptionText : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCaptionText(string value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];

                if (target.captionText == false)
                {
                    continue;
                }

                target.captionText.text = value;
            }
        }
    }

    [Serializable]
    [Label("Interactable", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingInteractable : MonoBindingProperty<TMP_Dropdown>, IBinder
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
    [Label("On Value Changed", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingOnValueChanged : MonoBindingCommand<TMP_Dropdown>, IBinder
    {
        private readonly UnityAction<int> _command;

        public TMP_DropdownBindingOnValueChanged()
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
        partial void OnValueChanged(int value);
    }

    [Serializable]
    [Label("Set Options", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingSetOptions : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOptions(List<TMP_Dropdown.OptionData> value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];
                target.options.Clear();
                target.AddOptions(value);
            }
        }
    }

    [Serializable]
    [Label("Set String Options", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingSetStringOptions : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetStringOptions(List<string> value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];
                target.options.Clear();
                target.AddOptions(value);
            }
        }
    }

    [Serializable]
    [Label("Set Sprite Options", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingSetSpriteOptions : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSpriteOptions(List<Sprite> value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];
                target.options.Clear();
                target.AddOptions(value);
            }
        }
    }

    [Serializable]
    [Label("Add Options", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingAddOptions : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void AddOptions(List<TMP_Dropdown.OptionData> value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].AddOptions(value);
            }
        }
    }

    [Serializable]
    [Label("Add String Options", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingAddStringOptions : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void AddStringOptions(List<string> value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].AddOptions(value);
            }
        }
    }

    [Serializable]
    [Label("Add Sprite Options", "TMP Dropdown")]
    public sealed partial class TMP_DropdownBindingAddSpriteOptions : MonoBindingProperty<TMP_Dropdown>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void AddSpriteOptions(List<Sprite> value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].AddOptions(value);
            }
        }
    }
}

#endif
