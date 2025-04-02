#if UNITY_EDITOR

using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    public static class EncosyEditorUIElementExtensions
    {
        public static void Bind<TBindableElement>(this TBindableElement self, SerializedProperty property)
            where TBindableElement : VisualElement, IHasBindingPath
        {
            self.BindingPath = property.propertyPath;
            self.Bind(property.serializedObject);
        }

        public static void ApplyEditorBuiltInStyleSheet(this VisualElement root, string styleSheet)
        {
            if (string.IsNullOrWhiteSpace(styleSheet) == false
                && EditorGUIUtility.Load(styleSheet) is StyleSheet uss
                && uss.IsValid()
            )
            {
                root.styleSheets.Add(uss);
            }
        }

        public static void ApplyEditorStyleSheet(this VisualElement root, string styleSheet)
        {
            if (string.IsNullOrWhiteSpace(styleSheet) == false
                && AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheet) is StyleSheet uss
                && uss.IsValid()
            )
            {
                root.styleSheets.Add(uss);
            }
        }

        public static void ApplyEditorStyleSheet(this VisualElement root, string darkStyleSheet, string lightStyleSheet)
        {
            var styleSheet = EditorAPI.IsDark ? darkStyleSheet : lightStyleSheet;
            ApplyEditorStyleSheet(root, styleSheet);
        }

        public static void ApplyEditorStyleSheet(this VisualElement root, StyleSheet darkStyleSheet, StyleSheet lightStyleSheet)
        {
            var styleSheet = EditorAPI.IsDark ? darkStyleSheet : lightStyleSheet;

            if (styleSheet.IsValid())
            {
                root.styleSheets.Add(styleSheet);
            }
        }
    }
}

#endif
