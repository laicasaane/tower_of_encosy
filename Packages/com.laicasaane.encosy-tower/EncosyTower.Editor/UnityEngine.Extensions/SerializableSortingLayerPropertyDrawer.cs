#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.Reflection;
using EncosyTower.Editor.Common;
using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UnityExtensions
{
    [CustomPropertyDrawer(typeof(SerializableSortingLayer), true)]
    public class SerializableSortingLayerPropertyDrawer : PropertyDrawer
    {
        private GUIContent[] _layerNames;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative(nameof(SerializableSortingLayer.value));

            if (valueProp == null)
            {
                WarningIfValuePropertyNull();
                return;
            }

            var oldValue = valueProp.intValue;

            GetData(oldValue, ref _layerNames, out var index);

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(rect, label, property);

            if (index < 0)
            {
                index = 0;
            }

            var tooltipAttrib = fieldInfo.GetCustomAttribute<TooltipAttribute>(true);

            if (tooltipAttrib != null)
            {
                label.tooltip = tooltipAttrib.tooltip;
            }

            index = EditorGUI.Popup(rect, label, index, _layerNames);
            var newValue = SortingLayer.layers[index].id;

            if (newValue != oldValue)
            {
                valueProp.intValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new PropertyContainer(property, false);
            var field = new SerializableSortingLayerField(property.displayName);
            container.Add(field.WithAlignFieldClass());

            SerializableSortingLayer layer = default;
            layer.TryCopyFrom(property);
            field.value = layer;

            field.RegisterValueChangedCallback(OnFieldValueChanged);
            field.TrackPropertyValue(property, OnPropertyValueChanged);

            return container;

            void OnFieldValueChanged(ChangeEvent<SerializableSortingLayer> evt)
            {
                evt.newValue.TryCopyTo(property);

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            void OnPropertyValueChanged(SerializedProperty _)
            {
                var value = field.value;

                if (value.TryCopyFrom(property))
                {
                    field.value = value;
                }
            }
        }

        private static void GetData(int id, ref GUIContent[] layerNames, out int index)
        {
            var layers = SortingLayer.layers.AsSpan();
            var length = layers.Length;
            index = length < 1 ? -1 : 0;

            if (layerNames == null || layerNames.Length != length)
            {
                layerNames = new GUIContent[length];
            }

            for (var i = 0; i < length; i++)
            {
                ref var layer = ref layers[i];

                if (layerNames[i] == null)
                {
                    layerNames[i] = new GUIContent(layer.name);
                }
                else
                {
                    layerNames[i].text = layer.name;
                }

                if (layer.id == id)
                {
                    index = i;
                }
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void WarningIfValuePropertyNull()
        {
            StaticDevLogger.LogWarning("Could not find the layer index property, was it renamed or removed?");
        }
    }
}

#endif
