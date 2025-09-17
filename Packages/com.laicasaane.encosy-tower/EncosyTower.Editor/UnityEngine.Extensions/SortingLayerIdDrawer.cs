#if UNITY_EDITOR

using System;
using System.Reflection;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.UnityExtensions
{
    [CustomPropertyDrawer(typeof(SortingLayerId), true)]
    public class SortingLayerIdDrawer : PropertyDrawer
    {
        private GUIContent[] _layerNames;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var idProperty = property.FindPropertyRelative(nameof(SortingLayerId.value));

            if (idProperty == null)
            {
                WarningIfValuePropertyNull();
                return;
            }

            GetData(idProperty.intValue, ref _layerNames, out var index);

            if (index < 0)
            {
                if (Application.isPlaying)
                {
                    // If application is playing we dont want to change the layers on the fly
                    // Instead, just display them as an int value
                    idProperty.intValue = EditorGUI.IntField(rect, property.displayName, idProperty.intValue);
                    return;
                }
                else
                {
                    // If the application is not running, reset the layer to the default layer
                    idProperty.intValue = 0;
                    index = 0;
                }
            }

            var tooltipAttrib = fieldInfo.GetCustomAttribute<TooltipAttribute>(true);

            if (tooltipAttrib != null)
            {
                label.tooltip = tooltipAttrib.tooltip;
            }

            index = EditorGUI.Popup(rect, label, index, _layerNames);
            idProperty.intValue = SortingLayer.layers[index].id;
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

        [HideInCallstack]
        private static void WarningIfValuePropertyNull()
        {
            StaticDevLogger.LogWarning("Could not find the layer index property, was it renamed or removed?");
        }
    }
}

#endif
