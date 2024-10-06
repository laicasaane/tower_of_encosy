#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private delegate void OnCopyValue(SerializedProperty src, SerializedProperty dest);

        private delegate bool OnValidatePasteValue(SerializedProperty src, SerializedArrayProperty dest);

        private class SerializedArrayProperty
        {
            private readonly string _undoKey;
            private readonly OnCopyValue _onCopyValue;
            private readonly OnValidatePasteValue _onValidatePasteAll;
            private readonly OnValidatePasteValue _onValidatePasteSingle;

            private readonly string _selectedIndexKey;
            private readonly Action<SerializedArrayProperty> _onSelectedIndexChanged;

            private SerializedProperty _property;
            private int? _selectedIndex = null;

            public SerializedArrayProperty(
                  string undoKey
                , OnCopyValue onCopyValue
                , OnValidatePasteValue onValidatePasteAll
                , OnValidatePasteValue onValidatePasteSingle
            )
            {
                _undoKey = undoKey;
                _onCopyValue = onCopyValue;
                _onValidatePasteAll = onValidatePasteAll;
                _onValidatePasteSingle = onValidatePasteSingle;
            }

            public SerializedArrayProperty(
                  string undoKey
                , OnCopyValue onCopyValue
                , OnValidatePasteValue onValidatePasteAll
                , OnValidatePasteValue onValidatePasteSingle
                , string selectedIndexKey
                , Action<SerializedArrayProperty> onSelectedIndexChanged
            ) : this(undoKey, onCopyValue, onValidatePasteAll, onValidatePasteSingle)
            {
                _selectedIndexKey = selectedIndexKey;
                _onSelectedIndexChanged = onSelectedIndexChanged;
            }

            public SerializedProperty Property => _property;

            public int? SelectedIndex => _selectedIndex;

            public int ArraySize => _property.arraySize;

            public void Intialize(SerializedProperty property)
            {
                _property = property;
            }

            public void LoadSelectedIndex()
            {
                if (_property.arraySize < 1 || string.IsNullOrWhiteSpace(_selectedIndexKey))
                {
                    SetSelectedIndex(null);
                    return;
                }

                var str = EditorUserSettings.GetConfigValue(_selectedIndexKey);

                if (int.TryParse(str, out var value))
                {
                    SetSelectedIndex(value);
                }
                else if (_property.arraySize > 0)
                {
                    SetSelectedIndex(0);
                }
                else
                {
                    SetSelectedIndex(null);
                }
            }

            public void SetSelectedIndex(int? value)
            {
                _selectedIndex = value;
                _onSelectedIndexChanged?.Invoke(this);

                if (string.IsNullOrWhiteSpace(_selectedIndexKey))
                {
                    return;
                }

                if (value.HasValue)
                {
                    EditorUserSettings.SetConfigValue(_selectedIndexKey, value.Value.ToString());
                }
                else
                {
                    EditorUserSettings.SetConfigValue(_selectedIndexKey, string.Empty);
                }
            }

            public bool ValidateIndex(int? value)
            {
                return value.HasValue
                    && (uint)value.Value < (uint)_property.arraySize;
            }

            public bool ValidateSelectedIndex()
                => ValidateIndex(_selectedIndex);

            public SerializedProperty GetElementAt(int index)
                => _property.GetArrayElementAtIndex(index);

            public void RefreshSelectedIndex()
            {
                if (_property.arraySize > 0 && _selectedIndex.HasValue == false)
                {
                    SetSelectedIndex(0);
                }
                else if (_property.arraySize < 1 && _selectedIndex.HasValue)
                {
                    SetSelectedIndex(null);
                }
            }

            public bool TryGetAtSelectedIndex(out SerializedProperty result)
            {
                if (ValidateSelectedIndex())
                {
                    result = _property.GetArrayElementAtIndex(_selectedIndex.Value);
                    return true;
                }

                result = null;
                return false;
            }

            public bool TryIncreaseSelectedIndex()
            {
                if (ValidateSelectedIndex() == false)
                {
                    return false;
                }

                var nextIndex = _selectedIndex.Value + 1;
                var lastIndex = _property.arraySize - 1;
                SetSelectedIndex(Mathf.Min(nextIndex, lastIndex));
                return true;
            }

            public bool TryDecreaseSelectedIndex()
            {
                if (ValidateSelectedIndex() == false)
                {
                    return false;
                }

                var prevIndex = _selectedIndex.Value - 1;
                SetSelectedIndex(Mathf.Max(prevIndex, 0));
                return true;
            }

            public void CopyAll()
            {
                var property = _property;

                if (property.arraySize < 1)
                {
                    return;
                }

                var serializedObject = property.serializedObject;
                var target = serializedObject.targetObject;
                s_copiedObject = new(target);

                EditorGUIUtility.systemCopyBuffer = property.propertyPath;
            }

            public bool ValidatePasteAll()
            {
                var copiedBuffer = EditorGUIUtility.systemCopyBuffer;

                if (s_copiedObject == null || string.IsNullOrWhiteSpace(copiedBuffer))
                {
                    return false;
                }

                var copiedProperty = s_copiedObject.FindProperty(copiedBuffer);

                if (copiedProperty == null
                    || copiedProperty.isArray == false
                    || _onValidatePasteAll(copiedProperty, this) == false
                )
                {
                    return false;
                }

                return true;
            }

            public void PasteAll()
            {
                if (ValidatePasteAll() == false)
                {
                    return;
                }

                var copiedBuffer = EditorGUIUtility.systemCopyBuffer;
                var copiedProperty = s_copiedObject.FindProperty(copiedBuffer);
                var property = _property;
                var serializedObject = property.serializedObject;
                var target = serializedObject.targetObject;
                var lastIndex = property.arraySize;
                var length = copiedProperty.arraySize;
                var onCopyValue = _onCopyValue;

                Undo.RecordObject(target, $"Paste all {_undoKey}");

                property.arraySize += length;

                for (var i = 0; i < length; i++)
                {
                    var srcProp = copiedProperty.GetArrayElementAtIndex(i);
                    var destProp = property.GetArrayElementAtIndex(lastIndex + i);
                    onCopyValue(srcProp, destProp);
                }

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            public void ClearAll()
            {
                var property = _property;

                if (property.arraySize < 1)
                {
                    return;
                }

                var serializedObject = property.serializedObject;
                var target = serializedObject.targetObject;

                Undo.RecordObject(target, $"Clear {property.propertyPath}");

                property.ClearArray();
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                SetSelectedIndex(null);
            }

            public void CopySelected()
            {
                if (ValidateSelectedIndex() == false)
                {
                    return;
                }

                var index = _selectedIndex.Value;
                var property = _property.GetArrayElementAtIndex(index);
                var serializedObject = property.serializedObject;
                var target = serializedObject.targetObject;
                s_copiedObject = new(target);
                EditorGUIUtility.systemCopyBuffer = property.propertyPath;
            }

            public bool ValidatePasteSingle()
            {
                var copiedBuffer = EditorGUIUtility.systemCopyBuffer;

                if (s_copiedObject == null || string.IsNullOrWhiteSpace(copiedBuffer))
                {
                    return false;
                }

                var copiedProperty = s_copiedObject.FindProperty(copiedBuffer);

                if (copiedProperty == null
                    || copiedProperty.isArray == true
                    || _onValidatePasteSingle(copiedProperty, this) == false
                )
                {
                    return false;
                }

                return true;
            }

            public void PasteSingle()
            {
                if (ValidatePasteSingle() == false)
                {
                    return;
                }

                var copiedBuffer = EditorGUIUtility.systemCopyBuffer;
                var copiedProperty = s_copiedObject.FindProperty(copiedBuffer);
                var property = _property;
                var lastIndex = property.arraySize;
                var serializedObject = property.serializedObject;
                var target = serializedObject.targetObject;

                Undo.RecordObject(target, $"Paste single {_undoKey}");

                property.arraySize++;
                var destProp = property.GetArrayElementAtIndex(lastIndex);
                _onCopyValue(copiedProperty, destProp);

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            public void DeleteSelected()
            {
                var property = _property;
                var length = property.arraySize;
                var index = _selectedIndex ?? length - 1;

                if ((uint)index >= (uint)length)
                {
                    return;
                }

                var serializedObject = property.serializedObject;
                var target = serializedObject.targetObject;

                Undo.RecordObject(target, $"Delete selected {_undoKey} at {property.propertyPath}[{index}]");

                property.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                var newLength = length - 1;

                if (index >= newLength)
                {
                    SetSelectedIndex(newLength > 0 ? newLength - 1 : default(int?));
                }
            }
        }
    }
}

#endif
