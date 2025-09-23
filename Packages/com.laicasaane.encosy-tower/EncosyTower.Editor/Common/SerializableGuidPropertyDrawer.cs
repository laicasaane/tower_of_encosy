#if UNITY_EDITOR

using System;
using EncosyTower.Common;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Common
{
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidPropertyDrawer : PropertyDrawer
    {
        [SerializeField] private StyleSheet _styleSheet;

        private const string ERROR = $"Property is not a valid {nameof(SerializableGuid)} type";

        private static readonly GUIContent s_newLabel = new("New");
        private static readonly GUIContent s_v7Label = new("Version 7");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var bytesProperty = property.FindPropertyRelative("_bytes");

            if (TryGet(bytesProperty, out var guid) == false)
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
                Set(bytesProperty, newValue);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement().WithStyleSheet(_styleSheet);

            // Create property fields.
            var field = new SerializableGuidField(property.displayName);
            var newBtn = new Button() { text = s_newLabel.text };
            var v7Btn = new Button() { text = s_v7Label.text };

            newBtn.clicked += () => field.value = SerializableGuid.NewGuid();
            v7Btn.clicked += () => field.value = SerializableGuid.CreateVersion7();

            container.Add(field);
            container.Add(newBtn);
            container.Add(v7Btn);

            return container;
        }

        private static bool TryGet(SerializedProperty property, out SerializableGuid result)
        {
            if (property is null
                || property.isFixedBuffer == false
                || property.fixedBufferSize != SerializableGuid.SIZE
            )
            {
                result = default;
                return false;
            }

            Span<byte> bytes = stackalloc byte[SerializableGuid.SIZE];

            for (var i = 0; i < SerializableGuid.SIZE; i++)
            {
                bytes[i] = (byte)property.GetFixedBufferElementAtIndex(i).intValue;
            }

            result = new SerializableGuid(bytes);
            return true;
        }

        private static void Set(SerializedProperty property, in SerializableGuid value)
        {
            if (property is null
                || property.isFixedBuffer == false
                || property.fixedBufferSize != SerializableGuid.SIZE
            )
            {
                return;
            }

            var bytes = value.AsReadOnlySpan();

            for (var i = 0; i < SerializableGuid.SIZE; i++)
            {
                property.GetFixedBufferElementAtIndex(i).intValue = bytes[i];
            }
        }
    }
}

#endif
