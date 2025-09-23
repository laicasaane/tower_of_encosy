#if UNITY_EDITOR

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    public static class EncosyEditorUIElementExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Bind<TBindableElement>(this TBindableElement self, SerializedProperty property)
            where TBindableElement : VisualElement, IHasBindingPath
        {
            self.BindingPath = property.propertyPath;
            self.Bind(property.serializedObject);
        }

        public static T WithEditorBuiltInStyleSheet<T>([NotNull] this T self, [NotNull] string styleSheet)
            where T : VisualElement
        {
            if (string.IsNullOrWhiteSpace(styleSheet) == false
                && EditorGUIUtility.Load(styleSheet) is StyleSheet uss
            )
            {
                self.WithStyleSheet(uss);
            }

            return self;
        }

        public static T WithEditorStyleSheet<T>([NotNull] this T self, [NotNull] string styleSheet)
            where T : VisualElement
        {
            if (string.IsNullOrWhiteSpace(styleSheet) == false
                && AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheet) is StyleSheet uss
            )
            {
                self.WithStyleSheet(uss);
            }

            return self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithEditorStyleSheet<T>(
              [NotNull] this T self
            , [NotNull] string darkStyleSheet
            , [NotNull] string lightStyleSheet
        )
            where T : VisualElement
        {
            var styleSheet = EditorAPI.IsDark ? darkStyleSheet : lightStyleSheet;
            return self.WithEditorStyleSheet(styleSheet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithEditorStyleSheet<T>(
              [NotNull] this T self
            , StyleSheet darkStyleSheet
            , StyleSheet lightStyleSheet
        )
            where T : VisualElement
        {
            var styleSheet = EditorAPI.IsDark ? darkStyleSheet : lightStyleSheet;
            return self.WithStyleSheet(styleSheet);
        }
    }
}

#endif
