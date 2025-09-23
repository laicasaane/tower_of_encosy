using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.UnityExtensions;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    public static class EncosyUIElementExtensions
    {
        /// <summary>
        /// Adds <paramref name="styleSheet"/> to the style sheets of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithStyleSheet<T>([NotNull] this T self, StyleSheet styleSheet)
            where T : VisualElement
        {
            if (styleSheet.IsValid())
            {
                self.styleSheets.Add(styleSheet);
            }

            return self;
        }

        /// <summary>
        /// Adds <paramref name="child"/> to <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithChild<T>([NotNull] this T self, [NotNull] VisualElement child)
            where T : VisualElement
        {
            self.Add(child);
            return self;
        }

        /// <summary>
        /// Adds <paramref name="className"/> to the class list of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithClass<T>([NotNull] this T self, [NotNull] string className)
            where T : VisualElement
        {
            self.AddToClassList(className);
            return self;
        }

        /// <summary>
        /// Adds <c>unity-base-field__aligned</c> to the class list of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithAlignFieldClass<T>([NotNull] this T self)
            where T : VisualElement
            => WithClass(self, TextField.alignedFieldUssClassName);

        /// <summary>
        /// Sets <paramref name="display"/> to the style of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithDisplay<T>([NotNull] this T self, DisplayStyle display)
            where T : VisualElement
        {
            self.style.display = display;
            return self;
        }
    }
}
