using System;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ButtonTextField : VisualElement, IHasBindingPath
    {
        public static readonly string UssClassName = "button-text-field";

        public readonly TextField TextField;
        public readonly Button Button;

        public event Action<ButtonTextField> Clicked;

        public ButtonTextField() : this(string.Empty)
        {
        }

        public ButtonTextField(string textLabel)
            : this(textLabel, default)
        {
        }

        public ButtonTextField(string textLabel, Background iconImage)
            : base()
        {
            AddToClassList(UssClassName);

            TextField = new(textLabel);

#if UNITY_6000_0_OR_NEWER
            Button = new(iconImage, Button_OnClick);
#else
            Button = ButtonAPI.CreateButton(iconImage, Button_OnClick);
#endif

            hierarchy.Add(TextField);
            hierarchy.Add(Button);
        }

        public string bindingPath
        {
            get => TextField.bindingPath;
            set => TextField.bindingPath = value;
        }

        private void Button_OnClick()
        {
            Clicked?.Invoke(this);
        }
    }
}
