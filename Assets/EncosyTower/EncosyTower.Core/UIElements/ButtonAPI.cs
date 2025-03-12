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

            if (iconImage.texture || iconImage.sprite)
            {
                var imageElement = new Image();
                imageElement.AddToClassList(ImageUssClassName);

                if (iconImage.texture)
                {
                    imageElement.image = iconImage.texture;
                }
                else
                {
                    imageElement.sprite = iconImage.sprite;
                }

                button.hierarchy.Add(imageElement);
                button.AddToClassList(IconUssClassName);
            }

            return button;
        }

        public static void SetToTextElement([NotNull] this Button button, string text)
        {
            var textElement = button.Q<TextElement>(className: TextUssClassName);

            if (textElement is not null)
            {
                textElement.text = text ?? string.Empty;
                button.EnableInClassList(IconOnlyUssClassName, false);
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            textElement = new TextElement() {
                text = text,
            };

            textElement.AddToClassList(TextUssClassName);
            button.hierarchy.Add(textElement);
            button.EnableInClassList(IconOnlyUssClassName, false);
        }
    }
}

#endif
