#if UNITY_EDITOR

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Core;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    [ApiForEditor]
    public static class EncosyEditorUIElementExtensions
    {
        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithBind<T>([NotNull] this T self, [NotNull] SerializedProperty property)
            where T : VisualElement, IHasBindingPath
        {
            self.bindingPath = property.propertyPath;
            self.Bind(property.serializedObject);
            return self;
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithBind<T>([NotNull] this T self, SerializedObject obj)
            where T : VisualElement
        {
            self.Bind(obj);
            return self;
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithBindProperty<T>([NotNull] this T self, [NotNull] SerializedProperty property)
                where T : IBindable
        {
            BindingExtensions.BindProperty(self, property);
            return self;
        }

        [ApiForEditor]
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

        [ApiForEditor]
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

        [ApiForEditor]
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

        [ApiForEditor]
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
