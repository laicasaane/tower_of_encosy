using System;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.UIElements
{
    public class EnableableFoldout : VisualElement, IHasBindingPath
    {
        public static readonly string InputUssClassName = Foldout.inputUssClassName;
        public static readonly string TextUssClassName = Foldout.textUssClassName;
        public static readonly string ToggleUssClassName = Foldout.toggleUssClassName;
        public static readonly string UssClassName = "enableable-foldout";
        public static readonly string EnableToggleUssClassName = $"{UssClassName}__enable_toggle";

        public event Action<ChangeEvent<bool>> ValueChanged;

        private readonly Foldout _foldout;
        private readonly Toggle _foldToggle;
        private readonly Toggle _enableToggle;
        private bool _linkToggleToFoldout;

        public EnableableFoldout(string text, bool linkToggleToFoldout = false) : base()
        {
            _linkToggleToFoldout = linkToggleToFoldout;

            AddToClassList(UssClassName);

            var foldout = _foldout = new Foldout { text = text };

            if (linkToggleToFoldout)
            {
                foldout.value = false;
            }

            hierarchy.Add(foldout);

            if (linkToggleToFoldout)
            {
                var foldToggle = _foldToggle = foldout.Q<Toggle>(className: ToggleUssClassName);
                foldToggle.enabledSelf = false;
            }

            var enableToggle = _enableToggle = new Toggle(string.Empty);
            enableToggle.AddToClassList(EnableToggleUssClassName);
            foldout.hierarchy.Insert(1, enableToggle);

            enableToggle.RegisterValueChangedCallback(EnableToggle_OnValueChanged);
        }

        public override VisualElement contentContainer => _foldout.contentContainer;

        public string BindingPath
        {
            get => _enableToggle.bindingPath;
            set => _enableToggle.bindingPath = value;
        }

        public bool LinkToggleToFoldout
        {
            get
            {
                return _linkToggleToFoldout;
            }

            set
            {
                _linkToggleToFoldout = value;

                if (_linkToggleToFoldout == false)
                {
                    _foldToggle.enabledSelf = true;
                }
            }
        }

        private void EnableToggle_OnValueChanged(ChangeEvent<bool> evt)
        {
            if (_linkToggleToFoldout)
            {
                if (evt.newValue == false)
                {
                    _foldout.value = false;
                }

                _foldToggle.enabledSelf = evt.newValue;
            }

            ValueChanged?.Invoke(evt);
        }
    }
}
