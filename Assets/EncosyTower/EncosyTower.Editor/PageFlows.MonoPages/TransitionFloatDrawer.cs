#if UNITY_EDITOR

using System.Reflection;
using EncosyTower.PageFlows.MonoPages;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.PageFlows.MonoPages
{
    [CustomPropertyDrawer(typeof(TransitionFloat), true)]
    public class TransitionFloatDrawer : PropertyDrawer
    {
        private static readonly GUIContent s_labelStart = new("Start");
        private static readonly GUIContent s_labelEnd = new("End");

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var startProperty = property.FindPropertyRelative(nameof(TransitionFloat.start));
            var endProperty = property.FindPropertyRelative(nameof(TransitionFloat.end));

            var tooltipAttrib = this.fieldInfo.GetCustomAttribute<TooltipAttribute>(true);

            if (tooltipAttrib != null)
            {
                label.tooltip = tooltipAttrib.tooltip;
            }

            rect = EditorGUI.PrefixLabel(rect, label);

            rect.x -= 14f;
            rect.width += 14f;
            rect.width *= 0.5f;

            var rect1 = rect;
            var rect2 = rect;
            rect2.x += rect.width;

            DrawField(rect1, 45f, s_labelStart, startProperty);
            DrawField(rect2, 40f, s_labelEnd, endProperty);
        }

        private void DrawField(Rect rect, float labelWidth, GUIContent label, SerializedProperty property)
        {
            var rect1 = rect;
            rect1.width = labelWidth;

            var rect2 = rect;
            rect2.width -= labelWidth;
            rect2.x += labelWidth;

            EditorGUI.LabelField(rect1, label, EditorStyles.label);
            property.floatValue = EditorGUI.FloatField(rect2, property.floatValue);
        }
    }
}

#endif
