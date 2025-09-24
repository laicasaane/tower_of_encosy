#if UNITY_EDITOR

using System;
using EncosyTower.Common;
using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Common
{
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidPropertyDrawer : PropertyDrawer
    {
        private const string ERROR = $"Property is not a valid {nameof(SerializableGuid)} type";

        private static readonly GUIContent s_newLabel = new("New");
        private static readonly GUIContent s_v7Label = new("Version 7");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializableGuid guid = default;

            if (guid.TryCopyFrom(property) == false)
            {
                EditorGUI.HelpBox(position, ERROR, MessageType.Error);
                return;
            }

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var x = position.x;
            var m = 4f;
            var w = position.width - m - 50f - 80f;
            var guidRect = new Rect(x, position.y, w, position.height);

            x += w + m;
            w = 50f;

            var newBtnRect = new Rect(x, position.y, w, position.height);

            x += w;
            w = 80f;

            var v7BtnRect = new Rect(x, position.y, w, position.height);

            var guidString = EditorGUI.TextField(guidRect, GUIContent.none, guid.ToString());

            Guid newValue;

            if (GUI.Button(newBtnRect, s_newLabel))
            {
                newValue = SerializableGuid.NewGuid();
            }
            else if (GUI.Button(v7BtnRect, s_v7Label))
            {
                newValue = SerializableGuid.CreateVersion7();
            }
            else if (Guid.TryParse(guidString, out newValue) == false)
            {
                newValue = guid;
            }

            if (newValue != guid)
            {
                Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.propertyPath}");
                newValue.TryCopyTo(property);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new PropertyContainer(property, false);
            var field = new SerializableGuidField(property.displayName);
            container.Add(field.WithAlignFieldClass());

            SerializableGuid guid = default;
            guid.TryCopyFrom(property);
            field.value = guid;

            field.RegisterValueChangedCallback(OnFieldValueChanged);
            field.TrackPropertyValue(property, OnPropertyValueChanged);

            return container;

            void OnFieldValueChanged(ChangeEvent<SerializableGuid> evt)
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
    }
}

#endif
