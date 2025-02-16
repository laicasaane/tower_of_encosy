using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    public class VisualSeparator : VisualElement
    {
        public static readonly string UssClassName = "visual-separator";

        public VisualSeparator(string additionalUssClassName = "") : base()
        {
            AddToClassList(UssClassName);

            if (string.IsNullOrWhiteSpace(additionalUssClassName) == false)
            {
                AddToClassList(additionalUssClassName);
            }
        }
    }
}
