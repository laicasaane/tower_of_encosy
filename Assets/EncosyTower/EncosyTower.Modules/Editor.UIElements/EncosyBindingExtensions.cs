#if UNITY_EDITOR

using EncosyTower.Modules.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor.UIElements
{
    public static class EncosyBindingExtensions
    {
        public static void Bind(this BindableElement self, SerializedProperty property)
        {
            self.bindingPath = property.propertyPath;
            self.Bind(property.serializedObject);
        }

        public static void Bind<T>(this T self, SerializedProperty property)
            where T : VisualElement, IHasBindingPath
        {
            self.BindingPath = property.propertyPath;
            self.Bind(property.serializedObject);
        }
    }
}

#endif
