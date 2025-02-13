#if UNITY_EDITOR

using EncosyTower.Modules.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor.UIElements
{
    public static class EncosyBindingExtensions
    {
        public static void Bind<TBindableElement>(this TBindableElement self, SerializedProperty property)
            where TBindableElement : VisualElement, IHasBindingPath
        {
            self.BindingPath = property.propertyPath;
            self.Bind(property.serializedObject);
        }
    }
}

#endif
