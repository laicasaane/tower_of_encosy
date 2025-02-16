using System;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    public class ButtonTextField : VisualElement, IHasBindingPath
    {
        public static readonly string UssClassName = "button-text-field";

        public readonly TextField TextField;
        public readonly Button Button;

        public event Action<ButtonTextField> Clicked;

        public ButtonTextField(string textLabel)
            : this(textLabel, default)
        {
        }

        public ButtonTextField(string textLabel, Background iconImage)
            : base()
        {
            AddToClassList(UssClassName);

            TextField = new(textLabel);
            Button = new(iconImage, Button_OnClick);

            hierarchy.Add(TextField);
            hierarchy.Add(Button);
        }

        public string BindingPath
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
