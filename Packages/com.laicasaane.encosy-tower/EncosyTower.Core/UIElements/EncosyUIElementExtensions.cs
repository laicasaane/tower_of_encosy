using System.Diagnostics.CodeAnalysis;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    public static class EncosyUIElementExtensions
    {
        public static VisualElement AddToAlignFieldClass([NotNull] this VisualElement self)
        {
            self.AddToClassList(TextField.alignedFieldUssClassName);
            return self;
        }

        public static VisualElement SetDisplay([NotNull] this VisualElement self, DisplayStyle display)
        {
            self.style.display = display;
            return self;
        }
    }
}
