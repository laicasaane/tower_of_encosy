using UnityEngine.UIElements;

namespace EncosyTower.Modules.UIElements
{
    public static class EncosyUIElementExtensions
    {
        public static VisualElement AddToAlignFieldClass(this VisualElement self)
        {
            self.AddToClassList(TextField.alignedFieldUssClassName);
            return self;
        }

        public static VisualElement SetDisplay(this VisualElement self, DisplayStyle display)
        {
            self.style.display = display;
            return self;
        }
    }
}
