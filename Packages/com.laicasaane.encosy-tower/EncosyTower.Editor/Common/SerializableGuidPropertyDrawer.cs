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
    [CustomPropertyDrawer(typeof(SerializableGuid), true)]
    public class SerializableGuidPropertyDrawer : PropertyDrawer
    {
        private const string ERROR = $"Property is not a valid {nameof(SerializableGuid)} type";

        private static readonly GUIContent s_emptyLabel = new(
              "Empty"
            , "Sets to all zeros."
        );

        private static readonly GUIContent s_newV4Label = new(
              "New v4"
            , "Creates a new Guid according to RFC 4122, following the Version 4 format."
        );

        private static readonly GUIContent s_newV7Label = new(
              "New v7"
            , "Creates a new Guid according to RFC 9562, following the Version 7 format."
        );

        [InitializeOnLoadMethod]
        private static void RegisterContextualPropertyMenu()
        {
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;

            return;

            static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
            {
                if (property.propertyType != SerializedPropertyType.Generic
                    || property.type != typeof(SerializableGuid).Name
                )
                {
                    return;
                }

                var propertyCopy = property.Copy();

                menu.AddItem(s_emptyLabel, false, () => {
                    SerializableGuid.Empty.TryCopyTo(propertyCopy);
                    propertyCopy.serializedObject.ApplyModifiedProperties();
                });
            }
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            SerializableGuid guid = default;

            if (guid.TryCopyFrom(property) == false)
            {
                EditorGUI.HelpBox(rect, ERROR, MessageType.Error);
                return;
            }

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(rect, label, property);

            // Draw label
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var x = rect.x;
            var m = 4f;
            var w = rect.width - m - 57f - 57f;
            var guidRect = new Rect(x, rect.y, w, rect.height);

            x += w + m;
            w = 57f;

            var newV4BtnRect = new Rect(x, rect.y, w, rect.height);

            x += w;
            w = 57f;

            var newV7BtnRect = new Rect(x, rect.y, w, rect.height);

            var guidString = EditorGUI.TextField(guidRect, GUIContent.none, guid.ToString());

            Guid newValue;

            if (GUI.Button(newV4BtnRect, s_newV4Label))
            {
                newValue = SerializableGuid.NewGuid();
            }
            else if (GUI.Button(newV7BtnRect, s_newV7Label))
            {
                newValue = SerializableGuid.CreateVersion7();
            }
            else if (Guid.TryParse(guidString, out newValue) == false)
            {
                newValue = SerializableGuid.Empty;
            }

            if (newValue != guid)
            {
                newValue.TryCopyTo(property);
                property.serializedObject.ApplyModifiedProperties();
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
