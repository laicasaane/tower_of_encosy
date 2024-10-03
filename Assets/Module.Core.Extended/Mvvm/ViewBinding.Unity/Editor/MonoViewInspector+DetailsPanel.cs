#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using Module.Core.Editor;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private enum DetailsToolbarButton
        {
            None = 0,
            Add = 1,
            Menu = 2,
        }

        private readonly GUIContent _clearBindingsLabel = new("Clear");
        private readonly GUIContent _clearTargetsLabel = new("Clear");

        private void DrawDetailsPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            {
                var rect = EditorGUILayout.BeginVertical(_detailsHeaderStyle, GUILayout.MinHeight(30), GUILayout.Height(30));
                {
                    EditorGUILayout.Space(20f);

                    var tabBarRect = rect;
                    tabBarRect.height = 32f;
                    tabBarRect.x += 10f;
                    tabBarRect.width -= 20f;

                    _selectedDetailsTabIndex = GUI.Toolbar(tabBarRect, _selectedDetailsTabIndex, _detailsTabLabels);

                    if (ValidateSelectedBinderIndex())
                    {
                        var index = _selectedBinderIndex.Value;
                        var bindersProp = _presetBindersProp;
                        var binderProp = bindersProp.GetArrayElementAtIndex(index);
                        var bindingsProp = binderProp.FindPropertyRelative(PROP_PRESET_BINDINGS);
                        var targetsProp = binderProp.FindPropertyRelative(PROP_PRESET_TARGETS);

                        var iconWarning = _iconWarning;
                        var iconRect = tabBarRect;
                        iconRect.y += 8f;
                        iconRect.width = 18f;
                        iconRect.height = 18f;

                        if (bindingsProp.arraySize < 1)
                        {
                            iconWarning.tooltip = NO_BINDING;
                            iconRect.x = tabBarRect.x + (tabBarRect.width / 2f) - 22f;

                            GUI.Label(iconRect, iconWarning);
                        }

                        if (targetsProp.arraySize < 1)
                        {
                            iconWarning.tooltip = NO_TARGET;
                            iconRect.x = tabBarRect.x + tabBarRect.width - 22f;

                            GUI.Label(iconRect, iconWarning);
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                rect = EditorGUILayout.BeginVertical(_rootTabViewStyle, GUILayout.ExpandHeight(true));
                {
                    // Draw background
                    {
                        var newRect = rect;
                        newRect.x += 1f;
                        newRect.y += 1f;
                        newRect.width -= 2f;
                        newRect.height -= 2f;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(newRect, tex, mode, false, 0f, _contentColor, Vector4.zero, 3f);
                    }

                    if (_selectedBinderIndex.HasValue == false)
                    {
                        GUILayout.Label("No binder is selected.", _noBinderStyle);
                    }
                    else
                    {
                        var index = _selectedBinderIndex.Value;
                        var bindersLength = _presetBindersProp.arraySize;

                        if ((uint)index >= (uint)bindersLength)
                        {
                            GUILayout.Label("No binder is selected.", _noBinderStyle);
                        }
                        else
                        {
                            DrawDetailsPanel_Content(index);
                        }
                    }

                    GUILayout.Space(6f);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailsPanel_Content(int index)
        {
            var bindersProp = _presetBindersProp;
            var binderProp = bindersProp.GetArrayElementAtIndex(index);
            var bindingsProp = binderProp.FindPropertyRelative(PROP_PRESET_BINDINGS);
            var targetsProp = binderProp.FindPropertyRelative(PROP_PRESET_TARGETS);
            var tabIndex = _selectedDetailsTabIndex;

            switch (tabIndex)
            {
                case TAB_INDEX_BINDINGS:
                    DrawDetailsPanel_Bindings(bindingsProp);
                    break;

                case TAB_INDEX_TARGETS:
                    DrawDetailsPanel_Targets(targetsProp);
                    break;
            }
        }

        private void DrawDetailsPanel_Targets(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var toolbarButton = DrawDetailsPanel_ToolbarButtons(_addLabel);

            switch (toolbarButton)
            {
                case DetailsToolbarButton.Add:
                {
                    ShowTargetDropdown(_selectedBinderIndex.Value);
                    break;
                }

                case DetailsToolbarButton.Menu:
                {
                    s_binderPropRef.Prop = property;
                    s_binderPropRef.Inspector = this;

                    var menu = new GenericMenu();
                    menu.AddItem(_clearTargetsLabel, false, ClearTargetsOrBindings, s_binderPropRef);
                    menu.ShowAsContext();
                    break;
                }
            }

            var length = property.arraySize;

            if (length < 1)
            {
                EditorGUILayout.LabelField("This target list is empty.", _noBinderStyle, GUILayout.Height(30));
                return;
            }

            var indexLabel = new GUIContent();
            var indexLabelWidth = GUILayout.Width(20);
            var removeLabel = _removeLabel;
            var removeButtonStyle = _removeButtonStyle;
            var itemLabelStyle = _indexLabelStyle;
            var contentColor = _altContentColor;
            var comp = EditorGUIUtility.isProSkin ? 0 : 1;
            int? indexToRemove = null;

            for (var i = 0; i < length; i++)
            {
                var elementProp = property.GetArrayElementAtIndex(i);

                EditorGUILayout.Space(2f);
                var rect = EditorGUILayout.BeginHorizontal();
                {
                    // Draw background
                    if (i % 2 == comp)
                    {
                        var backRect = rect;
                        backRect.x -= 2f;
                        backRect.y -= 3.5f;
                        backRect.width += 4f;
                        backRect.height += 7f;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(backRect, tex, mode, false, 0f, contentColor, Vector4.zero, Vector4.zero);
                    }

                    indexLabel.text = i.ToString();
                    EditorGUILayout.LabelField(indexLabel, itemLabelStyle, indexLabelWidth);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(elementProp, GUIContent.none);
                    GUILayout.Space(23f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, $"Set value at {elementProp.propertyPath}");
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }

                    // Draw remove button
                    {
                        var buttonRect = rect;
                        buttonRect.width = 22;
                        buttonRect.height = 22;
                        buttonRect.x += rect.width - 19;

                        if (GUI.Button(buttonRect, removeLabel, removeButtonStyle))
                        {
                            indexToRemove = i;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2f);
            }

            if (indexToRemove.HasValue)
            {
                Undo.RecordObject(target, $"Remove target at index {indexToRemove}");

                property.DeleteArrayElementAtIndex(indexToRemove.Value);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }

        private void DrawDetailsPanel_Bindings(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var toolbarButton = DrawDetailsPanel_ToolbarButtons(_addLabel);

            switch (toolbarButton)
            {
                case DetailsToolbarButton.Add:
                {
                    ShowBindingDropdown(_selectedBinderIndex.Value);
                    break;
                }

                case DetailsToolbarButton.Menu:
                {
                    s_binderPropRef.Prop = property;
                    s_binderPropRef.Inspector = this;

                    var menu = new GenericMenu();
                    menu.AddItem(_clearBindingsLabel, false, ClearTargetsOrBindings, s_binderPropRef);
                    menu.ShowAsContext();
                    break;
                }
            }

            var length = property.arraySize;

            if (length < 1)
            {
                EditorGUILayout.LabelField("This binding list is empty.", _noBinderStyle, GUILayout.Height(30));
                return;
            }

            var propertyLabel = _propertyBindingLabel;
            var commandLabel = _commandBindingLabel;
            var itemLabelStyle = _itemLabelStyle;

            itemLabelStyle.CalcMinMaxWidth(propertyLabel, out _, out var maxPropWidth);
            itemLabelStyle.CalcMinMaxWidth(commandLabel, out _, out var maxCmdWidth);

            var itemLabelWidth = Mathf.Max(maxPropWidth, maxCmdWidth);
            var indexLabel = new GUIContent();
            var headerLabel = new GUIContent();
            var indexLabelWidth = GUILayout.Width(20);
            var removeLabel = _removeLabel;
            var removeButtonStyle = _removeButtonStyle;
            var indexLabelStyle = _indexLabelStyle;
            var headerLabelStyle = _headerLabelStyle;
            var contentColor = _altContentColor;
            var comp = EditorGUIUtility.isProSkin ? 0 : 1;
            int? indexToRemove = null;
            var last = length - 1;

            for (var i = 0; i < length; i++)
            {
                var elementProp = property.GetArrayElementAtIndex(i);

                if (elementProp.managedReferenceValue is not MonoBinding binding)
                {
                    continue;
                }

                var isAlternate = i % 2 == comp;
                var type = binding.GetType();
                var attrib = type.GetCustomAttribute<LabelAttribute>();

                EditorGUILayout.Space(2f);
                var rect = EditorGUILayout.BeginVertical();
                {
                    // Draw background
                    if (i % 2 == comp)
                    {
                        var backRect = rect;
                        backRect.x += 1f;
                        backRect.y -= 3.5f;
                        backRect.width += 1f;
                        backRect.height += 7f + 4f;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(backRect, tex, mode, false, 0f, contentColor, Vector4.zero, Vector4.zero);
                    }

                    var headerRect = EditorGUILayout.BeginHorizontal();
                    {
                        indexLabel.text = i.ToString();
                        EditorGUILayout.LabelField(indexLabel, indexLabelStyle, indexLabelWidth);

                        headerLabel.text = attrib?.Label ?? type.Name;
                        EditorGUILayout.LabelField(headerLabel, headerLabelStyle);
                        GUILayout.Space(23f);

                        // Draw remove button
                        {
                            var buttonRect = headerRect;
                            buttonRect.width = 22;
                            buttonRect.height = 22;
                            buttonRect.x += headerRect.width - 19;

                            if (GUI.Button(buttonRect, removeLabel, removeButtonStyle))
                            {
                                indexToRemove = i;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(6f);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginVertical();
                        {
                            var itemRect = EditorGUILayout.BeginHorizontal();
                            {
                                // Draw label
                                {
                                    var itemLabel = binding.IsCommand ? commandLabel : propertyLabel;
                                    var itemLabelRect = itemRect;
                                    itemLabelRect.width = itemLabelWidth;

                                    GUI.Label(itemLabelRect, itemLabel, itemLabelStyle);
                                }

                                GUILayout.Space(itemLabelWidth + 22f);

                                if (GUILayout.Button("A", EditorStyles.popup))
                                {

                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6f);
            }

            if (indexToRemove.HasValue)
            {
                Undo.RecordObject(target, $"Remove binding at index {indexToRemove}");

                property.DeleteArrayElementAtIndex(indexToRemove.Value);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }

        private void ShowTargetDropdown(int binderIndex)
        {
            if (TryGetTargetType(binderIndex, out var binderProp, out var targetType) == false)
            {
                return;
            }

            var rootGO = _view.gameObject;
            var menu = new TreeViewPopup(targetType.Name) {
                width = 300,
                data = binderProp,
                onApplySelectedIds = TargetMenu_AddTargets,
            };

            var tree = targetType == typeof(GameObject)
                ? (menu.Tree = new TargetGameObjectTreeView(menu.TreeViewState, rootGO))
                : (menu.Tree = new TargetComponentTreeView(menu.TreeViewState, rootGO, targetType))
                ;

            tree.ExpandAll();
            menu.Show(Event.current.mousePosition);
        }

        private void ShowBindingDropdown(int binderIndex)
        {
            if (TryGetTargetType(binderIndex, out var binderProp, out var targetType) == false)
            {
                return;
            }

            if (s_bindingMenuMap.TryGetValue(targetType, out var menu) == false)
            {
                return;
            }

            s_binderPropRef.Prop = binderProp;
            s_binderPropRef.Inspector = this;

            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.Show(Event.current.mousePosition);
        }

        private static void TargetMenu_AddTargets(object data, IList<int> selectedIds)
        {
            if (data is not SerializedProperty binderProp)
            {
                return;
            }

            if (selectedIds == null || selectedIds.Count < 1)
            {
                return;
            }

            var length = selectedIds.Count;
            var targetsProp = binderProp.FindPropertyRelative(PROP_PRESET_TARGETS);
            var serializedObject = targetsProp.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Add {length} items to {targetsProp.propertyPath}");

            targetsProp.arraySize += length;

            for (var i = 0; i < length; i++)
            {
                var elementProp = targetsProp.GetArrayElementAtIndex(i);
                elementProp.objectReferenceInstanceIDValue = selectedIds[i];
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void BindingMenu_AddBinding(object userData)
        {
            if (userData is not BindingMenuItem menuItem)
            {
                return;
            }

            var (bindingType, _, binderPropRef) = menuItem;
            var binderProp = binderPropRef.Prop;
            var bindingsProp = binderProp.FindPropertyRelative(PROP_PRESET_BINDINGS);
            var serializedObject = bindingsProp.serializedObject;
            var target = serializedObject.targetObject;
            var lastIndex = bindingsProp.arraySize;

            Undo.RecordObject(target, $"Add {bindingType} to {bindingsProp.propertyPath}");

            bindingsProp.arraySize++;

            var elementProp = bindingsProp.GetArrayElementAtIndex(lastIndex);
            elementProp.managedReferenceValue = Activator.CreateInstance(bindingType);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void ClearTargetsOrBindings(object userData)
        {
            if (userData is not BinderPropRef propRef)
            {
                return;
            }

            var property = propRef.Prop;

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
        }

        private DetailsToolbarButton DrawDetailsPanel_ToolbarButtons(GUIContent addLabel)
        {
            var result = DetailsToolbarButton.None;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(addLabel, _toolbarLeftButtonStyle, GUILayout.Height(20)))
                {
                    result = DetailsToolbarButton.Add;
                }

                var guiContentColor = GUI.contentColor;
                GUI.contentColor = _menuColor;

                if (GUILayout.Button(_menuLabel, _toolbarMenuButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    result = DetailsToolbarButton.Menu;
                }

                GUI.contentColor = guiContentColor;
            }
            EditorGUILayout.EndHorizontal();

            return result;
        }

        private bool TryGetTargetType(int binderIndex, out SerializedProperty binderProp, out Type targetType)
        {
            var bindersProp = _presetBindersProp;
            binderProp = bindersProp.GetArrayElementAtIndex(binderIndex);
            var binderType = binderProp.managedReferenceValue?.GetType();

            if (binderType == null)
            {
                EditorUtility.DisplayDialog(
                      "Invalid Target Type"
                    , "Cannot find target type for an unknown binder"
                    , "I understand"
                );

                targetType = default;
                return false;
            }

            if (s_binderToTargetTypeMap.TryGetValue(binderType, out targetType) == false)
            {
                EditorUtility.DisplayDialog(
                      "Invalid Target Type"
                    , $"Cannot find target type for the binder {binderType}!"
                    , "I understand"
                );

                targetType = default;
                return false;
            }

            if (targetType != typeof(GameObject)
                && typeof(Component).IsAssignableFrom(targetType) == false
            )
            {
                EditorUtility.DisplayDialog(
                      "Invalid Target Type"
                    , $"{targetType} is not UnityEngine.GameObject nor UnityEngine.Component!"
                    , "I understand"
                );

                targetType = default;
                return false;
            }

            return true;
        }
    }
}

#endif
