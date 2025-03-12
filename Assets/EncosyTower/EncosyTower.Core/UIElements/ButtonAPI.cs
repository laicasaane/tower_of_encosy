#if !UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    /// <summary>
    /// Emulates Button element in Unity 6
    /// </summary>
    public static class ButtonAPI
    {
        public static readonly string IconUssClassName = $"{Button.ussClassName}--with-icon";
        public static readonly string IconOnlyUssClassName = $"{Button.ussClassName}--with-icon-only";
        public static readonly string ImageUssClassName = $"{Button.ussClassName}__image";
        public static readonly string TextUssClassName = $"{Button.ussClassName}__text";

        public static Button CreateButton(Background iconImage, Action clickEvent = null)
        {
            var button = new Button(clickEvent);
            button.SetToImageElement(iconImage);
            return button;
        }

        public static void SetToTextElement([NotNull] this Button button, string text)
        {
            var textElement = button.Q<TextElement>(className: TextUssClassName);

            if (textElement is null)
            {
                textElement = new TextElement();
                textElement.AddToClassList(TextUssClassName);

                button.hierarchy.Add(textElement);
                button.EnableInClassList(IconOnlyUssClassName, false);
            }

            textElement.text = text ?? string.Empty;
        }

        public static void SetToImageElement([NotNull] this Button button, Background iconImage)
        {
            var imageElement = button.Q<Image>(className: ImageUssClassName);

            if (imageElement is null)
            {
                imageElement = new Image();
                imageElement.AddToClassList(ImageUssClassName);

                button.hierarchy.Add(imageElement);
                button.AddToClassList(IconUssClassName);
            }

            if (iconImage.texture.IsInvalid() && iconImage.sprite.IsInvalid())
            {
                return;
            }

            if (iconImage.texture)
            {
                imageElement.image = iconImage.texture;
            }
            else
            {
                imageElement.sprite = iconImage.sprite;
            }
        }
    }
}

#endif
