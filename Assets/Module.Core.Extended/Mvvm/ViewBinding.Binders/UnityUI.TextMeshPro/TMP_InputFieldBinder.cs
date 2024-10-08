#if UNITY_TEXTMESHPRO

using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TMPro
{
    public readonly record struct TMP_TextSelectionData(string Text, int StringPosition, int StringSelectPosition);
}

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.UnityUI.TextMeshPro
{
    [Serializable]
    [Label("TMP Input Field", "TextMeshPro")]
    public sealed partial class TMP_InputFieldBinder : MonoBinder<TMP_InputField>
    {
    }

    [Serializable]
    [Label("Text", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingText : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetText(string value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].SetTextWithoutNotify(value);
            }
        }
    }

    [Serializable]
    [Label("Interactable", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingInteractable : MonoBindingProperty<TMP_InputField>, IBinder
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
    [Label("On Value Changed", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnValueChanged : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<string> _command;

        public TMP_InputFieldBindingOnValueChanged()
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
        partial void OnValueChanged(string value);
    }

    [Serializable]
    [Label("Character Limit", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingCharacterLimit : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCharacterLimit(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].characterLimit = value;
            }
        }
    }

    [Serializable]
    [Label("Content Type", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingContentType : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetContentType(TMP_InputField.ContentType value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].contentType = value;
            }
        }
    }

    [Serializable]
    [Label("Line Type", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingLineType : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLineType(TMP_InputField.LineType value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].lineType = value;
            }
        }
    }

    [Serializable]
    [Label("Rich Text", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingRichText : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRichText(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].richText = value;
            }
        }
    }

    [Serializable]
    [Label("Allow Rich Text Editing", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingAllowRichTextEditing : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAllowRichTextEditing(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].isRichTextEditingAllowed = value;
            }
        }
    }

    [Serializable]
    [Label("Custom Caret Color", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingCustomCaretColor : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCustomCaretColor(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].customCaretColor = value;
            }
        }
    }

    [Serializable]
    [Label("Caret Color", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingCaretColor : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCaretColor(in Color value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].caretColor = value;
            }
        }
    }

    [Serializable]
    [Label("Selection Color", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingSelectionColor : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSelectionColor(in Color value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].selectionColor = value;
            }
        }
    }

    [Serializable]
    [Label("Placeholder Text", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingPlaceholderText : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPlaceholderText(string value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                if (targets[i].placeholder is TMP_Text placeholder)
                {
                    placeholder.text = value;
                }
            }
        }
    }

    [Serializable]
    [Label("Text Color", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingTextColor : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetTextColor(in Color value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];

                if (target.textComponent == false)
                {
                    continue;
                }

                target.textComponent.color = value;
            }
        }
    }

    [Serializable]
    [Label("Placeholder Text Color", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingPlaceholderTextColor : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetPlaceholderTextColor(in Color value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];

                if (target.placeholder == false)
                {
                    continue;
                }

                target.placeholder.color = value;
            }
        }
    }

    [Serializable]
    [Label("Font Asset", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingFontAsset : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFontAsset(TMP_FontAsset value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].fontAsset = value;
            }
        }
    }

    [Serializable]
    [Label("Font Size", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingFontSize : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFontSize(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].pointSize = value;
            }
        }
    }

    [Serializable]
    [Label("Raycast Target", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingRaycastTarget : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRaycastTarget(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];

                if (target.textComponent == false)
                {
                    continue;
                }

                target.textComponent.raycastTarget = value;
            }
        }
    }

    [Serializable]
    [Label("Maskable", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingMaskable : MonoBindingProperty<TMP_InputField>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaskable(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                var target = targets[i];

                if (target.textComponent == false)
                {
                    continue;
                }

                target.textComponent.maskable = value;
            }
        }
    }

    [Serializable]
    [Label("On End Edit", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnEndEdit : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<string> _command;

        public TMP_InputFieldBindingOnEndEdit()
        {
            _command = OnEndEdit;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onEndEdit.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onEndEdit.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnEndEdit(string value);
    }

    [Serializable]
    [Label("On Submit", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnSubmit : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<string> _command;

        public TMP_InputFieldBindingOnSubmit()
        {
            _command = OnSubmit;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onSubmit.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onSubmit.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnSubmit(string value);
    }

    [Serializable]
    [Label("On Select", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnSelect : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<string> _command;

        public TMP_InputFieldBindingOnSelect()
        {
            _command = OnSelect;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onSelect.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onSelect.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnSelect(string value);
    }

    [Serializable]
    [Label("On Deselect", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnDeselect : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<string> _command;

        public TMP_InputFieldBindingOnDeselect()
        {
            _command = OnDeselect;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onDeselect.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onDeselect.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnDeselect(string value);
    }

    [Serializable]
    [Label("On Text Selection", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnTextSelection : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<string, int, int> _command;

        public TMP_InputFieldBindingOnTextSelection()
        {
            _command = (a, b, c) => OnTextSelection(new(a, b, c));
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onTextSelection.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onTextSelection.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnTextSelection(TMP_TextSelectionData value);
    }

    [Serializable]
    [Label("On End Text Selection", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnEndTextSelection : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<string, int, int> _command;

        public TMP_InputFieldBindingOnEndTextSelection()
        {
            _command = (a, b, c) => OnEndTextSelection(new(a, b, c));
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onEndTextSelection.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onEndTextSelection.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnEndTextSelection(TMP_TextSelectionData value);
    }

    [Serializable]
    [Label("On Touch Screen Keyboard Status Changed", "TMP Input Field")]
    public sealed partial class TMP_InputFieldBindingOnTouchScreenKeyboardStatusChanged : MonoBindingCommand<TMP_InputField>, IBinder
    {
        private readonly UnityAction<TouchScreenKeyboard.Status> _command;

        public TMP_InputFieldBindingOnTouchScreenKeyboardStatusChanged()
        {
            _command = OnTouchScreenKeyboardStatusChanged;
        }

        protected override void OnBeforeSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onTouchScreenKeyboardStatusChanged.RemoveListener(command);
            }
        }

        protected override void OnAfterSetTargets()
        {
            var targets = Targets;
            var length = targets.Length;
            var command = _command;

            for (var i = 0; i < length; i++)
            {
                targets[i].onTouchScreenKeyboardStatusChanged.AddListener(command);
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnTouchScreenKeyboardStatusChanged(TouchScreenKeyboard.Status value);
    }
}

#endif
