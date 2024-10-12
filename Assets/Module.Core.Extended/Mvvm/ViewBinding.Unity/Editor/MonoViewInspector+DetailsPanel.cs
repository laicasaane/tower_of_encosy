#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
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
            Remove = 2,
            Menu = 3,
        }

        private enum TargetBindingResult
        {
            Empty,
            UnknownType,
            UnknownMember,
            Valid,
        }

        private void DrawDetailsPanel(in EventData eventData)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            {
                var rect = EditorGUILayout.BeginVertical(s_detailsHeaderStyle, GUILayout.MinHeight(30), GUILayout.Height(30));
                {
                    EditorGUILayout.Space(20f);

                    var tabBarRect = rect;
                    tabBarRect.height = 32f;
                    tabBarRect.x += 10f;
                    tabBarRect.width -= 20f;

                    _selectedDetailsTabIndex = GUI.Toolbar(tabBarRect, _selectedDetailsTabIndex, s_detailsTabLabels);

                    if (_presetBindersProp.ValidateSelectedIndex())
                    {
                        var iconWarning = s_iconWarning;
                        var iconRect = tabBarRect;
                        iconRect.y += 8f;
                        iconRect.width = 18f;
                        iconRect.height = 18f;

                        if (_presetBindingsProp == null || _presetBindingsProp.ArraySize < 1)
                        {
                            iconWarning.tooltip = NO_BINDING;
                            iconRect.x = tabBarRect.x + (tabBarRect.width / 2f) - 22f;

                            GUI.Label(iconRect, iconWarning);
                        }

                        if (_presetTargetsProp == null || _presetTargetsProp.ArraySize < 1)
                        {
                            iconWarning.tooltip = NO_TARGET;
                            iconRect.x = tabBarRect.x + tabBarRect.width - 22f;

                            GUI.Label(iconRect, iconWarning);
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                rect = EditorGUILayout.BeginVertical(s_rootTabViewStyle, GUILayout.ExpandHeight(true));
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

                        GUI.DrawTexture(newRect, tex, mode, false, 0f, s_contentColor, Vector4.zero, 3f);
                    }

                    if (_presetBindersProp.SelectedIndex.HasValue == false)
                    {
                        GUILayout.Label("No binder is selected.", s_noBinderStyle);
                    }
                    else
                    {
                        var index = _presetBindersProp.SelectedIndex.Value;
                        var bindersLength = _presetBindersProp.ArraySize;

                        if ((uint)index >= (uint)bindersLength)
                        {
                            GUILayout.Label("No binder is selected.", s_noBinderStyle);
                        }
                        else
                        {
                            DrawDetailsPanel_Content(eventData, rect);
                        }
                    }

                    GUILayout.Space(6f);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailsPanel_Content(in EventData eventData, Rect sectionRect)
        {
            var tabIndex = _selectedDetailsTabIndex;

            switch (tabIndex)
            {
                case TAB_INDEX_BINDINGS:
                    DrawDetailsPanel_Bindings(eventData, sectionRect);
                    break;

                case TAB_INDEX_TARGETS:
                    DrawDetailsPanel_Targets(eventData, sectionRect);
                    break;
            }
        }

        private void DrawDetailsPanel_Bindings(in EventData eventData, Rect sectionRect)
        {
            var property = _presetBindingsProp;
            var serializedObject = property.Property.serializedObject;
            var target = serializedObject.targetObject;
            var toolbarButton = DrawDetailsPanel_ToolbarButtons(property.ArraySize);

            switch (toolbarButton)
            {
                case DetailsToolbarButton.Add:
                {
                    ShowBindingDropdown(_presetBindersProp, this);
                    break;
                }

                case DetailsToolbarButton.Remove:
                {
                    property.DeleteSelected();
                    break;
                }

                case DetailsToolbarButton.Menu:
                {
                    ShowMenuContextMenu(property);
                    break;
                }
            }

            var length = property.ArraySize;

            if (length < 1)
            {
                EditorGUILayout.LabelField("This binding list is empty.", s_noBinderStyle, GUILayout.Height(30));

                if (eventData.Type == EventType.MouseDown
                    && eventData.Button == 1
                    && sectionRect.Contains(eventData.MousePos)
                )
                {
                    ShowRightClickContextMenuEmpty(property);
                }
                return;
            }

            var propertyLabel = s_propertyBindingLabel;
            var commandLabel = s_commandBindingLabel;
            var converterLabel = s_converterLabel;
            var itemLabelStyle = s_itemLabelStyle;

            itemLabelStyle.CalcMinMaxWidth(propertyLabel, out _, out var maxPropWidth);
            itemLabelStyle.CalcMinMaxWidth(commandLabel, out _, out var maxCmdWidth);

            var itemLabelWidth = Mathf.Max(maxPropWidth, maxCmdWidth);
            var indexLabel = new GUIContent();
            var headerLabel = new GUIContent();
            var indexLabelWidth = GUILayout.Width(20);
            var indexLabelStyle = s_indexLabelStyle;
            var headerLabelStyle = s_headerLabelStyle;
            var selectedColor = s_selectedColor;
            var contentColor = s_altContentColor;
            var comp = EditorGUIUtility.isProSkin ? 0 : 1;
            var selectedIndex = property.SelectedIndex;
            var mouseDownIndex = -1;
            var mousePos = eventData.MousePos;
            var bindingProperyMap = s_bindingPropertyMap;
            var bindingCommandMap = s_bindingCommandMap;
            var contextPropertyMap = _contextPropertyMap;
            var contextCommandMap = _contextCommandMap;
            var contextType = _contextType;
            var targetLabel = new GUIContent();

            for (var i = 0; i < length; i++)
            {
                var elementProp = property.GetElementAt(i);

                if (elementProp.managedReferenceValue is not MonoBinding binding)
                {
                    continue;
                }

                var type = binding.GetType();
                var attrib = type.GetCustomAttribute<LabelAttribute>();

                EditorGUILayout.Space(2f);
                var rect = EditorGUILayout.BeginVertical();
                {
                    // Draw background and select button
                    {
                        var backRect = rect;
                        backRect.x += 1f;
                        backRect.y -= 4f;
                        backRect.width += 1f;
                        backRect.height += 7f + 4f;

                        var backColor = i % 2 == comp ? contentColor : new Color(1, 1, 1, 0f);
                        backColor = i == selectedIndex ? selectedColor : backColor;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(backRect, tex, mode, false, 0f, backColor, Vector4.zero, Vector4.zero);

                        if (GUI.Button(backRect, GUIContent.none, GUIStyle.none))
                        {
                            property.SetSelectedIndex(i);
                        }

                        if (eventData.Type == EventType.MouseDown
                            && eventData.Button == 1
                            && backRect.Contains(mousePos)
                        )
                        {
                            mouseDownIndex = i;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        indexLabel.text = i.ToString();
                        EditorGUILayout.LabelField(indexLabel, indexLabelStyle, indexLabelWidth);

                        headerLabel.text = attrib?.Label ?? type.Name;
                        EditorGUILayout.LabelField(headerLabel, headerLabelStyle);
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(6f);

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginVertical();
                        {
                            MemberMap memberMap = null;

                            if (binding.IsCommand)
                            {
                                if (bindingCommandMap.TryGetValue(type, out var commandMap))
                                {
                                    memberMap = commandMap;
                                }
                            }
                            else
                            {
                                if (bindingProperyMap.TryGetValue(type, out var propertyMap))
                                {
                                    memberMap = propertyMap;
                                }
                            }

                            if (memberMap == null || memberMap.Count < 1)
                            {
                                EditorGUILayout.HelpBox(
                                      $"There are 0 binding member.\n" +
                                      "MonoView requires that any MonoBinding must " +
                                      "contain 1 binding member (either property or command)."
                                    , MessageType.Warning
                                );
                            }
                            else if (memberMap.Count > 1)
                            {
                                EditorGUILayout.HelpBox(
                                      $"There are more than 1 binding member.\n" +
                                      "MonoView requires that any MonoBinding must " +
                                      "contain only 1 binding member (either property or command)."
                                    , MessageType.Warning
                                );
                            }
                            else
                            {
                                var (methodName, paramType) = memberMap.First();
                                var bindingPrefix = binding.IsCommand ? "_bindingCommandFor" : "_bindingFieldFor";
                                var bindingSuffix = binding.IsCommand
                                    ? ".<TargetCommandName>k__BackingField"
                                    : ".<TargetPropertyName>k__BackingField";

                                var targetPropertyPath = $"{bindingPrefix}{methodName}{bindingSuffix}";
                                var converterPath = $"_converterFor{methodName}.<Adapter>k__BackingField";
                                var targetPropertyProp = elementProp.FindPropertyRelative(targetPropertyPath);
                                var converterProp = elementProp.FindPropertyRelative(converterPath);

                                if (targetPropertyProp == null)
                                {
                                    EditorGUILayout.HelpBox(
                                          $"Cannot find serialized field `{bindingPrefix}{methodName}`."
                                        , MessageType.Error
                                    );
                                }
                                else
                                {
                                    var result = binding.IsCommand
                                        ? MakeTargetCommandLabel(contextType, contextCommandMap, targetPropertyProp, targetLabel)
                                        : MakeTargetPropertyLabel(contextType, contextPropertyMap, targetPropertyProp, targetLabel)
                                        ;

                                    var memberRect = EditorGUILayout.BeginHorizontal();
                                    {
                                        // Draw label
                                        {
                                            var itemLabel = binding.IsCommand ? commandLabel : propertyLabel;
                                            var itemLabelRect = memberRect;
                                            itemLabelRect.width = itemLabelWidth;

                                            GUI.Label(itemLabelRect, itemLabel, itemLabelStyle);
                                        }

                                        GUILayout.Space(itemLabelWidth + 22f);

                                        if (GUILayout.Button(targetLabel, EditorStyles.popup))
                                        {

                                        }

                                        if (result != TargetBindingResult.Valid)
                                        {
                                            GUILayout.Space(20f);

                                            var iconRect = memberRect;
                                            iconRect.x += iconRect.width - 16f;
                                            iconRect.width = 16f;
                                            iconRect.height = 16f;

                                            GUI.DrawTexture(iconRect, s_iconWarning.image);
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    var converterRect = EditorGUILayout.BeginHorizontal();
                                    if (converterProp != null)
                                    {
                                        // Draw label
                                        {
                                            var itemLabelRect = converterRect;
                                            itemLabelRect.width = itemLabelWidth;

                                            GUI.Label(itemLabelRect, converterLabel, itemLabelStyle);

                                            GUILayout.Space(itemLabelWidth + 22f);

                                            if (GUILayout.Button("A", EditorStyles.popup))
                                            {

                                            }
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6f);
            }

            if (mouseDownIndex >= 0)
            {
                ShowRightClickContextMenu(property, mouseDownIndex);
            }
        }

        private void DrawDetailsPanel_Targets(in EventData eventData, Rect sectionRect)
        {
            var property = _presetTargetsProp;
            var serializedObject = property.Property.serializedObject;
            var target = serializedObject.targetObject;
            var toolbarButton = DrawDetailsPanel_ToolbarButtons(property.ArraySize);

            switch (toolbarButton)
            {
                case DetailsToolbarButton.Add:
                {
                    ShowTargetDropdown(_presetBindersProp, this);
                    break;
                }

                case DetailsToolbarButton.Remove:
                {
                    property.DeleteSelected();
                    break;
                }

                case DetailsToolbarButton.Menu:
                {
                    ShowMenuContextMenu(property);
                    break;
                }
            }

            var length = property.ArraySize;

            if (length < 1)
            {
                EditorGUILayout.LabelField("This target list is empty.", s_noBinderStyle, GUILayout.Height(30));

                if (eventData.Type == EventType.MouseDown
                    && eventData.Button == 1
                    && sectionRect.Contains(eventData.MousePos)
                )
                {
                    ShowRightClickContextMenuEmpty(property);
                }
                return;
            }

            var indexLabel = new GUIContent();
            var indexLabelWidth = GUILayout.Width(30);
            var itemLabelStyle = s_indexLabelStyle;
            var selectedColor = s_selectedColor;
            var contentColor = s_altContentColor;
            var comp = EditorGUIUtility.isProSkin ? 0 : 1;
            var selectedIndex = property.SelectedIndex;
            var mouseDownIndex = -1;
            var mousePos = eventData.MousePos;

            for (var i = 0; i < length; i++)
            {
                var elementProp = property.GetElementAt(i);

                EditorGUILayout.Space(2f);
                var rect = EditorGUILayout.BeginHorizontal();
                {
                    // Draw background and select button
                    {
                        var backRect = rect;
                        backRect.x -= 2f;
                        backRect.y -= 4f;
                        backRect.width += 4f;
                        backRect.height += 7f;

                        var backColor = i % 2 == comp ? contentColor : new Color(1, 1, 1, 0f);
                        backColor = i == selectedIndex ? selectedColor : backColor;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(backRect, tex, mode, false, 0f, backColor, Vector4.zero, Vector4.zero);

                        if (GUI.Button(backRect, GUIContent.none, GUIStyle.none))
                        {
                            property.SetSelectedIndex(i);
                        }

                        if (eventData.Type == EventType.MouseDown
                            && eventData.Button == 1
                            && backRect.Contains(mousePos)
                        )
                        {
                            mouseDownIndex = i;
                        }
                    }

                    indexLabel.text = i.ToString();
                    EditorGUILayout.LabelField(indexLabel, itemLabelStyle, indexLabelWidth);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(elementProp, GUIContent.none);
                    GUILayout.Space(4f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, $"Set value at {elementProp.propertyPath}");
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2f);
            }

            if (mouseDownIndex >= 0)
            {
                ShowRightClickContextMenu(property, mouseDownIndex);
            }
        }

        private static void ShowBindingDropdown(SerializedArrayProperty bindersProp, MonoViewInspector inspector)
        {
            if (TryGetTargetType(bindersProp, out var binderProp, out var targetType) == false)
            {
                return;
            }

            if (s_targetTypeToBindingMenuMap.TryGetValue(targetType, out var menu) == false)
            {
                return;
            }

            s_binderPropRef.Prop = binderProp;
            s_binderPropRef.Inspector = inspector;

            menu.width = 250;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.Show(Event.current.mousePosition);
        }

        private static void ShowTargetDropdown(SerializedArrayProperty bindersProp, MonoViewInspector inspector)
        {
            if (TryGetTargetType(bindersProp, out var binderProp, out var targetType) == false)
            {
                return;
            }

            var rootGO = inspector._view.gameObject;
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

        private static void ShowMenuContextMenu(SerializedArrayProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(s_copyAllLabel, false, Menu_OnCopyAll, property);

            if (property.ValidatePasteAll())
            {
                menu.AddItem(s_pasteAllLabel, false, Menu_OnPasteAll, property);
            }
            else
            {
                menu.AddDisabledItem(s_pasteAllLabel);
            }

            menu.AddItem(s_clearAllLabel, false, Menu_OnClearAll, property);
            menu.ShowAsContext();
        }

        private static void ShowRightClickContextMenuEmpty(SerializedArrayProperty property)
        {
            var menu = new GenericMenu();

            if (property.ValidatePasteSingle())
            {
                menu.AddItem(s_pasteItemLabel, false, Menu_OnPasteSingle, property);
            }
            else
            {
                menu.AddDisabledItem(s_pasteItemLabel);
            }

            menu.ShowAsContext();
        }

        private static void ShowRightClickContextMenu(SerializedArrayProperty property, int mouseDownIndex)
        {
            property.SetSelectedIndex(mouseDownIndex);

            var menu = new GenericMenu();
            menu.AddItem(s_copyItemLabel, false, Menu_OnCopySelected, property);

            if (property.ValidatePasteSingle())
            {
                menu.AddItem(s_pasteItemLabel, false, Menu_OnPasteSingle, property);
            }
            else
            {
                menu.AddDisabledItem(s_pasteItemLabel);
            }

            menu.AddItem(s_deleteItemLabel, false, Menu_OnDeleteSelected, property);
            menu.ShowAsContext();
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
            if (userData is not MenuItemBinding menuItem)
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

        private static DetailsToolbarButton DrawDetailsPanel_ToolbarButtons(int arraySize)
        {
            var result = DetailsToolbarButton.None;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(s_addLabel, s_toolbarLeftButtonStyle, GUILayout.Height(20)))
                {
                    result = DetailsToolbarButton.Add;
                }

                var guiEnabled = GUI.enabled;
                GUI.enabled = arraySize > 0;

                if (GUILayout.Button(s_removeSelectedLabel, s_toolbarMidButtonStyle, GUILayout.Height(20)))
                {
                    result = DetailsToolbarButton.Remove;
                }

                GUI.enabled = guiEnabled;

                var guiContentColor = GUI.contentColor;
                GUI.contentColor = s_menuColor;

                if (GUILayout.Button(s_menuLabel, s_toolbarMenuButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    result = DetailsToolbarButton.Menu;
                }

                GUI.contentColor = guiContentColor;
            }
            EditorGUILayout.EndHorizontal();

            return result;
        }

        private static bool TryGetTargetType(
              SerializedArrayProperty bindersProp
            , out SerializedProperty binderProp
            , out Type targetType
        )
        {
            binderProp = bindersProp.GetElementAt(bindersProp.SelectedIndex.Value);
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

        private static TargetBindingResult MakeTargetPropertyLabel(
              Type contextType
            , Dictionary<string, Type> contextPropertyMap
            , SerializedProperty property
            , GUIContent label
        )
        {
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                label.text = "< empty >";
                label.tooltip = "No property is selected";
                return TargetBindingResult.Empty;
            }

            var candidate = property.stringValue;

            if (contextType == null || contextPropertyMap.TryGetValue(candidate, out var propertyType) == false)
            {
                label.text = $"< invalid > {candidate}";

                if (contextType == null)
                {
                    label.tooltip = "The type of the context object is unknown.";
                    return TargetBindingResult.UnknownType;
                }
                else
                {
                    label.tooltip = $"{contextType.FullName} does not contain a `{candidate}` property.";
                    return TargetBindingResult.UnknownMember;
                }
            }
            else
            {
                var returnTypeName = propertyType.GetFriendlyName();
                label.text = $"{candidate} : {returnTypeName}";
                label.tooltip = $"property {candidate} : {returnTypeName}\n" +
                    $"class {contextType.Name}\nnamespace {contextType.Namespace}";

                return TargetBindingResult.Valid;
            }
        }

        private static TargetBindingResult MakeTargetCommandLabel(
              Type contextType
            , Dictionary<string, Type> contextCommandMap
            , SerializedProperty property
            , GUIContent label
        )
        {
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                label.text = "< empty >";
                label.tooltip = "No command is selected";
                return TargetBindingResult.Empty;
            }

            var candidate = property.stringValue;

            if (contextType == null || contextCommandMap.TryGetValue(candidate, out var propertyType) == false)
            {
                label.text = $"< invalid > {candidate}";

                if (contextType == null)
                {
                    label.tooltip = "The type of the context object is unknown.";
                    return TargetBindingResult.UnknownType;
                }
                else
                {
                    label.tooltip = $"{contextType.FullName} does not contain a `{candidate}` command.";
                    return TargetBindingResult.UnknownMember;
                }
            }
            else
            {
                var returnTypeName = propertyType.GetFriendlyName();
                label.text = $"{candidate} : {returnTypeName}";
                label.tooltip = $"command {candidate} : {returnTypeName}\n" +
                    $"class {contextType.Name}\nnamespace {contextType.Namespace}";

                return TargetBindingResult.Valid;
            }
        }
    }
}

#endif
