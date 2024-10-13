#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Module.Core.Editor;
using Module.Core.Extended.Mvvm.ViewBinding.Adapters.Unity;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Logging;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Mvvm.ViewBinding.Adapters;
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
            var subHeaderLabel = new GUIContent();
            var indexLabelWidth = GUILayout.Width(20);
            var indexLabelStyle = s_indexLabelStyle;
            var headerLabelStyle = s_headerLabelStyle;
            var subHeaderLabelStyle = s_subHeaderLabelStyle;
            var popupStyle = s_popupStyle;
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
            var adapterLabel = new GUIContent();

            for (var i = 0; i < length; i++)
            {
                var elementProp = property.GetElementAt(i);

                if (elementProp.managedReferenceValue is not MonoBinding binding)
                {
                    continue;
                }

                var isCommand = binding.IsCommand;
                var type = binding.GetType();
                var attrib = type.GetCustomAttribute<LabelAttribute>();

                MemberMap memberMap = null;
                string bindingMethodName = null;
                Type bindingParamType = null;

                if (isCommand)
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

                if (memberMap != null && memberMap.Count == 1)
                {
                    (bindingMethodName, bindingParamType) = memberMap.First();
                }

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

                        if (eventData.Type == EventType.MouseDown
                            && backRect.Contains(mousePos)
                        )
                        {
                            if (eventData.Button == 0)
                            {
                                property.SetSelectedIndex(i);
                            }
                            else if (eventData.Button == 1)
                            {
                                mouseDownIndex = i;
                            }
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

                    EditorGUILayout.BeginHorizontal();
                    if (bindingParamType != null)
                    {
                        EditorGUILayout.LabelField(GUIContent.none, indexLabelStyle, indexLabelWidth);

                        subHeaderLabel.text = bindingParamType.GetFriendlyName();
                        subHeaderLabel.tooltip = bindingParamType.FullName;
                        EditorGUILayout.LabelField(subHeaderLabel, subHeaderLabelStyle);
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(6f);

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginVertical();
                        {
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
                                var contextMemberMap = isCommand ? contextCommandMap : contextPropertyMap;
                                var bindingPrefix = isCommand ? "_bindingCommandFor" : "_bindingFieldFor";
                                var bindingSuffix = isCommand
                                    ? ".<TargetCommandName>k__BackingField"
                                    : ".<TargetPropertyName>k__BackingField";

                                var targetPropertyPath = $"{bindingPrefix}{bindingMethodName}{bindingSuffix}";
                                var adapterPath = $"_converterFor{bindingMethodName}.<Adapter>k__BackingField";
                                var targetMemberProp = elementProp.FindPropertyRelative(targetPropertyPath);
                                var adapterProp = elementProp.FindPropertyRelative(adapterPath);

                                if (targetMemberProp == null)
                                {
                                    EditorGUILayout.HelpBox(
                                          $"Cannot find serialized field `{bindingPrefix}{bindingMethodName}`."
                                        , MessageType.Error
                                    );
                                }
                                else
                                {
                                    var result = GetTargetMemberData(
                                          contextType
                                        , contextMemberMap
                                        , targetMemberProp
                                        , targetLabel
                                        , binding.IsCommand
                                        , out var targetMemberType
                                    );

                                    var memberRect = EditorGUILayout.BeginHorizontal();
                                    {
                                        // Draw label
                                        {
                                            var itemLabel = isCommand ? commandLabel : propertyLabel;
                                            var itemLabelRect = memberRect;
                                            itemLabelRect.width = itemLabelWidth;

                                            GUI.Label(itemLabelRect, itemLabel, itemLabelStyle);
                                        }

                                        GUILayout.Space(itemLabelWidth + 22f);

                                        if (GUILayout.Button(targetLabel, popupStyle))
                                        {
                                            if (isCommand)
                                            {
                                                ShowBindingCommandMenu(
                                                      targetMemberProp
                                                    , contextType
                                                    , targetMemberProp.stringValue
                                                    , contextMemberMap
                                                );
                                            }
                                            else
                                            {
                                                ShowBindingPropertyMenu(
                                                      targetMemberProp
                                                    , adapterProp
                                                    , contextType
                                                    , bindingParamType
                                                    , targetMemberProp.stringValue
                                                    , contextMemberMap
                                                );
                                            }
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

                                    if (adapterProp != null)
                                    {
                                        GetAdapterData(
                                              adapterProp
                                            , adapterLabel
                                            , out var adapterType
                                            , out var scriptableAdapter
                                            , out var compositeAdapter
                                        );

                                        var converterRect = EditorGUILayout.BeginHorizontal();
                                        {
                                            // Draw label
                                            {
                                                var itemLabelRect = converterRect;
                                                itemLabelRect.width = itemLabelWidth;

                                                GUI.Label(itemLabelRect, converterLabel, itemLabelStyle);
                                            }

                                            GUILayout.Space(itemLabelWidth + 22f);

                                            if (GUILayout.Button(adapterLabel, popupStyle))
                                            {
                                                ShowAdapterPropertyMenu(
                                                      bindingParamType
                                                    , targetMemberType
                                                    , adapterProp
                                                    , adapterType
                                                );
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        if (scriptableAdapter != null)
                                        {
                                            EditorGUILayout.HelpBox(
                                                  "Scriptable Adapter is not yet supported."
                                                , MessageType.Warning
                                            );
                                        }

                                        if (compositeAdapter != null)
                                        {
                                            EditorGUILayout.HelpBox(
                                                  "Composite Adapter is not yet supported."
                                                , MessageType.Warning
                                            );
                                        }
                                    }
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

        private static TargetBindingResult GetTargetMemberData(
              Type contextType
            , Dictionary<string, Type> contextMemberMap
            , SerializedProperty memberProp
            , GUIContent label
            , bool isCommand
            , out Type targetMemberType
        )
        {
            var word = isCommand ? "command" : "property";

            if (string.IsNullOrWhiteSpace(memberProp.stringValue))
            {
                targetMemberType = null;
                label.text = "< None >";
                label.tooltip = $"No {word} is selected";
                return TargetBindingResult.Empty;
            }

            var candidate = memberProp.stringValue;

            if (contextType == null || contextMemberMap.TryGetValue(candidate, out var memberType) == false)
            {
                targetMemberType = null;
                label.text = $"< invalid > {candidate}";

                if (contextType == null)
                {
                    label.tooltip = "The type of the context object is unknown.";
                    return TargetBindingResult.UnknownType;
                }
                else
                {
                    label.tooltip = $"{contextType.FullName} does not contain a `{candidate}` {word}.";
                    return TargetBindingResult.UnknownMember;
                }
            }
            else
            {
                targetMemberType = memberType;
                var memberTypeName = memberType.GetFriendlyName();
                var candidateName = ObjectNames.NicifyVariableName(candidate);

                label.text = isCommand
                    ? $"<b>{candidateName}</b> ( {memberTypeName} )"
                    : $"<b>{candidateName}</b> : {memberTypeName}";

                label.tooltip = $"{word} {candidateName} : {memberTypeName}\n" +
                    $"class {contextType.Name}\n" +
                    $"namespace {contextType.Namespace}";

                return TargetBindingResult.Valid;
            }
        }

        private static void ShowBindingPropertyMenu(
              SerializedProperty targetPropertyProp
            , SerializedProperty adapterProp
            , Type contextType
            , Type bindingParamType
            , string targetPropertyName
            , Dictionary<string, Type> targetPropertyMap
        )
        {
            if (contextType == null)
            {
                EditorUtility.DisplayDialog(
                      "Observable Context"
                    , "No valid observable context is selected!"
                    , "I understand"
                );
                return;
            }

            var root = new MenuItemNode();
            var menu = new GenericMenuPopup(root, "Properties");

            {
                var node = root.CreateNode("< None >");
                node.func2 = RemoveBindingProperty;
                node.on = false;
                node.userData = (targetPropertyProp, adapterProp);
            }

            foreach (var (propName, propType) in targetPropertyMap)
            {
                var node = root.CreateNode(
                      $"<b>{ObjectNames.NicifyVariableName(propName)}</b> : {propType.GetFriendlyName()}"
                    , propType.FullName
                );

                node.func2 = SetBindingProperty;
                node.on = propName == targetPropertyName;
                node.userData = (targetPropertyProp, adapterProp, bindingParamType, propName, propType);
            }

            if (targetPropertyMap.Count < 1)
            {
                root.CreateNode($"{contextType.GetFriendlyName()} contains no observable property!", contextType.FullName);
            }

            menu.width = 250;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.showTooltip = true;
            menu.Show(Event.current.mousePosition);
        }

        private static void RemoveBindingProperty(object param)
        {
            if (param is not (
                  SerializedProperty targetPropertyProp
                , SerializedProperty adapterProp
            ))
            {
                return;
            }

            var serializedObject = targetPropertyProp.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Remove {targetPropertyProp.propertyPath}");

            targetPropertyProp.stringValue = string.Empty;
            adapterProp.managedReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void SetBindingProperty(object param)
        {
            {
                if (param is (
                      SerializedProperty targetPropertyProp
                    , SerializedProperty adapterProp
                    , Type bindingParamType
                    , string selectedPropertyName
                    , Type selectedPropertyType
                ))
                {
                    var serializedObject = targetPropertyProp.serializedObject;
                    var target = serializedObject.targetObject;

                    Undo.RecordObject(target, $"Set {targetPropertyProp.propertyPath}");

                    targetPropertyProp.stringValue = selectedPropertyName;

                    if (TryCreateDefaultAdapter(selectedPropertyType, bindingParamType, out var adapter))
                    {
                        adapterProp.managedReferenceValue = adapter;
                    }

                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    return;
                }
            }

            {
                if (param is (
                      SerializedProperty targetPropertyProp
                    , _
                    , _
                    , string selectedPropName
                    , _
                ))
                {
                    var serializedObject = targetPropertyProp.serializedObject;
                    var target = serializedObject.targetObject;

                    Undo.RecordObject(target, $"Set {targetPropertyProp.propertyPath}");

                    targetPropertyProp.stringValue = selectedPropName;
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    return;
                }
            }
        }

        private static void ShowBindingCommandMenu(
              SerializedProperty targetCommandProp
            , Type contextType
            , string targetCommandName
            , Dictionary<string, Type> targetCommandMap
        )
        {
            if (contextType == null)
            {
                EditorUtility.DisplayDialog(
                      "Observable Context"
                    , "No valid observable context is selected!"
                    , "I understand"
                );
                return;
            }

            var root = new MenuItemNode();
            var menu = new GenericMenuPopup(root, "Properties");

            {
                var node = root.CreateNode("< None >");
                node.func2 = RemoveBindingProperty;
                node.on = false;
                node.userData = targetCommandProp;
            }

            foreach (var (commandName, commandType) in targetCommandMap)
            {
                var label = commandType == null
                    ? commandName
                    : $"<b>{ObjectNames.NicifyVariableName(commandName)}</b> ( {commandType.GetFriendlyName()} )";

                var node = root.CreateNode(label, commandType.FullName);
                node.func2 = SetBindingCommand;
                node.on = commandName == targetCommandName;
                node.userData = (targetCommandProp, commandName);
            }

            if (targetCommandMap.Count < 1)
            {
                root.CreateNode($"{contextType.GetFriendlyName()} contains no observable command!", contextType.FullName);
            }

            menu.width = 250;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.showTooltip = true;
            menu.Show(Event.current.mousePosition);
        }

        private static void RemoveBindingCommand(object param)
        {
            if (param is not SerializedProperty targetCommandProp)
            {
                return;
            }

            var serializedObject = targetCommandProp.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Remove {targetCommandProp.propertyPath}");

            targetCommandProp.stringValue = string.Empty;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void SetBindingCommand(object param)
        {
            if (param is not (
                  SerializedProperty targetCommandProp
                , string selectedCommandName
            ))
            {
                return;
            }

            var serializedObject = targetCommandProp.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Set {targetCommandProp.propertyPath}");

            targetCommandProp.stringValue = selectedCommandName;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static bool TryCreateDefaultAdapter(Type sourceType, Type destType, out IAdapter adapter)
        {
            adapter = null;

            if (s_adapterMap.TryGetValue(destType, out var map)
                && map.TryGetValue(sourceType, out var adapterTypes)
            )
            {
                try
                {
                    var sortedTypes = adapterTypes.OrderBy(x => {
                        var attrib = x.GetCustomAttribute<AdapterAttribute>();
                        return attrib?.Order ?? AdapterAttribute.DEFAULT_ORDER;
                    });

                    var adapterType = sortedTypes.FirstOrDefault();

                    if (adapterType != null)
                    {
                        adapter = Activator.CreateInstance(adapterType) as IAdapter;
                    }
                }
                catch
                {
                    adapter = null;
                }
            }

            return adapter != null;
        }

        private static void GetAdapterData(
              SerializedProperty adapterProp
            , GUIContent label
            , out Type adapterType
            , out ScriptableAdapter scriptableAdapter
            , out CompositeAdapter compositeAdapter
        )
        {
            if (adapterProp?.managedReferenceValue is IAdapter adapter)
            {
                adapterType = adapter.GetType();

                var keyword = adapterType.IsValueType ? "struct" : "class";
                var adapterLabelAttrib = adapterType.GetCustomAttribute<LabelAttribute>();

                label.text = adapterLabelAttrib?.Label ?? adapterType.Name;
                label.tooltip = $"{keyword} <b>{adapterType.Name}</b>\nnamespace {adapterType.Namespace}";

                scriptableAdapter = adapter as ScriptableAdapter;
                compositeAdapter = adapter as CompositeAdapter;
            }
            else
            {
                adapterType = null;
                scriptableAdapter = null;
                compositeAdapter = null;
                label.text = "< Undefined >";
                label.tooltip = string.Empty;
            }
        }

        private static void ShowAdapterPropertyMenu(
              Type bindingParamType
            , Type targetMemberType
            , SerializedProperty adapterProp
            , Type adapterTypeSaved
        )
        {
            var adapterTypes = new List<Type> {
                typeof(CompositeAdapter),
                typeof(ScriptableAdapter)
            };

            try
            {
                if (s_adapterMap.TryGetValue(bindingParamType, out var map)
                    && map.TryGetValue(targetMemberType, out var types)
                )
                {
                    var orderedTypes = types.OrderBy(x => {
                        var attrib = x.GetCustomAttribute<AdapterAttribute>();
                        return attrib?.Order ?? AdapterAttribute.DEFAULT_ORDER;
                    });

                    adapterTypes.AddRange(orderedTypes);
                }
            }
            catch { }

            var root = new MenuItemNode();
            var menu = new GenericMenuPopup(root, "Adapters");

            {
                var node = root.CreateNode("< None >");
                node.on = false;
                node.func2 = RemoveAdapterProperty;
                node.userData = adapterProp;
            }

            AddAdapterTypesToMenu(
                  root
                , adapterProp
                , adapterTypeSaved
                , adapterTypes
                , string.Empty
            );

            AddAdapterTypesToMenu(
                  root
                , adapterProp
                , adapterTypeSaved
                , GetOtherAdapterTypesExcludeSourceType(targetMemberType)
                , "Other"
            );

            menu.width = 450;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.showTooltip = true;
            menu.Show(Event.current.mousePosition);
        }

        private static void AddAdapterTypesToMenu(
              MenuItemNode root
            , SerializedProperty adapterProp
            , Type adapterTypeSaved
            , List<Type> adapterTypes
            , string directoryPrefix
        )
        {
            foreach (var adapterType in adapterTypes)
            {
                var adapterAttrib = adapterType.GetCustomAttribute<AdapterAttribute>();
                var labelAttrib = adapterType.GetCustomAttribute<LabelAttribute>();
                var labelText = labelAttrib?.Label ?? adapterType.Name;
                var keyword = adapterType.IsValueType ? "struct" : "class";
                var tooltip = $"{keyword} <b>{adapterType.Name}</b>\nnamespace {adapterType.Namespace}";
                var directory = labelAttrib?.Directory ?? adapterType.Namespace;

                var node = string.IsNullOrWhiteSpace(directoryPrefix)
                    ? root
                    : root.GetOrCreateNode(directoryPrefix);

                node = string.IsNullOrWhiteSpace(directory)
                    ? node
                    : node.GetOrCreateNode(directory);

                if (adapterAttrib?.DestinationType is Type destinationType)
                {
                    node = node.GetOrCreateNode(destinationType.GetFriendlyName(), destinationType.FullName);
                }

                node = node.CreateNode(labelText, tooltip);
                node.on = adapterType == adapterTypeSaved;
                node.func2 = SetAdapterProperty;
                node.userData = (adapterProp, adapterType);
            }
        }

        private static void RemoveAdapterProperty(object param)
        {
            if (param is not SerializedProperty adapterProp)
            {
                return;
            }

            var serializedObject = adapterProp.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Remove {adapterProp.propertyPath}");

            adapterProp.managedReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void SetAdapterProperty(object param)
        {
            if (param is not (
                  SerializedProperty adapterProp
                , Type selectedAdapterType
            ))
            {
                return;
            }

            var serializedObject = adapterProp.serializedObject;
            var target = serializedObject.targetObject;

            try
            {
                Undo.RecordObject(target, $"Set {adapterProp.propertyPath}");

                adapterProp.managedReferenceValue = Activator.CreateInstance(selectedAdapterType);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
            catch (Exception ex)
            {
                DevLoggerAPI.LogException(target, ex);
            }
        }
    }
}

#endif
