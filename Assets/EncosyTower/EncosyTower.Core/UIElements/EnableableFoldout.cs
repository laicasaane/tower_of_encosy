using System;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    public class EnableableFoldout : VisualElement, IHasBindingPath
    {
        public static readonly string InputUssClassName = Foldout.inputUssClassName;
        public static readonly string TextUssClassName = Foldout.textUssClassName;
        public static readonly string ToggleUssClassName = Foldout.toggleUssClassName;
        public static readonly string UssClassName = "enableable-foldout";
        public static readonly string EnableToggleUssClassName = $"{UssClassName}__enable_toggle";

        public event Action<EnableableFoldout, ChangeEvent<bool>> ValueChanged;

        public readonly Foldout Foldout;
        public readonly Toggle EnableToggle;

        private readonly Toggle _foldToggle;
        private bool _linkToggleToFoldout;

        public EnableableFoldout(string text, bool linkToggleToFoldout = false) : base()
        {
            _linkToggleToFoldout = linkToggleToFoldout;

            AddToClassList(UssClassName);

            var foldout = Foldout = new Foldout { text = text };

            if (linkToggleToFoldout)
            {
                foldout.value = false;
            }

            hierarchy.Add(foldout);

            if (linkToggleToFoldout)
            {
                var foldToggle = _foldToggle = foldout.Q<Toggle>(className: ToggleUssClassName);

#if UNITY_6000_0_OR_NEWER
                foldToggle.enabledSelf = false;
#else
                foldToggle.SetEnabled(false);
#endif
            }

            var enableToggle = EnableToggle = new Toggle(string.Empty);
            enableToggle.AddToClassList(EnableToggleUssClassName);
            foldout.hierarchy.Insert(1, enableToggle);

            enableToggle.RegisterValueChangedCallback(EnableToggle_OnValueChanged);
        }

        public override VisualElement contentContainer => Foldout.contentContainer;

        public string BindingPath
        {
            get => EnableToggle.bindingPath;
            set => EnableToggle.bindingPath = value;
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
#if UNITY_6000_0_OR_NEWER
                    _foldToggle.enabledSelf = true;
#else
                    _foldToggle.SetEnabled(true);
#endif
                }
            }
        }

        public bool Expand
        {
            get => Foldout.value;
            set => Foldout.value = value;
        }

        private void EnableToggle_OnValueChanged(ChangeEvent<bool> evt)
        {
            if (_linkToggleToFoldout)
            {
                if (evt.newValue == false)
                {
                    Foldout.value = false;
                }

#if UNITY_6000_0_OR_NEWER
                _foldToggle.enabledSelf = evt.newValue;
#else
                _foldToggle.SetEnabled(evt.newValue);
#endif
            }

            ValueChanged?.Invoke(this, evt);
        }
    }
}
