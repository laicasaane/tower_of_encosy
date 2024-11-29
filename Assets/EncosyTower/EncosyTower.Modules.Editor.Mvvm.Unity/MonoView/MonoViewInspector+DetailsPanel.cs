#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Mvvm.ViewBinding.Adapters;
using EncosyTower.Modules.Mvvm.ViewBinding.Adapters.Unity;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private enum DetailsToolbarButton
        {
            None = 0,
            Add = 1,
            Remove = 2,
            Menu = 3,
            SimpleAdd = 4,
        }

        private enum TargetBindingResult
        {
            Empty,
            UnknownType,
            UnknownMember,
            Valid,
        }

        private enum TargetTypeResult
        {
            Success,
            UnknownBinder,
            NotFoundForBinder,
            NotGameObjectOrComponent,
        }

        private enum EventResult
        {
            None,
            Consume,
        }

        private Action<Memory<UnityEngine.Object>> _onDropCreateTargets;

        private void DrawDetailsPanels(in EventData eventData)
        {
            var eventResult = EventResult.None;

            EditorGUILayout.BeginVertical();
            {
                var bindersProp = _presetBindersProp;

                if (bindersProp.SelectedIndex.HasValue == false)
                {
                    DrawDetailsPanel_NoBinderSelected();
                }
                else
                {
                    var index = bindersProp.SelectedIndex.Value;
                    var bindersLength = bindersProp.ArraySize;

                    if ((uint)index >= (uint)bindersLength)
                    {
                        DrawDetailsPanel_NoBinderSelected();
                    }
                    else
                    {
                        var result = TryGetTargetType(
                              bindersProp
                            , out var binderProp
                            , out var binderType
                            , out var targetType
                        );

                        if (result == TargetTypeResult.Success)
                        {
                            eventResult = DrawDetailsPanel_Targets(eventData, binderProp, targetType);

                            GUILayout.Space(4f);

                            if (eventResult == EventResult.None)
                            {
                                eventResult = DrawDetailsPanel_Bindings(eventData, binderProp, targetType);
                            }
                        }
                        else
                        {
                            DrawDetailsPanel_InvalidTargetType(result, binderType, targetType);
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (eventResult == EventResult.Consume)
            {
                Event.current.Use();
            }
        }

        private void DrawDetailsPanel_NoBinderSelected()
        {
            EditorGUILayout.BeginVertical(s_rootTabViewStyle);
            {
                GUILayout.Label("No binder is selected.", s_noBinderStyle);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailsPanel_InvalidTargetType(TargetTypeResult result, Type binderType, Type targetType)
        {
            EditorGUILayout.BeginVertical(s_rootTabViewStyle);
            {
                GUILayout.Label(GetMessage(result, binderType, targetType), s_noBinderStyle);
            }
            EditorGUILayout.EndVertical();
        }

        private EventResult DrawDetailsPanel_Targets(
              in EventData eventData
            , SerializedProperty binderProp
            , Type targetType
        )
        {
            var eventResult = EventResult.None;

            EditorGUILayout.BeginVertical(s_rootTabViewStyle);
            {
                var targetsProp = _presetTargetsProp.Property;
                var layoutWidth = GUILayout.ExpandWidth(true);

                var headerRect = EditorGUILayout.BeginHorizontal(layoutWidth, GUILayout.Height(30f));
                {
                    EditorGUILayout.Space(30f);
                    DrawPanelHeaderFoldout(headerRect, targetsProp, s_targetsLabel, new Rect(0f, 0f, 0f, -2f));
                }
                EditorGUILayout.EndHorizontal();

                if (_presetTargetsProp == null || _presetTargetsProp.ArraySize < 1)
                {
                    var iconRect = headerRect;
                    iconRect.x += headerRect.width - 22f;
                    iconRect.y += 6f;
                    iconRect.width = 18f;
                    iconRect.height = 18f;

                    var iconWarning = s_iconWarning;
                    iconWarning.tooltip = NO_BINDING;

                    GUI.Label(iconRect, iconWarning);
                }

                // Draw drop area for Targets
                if (_presetTargetsProp != null)
                {
                    DrawDragDropArea(headerRect, eventData, _onDropCreateTargets ??= OnDropCreateTargets);
                }

                if (targetsProp.isExpanded)
                {
                    var contentRect = EditorGUILayout.BeginVertical(layoutWidth);
                    {
                        eventResult = DrawDetailsPanel_Targets_Content(eventData, contentRect, binderProp, targetType);
                        GUILayout.Space(6f);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();

            return eventResult;
        }

        private EventResult DrawDetailsPanel_Targets_Content(
              in EventData eventData
            , in Rect sectionRect
            , SerializedProperty binderProp
            , Type targetType
        )
        {
            var property = _presetTargetsProp;
            var serializedObject = property.Property.serializedObject;
            var target = serializedObject.targetObject;
            var toolbarButton = DrawDetailsPanel_ToolbarButtons(property.ArraySize, true);

            switch (toolbarButton)
            {
                case DetailsToolbarButton.SimpleAdd:
                {
                    AddTargetToSelectedBinder(binderProp);
                    break;
                }

                case DetailsToolbarButton.Add:
                {
                    ShowTargetDropdown(this, binderProp, targetType);
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

                if (eventData is { Type: EventType.MouseDown, Button: 1 }
                    && sectionRect.Contains(eventData.MousePos)
                )
                {
                    ShowRightClickContextMenuEmpty(property);
                }
                return EventResult.None;
            }

            var indexLabel = new GUIContent();
            var indexLabelWidth = GUILayout.Width(30);
            var itemLabelStyle = s_indexLabelStyle;
            var selectedColor = s_selectedColor;
            var backColor = s_backColor;
            var altBackColor = s_altBackColor;
            var selectedIndex = property.SelectedIndex;
            var mouseDownIndex = -1;
            var mousePos = eventData.MousePos;

            for (var i = 0; i < length; i++)
            {
                var elementProp = property.GetElementAt(i);

                EditorGUILayout.Space(2f);
                var rect = EditorGUILayout.BeginHorizontal();
                {
                    var backRect = rect;
                    backRect.x -= 2f;
                    backRect.y -= 4f;
                    backRect.width += 4f;
                    backRect.height += 7f;

                    // Draw background and select button
                    {
                        var bgColor = i % 2 == 0 ? altBackColor : backColor;
                        bgColor = i == selectedIndex ? selectedColor : bgColor;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(backRect, tex, mode, false, 0f, bgColor, Vector4.zero, Vector4.zero);
                    }

                    indexLabel.text = i.ToString();
                    EditorGUILayout.LabelField(indexLabel, itemLabelStyle, indexLabelWidth);

                    {
                        var labelRect = rect;
                        labelRect.width = 30;

                        if (eventData is { Type: EventType.MouseDown, Button: 1 }
                            && labelRect.Contains(mousePos)
                        )
                        {
                            mouseDownIndex = i;
                        }
                    }

                    var oldValue = elementProp.objectReferenceValue;
                    var newValue = EditorGUILayout.ObjectField(oldValue, targetType, true);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject(target, $"Set value at {elementProp.propertyPath}");

                        elementProp.objectReferenceValue = newValue;
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }

                    GUILayout.Space(4f);

                    if (eventData is { Type: EventType.MouseDown, Button: 0 }
                        && backRect.Contains(mousePos)
                    )
                    {
                        property.SetSelectedIndex(i);
                        EditorGUILayout.EndHorizontal();
                        return EventResult.Consume;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2f);
            }

            if (mouseDownIndex >= 0)
            {
                ShowRightClickContextMenu(property, mouseDownIndex);
            }

            return EventResult.None;
        }

        private EventResult DrawDetailsPanel_Bindings(
              in EventData eventData
            , SerializedProperty binderProp
            , Type targetType
        )
        {
            EventResult eventResult;

            EditorGUILayout.BeginVertical(s_rootTabViewStyle);
            {
                var layoutWidth = GUILayout.ExpandWidth(true);
                var headerRect = EditorGUILayout.BeginHorizontal(s_panelHeaderStyle, GUILayout.Height(30));
                {
                    DrawPanelHeaderLabel(s_bindingsLabel, layoutWidth, headerRect);
                }
                EditorGUILayout.EndHorizontal();

                if (_presetBindingsProp == null || _presetBindingsProp.ArraySize < 1)
                {
                    var iconRect = headerRect;
                    iconRect.x += headerRect.width - 22f;
                    iconRect.y += 6f;
                    iconRect.width = 18f;
                    iconRect.height = 18f;

                    var iconWarning = s_iconWarning;
                    iconWarning.tooltip = NO_BINDING;

                    GUI.Label(iconRect, iconWarning);
                }

                var contentRect = EditorGUILayout.BeginVertical(layoutWidth);
                {
                    eventResult = DrawDetailsPanel_Bindings_Content(eventData, contentRect, binderProp, targetType);
                    GUILayout.Space(6f);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            return eventResult;
        }

        private EventResult DrawDetailsPanel_Bindings_Content(
              in EventData eventData
            , in Rect sectionRect
            , SerializedProperty binderProp
            , Type targetType
        )
        {
            var property = _presetBindingsProp;
            var toolbarButton = DrawDetailsPanel_ToolbarButtons(property.ArraySize, false);

            switch (toolbarButton)
            {
                case DetailsToolbarButton.Add:
                {
                    ShowBindingDropdown(this, binderProp, targetType);
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

                if (eventData is { Type: EventType.MouseDown, Button: 1 }
                    && sectionRect.Contains(eventData.MousePos)
                )
                {
                    ShowRightClickContextMenuEmpty(property);
                }
                return EventResult.None;
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
            var iconWarning = s_iconWarning;
            var indexLabelStyle = s_indexLabelStyle;
            var headerLabelStyle = s_headerLabelStyle;
            var subHeaderLabelStyle = s_subHeaderLabelStyle;
            var popupStyle = s_popupStyle;
            var iconStyle = s_iconButtonStyle;
            var selectedColor = s_selectedColor;
            var backColor = s_backColor;
            var altBackColor = s_altBackColor;
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
            var adapterMemberLabel = new GUIContent();
            var resetIconLabel = s_resetIconLabel;

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

                if (memberMap is { Count: 1 })
                {
                    (bindingMethodName, bindingParamType) = memberMap.First();
                }

                EditorGUILayout.Space(2f);
                var rect = EditorGUILayout.BeginVertical();
                {
                    var backRect = rect;
                    backRect.x += 1f;
                    backRect.y -= 4f;
                    backRect.width += 1f;
                    backRect.height += 7f + 4f;

                    // Draw background and select button
                    {
                        var bgColor = i % 2 == 0 ? altBackColor : backColor;
                        bgColor = i == selectedIndex ? selectedColor : bgColor;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(backRect, tex, mode, false, 0f, bgColor, Vector4.zero, Vector4.zero);
                    }

                    var labelRect = EditorGUILayout.BeginHorizontal();
                    {
                        indexLabel.text = i.ToString();
                        EditorGUILayout.LabelField(indexLabel, indexLabelStyle, indexLabelWidth);

                        headerLabel.text = attrib?.Label ?? type.Name;
                        EditorGUILayout.LabelField(headerLabel, headerLabelStyle);
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();

                    var rightClickRect = labelRect;

                    if (bindingParamType != null)
                    {
                        var subLabelRect = EditorGUILayout.BeginHorizontal();
                        {
                            subHeaderLabel.text = bindingParamType.GetFriendlyName(true);
                            subHeaderLabel.tooltip = bindingParamType.FullName;

                            EditorGUILayout.LabelField(GUIContent.none, indexLabelStyle, indexLabelWidth);
                            EditorGUILayout.LabelField(subHeaderLabel, subHeaderLabelStyle);
                            GUILayout.Space(4f);
                        }
                        EditorGUILayout.EndHorizontal();

                        rightClickRect.height += subLabelRect.height;
                    }

                    if (eventData is { Type: EventType.MouseDown, Button: 1 }
                        && rightClickRect.Contains(mousePos)
                    )
                    {
                        mouseDownIndex = i;
                    }

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

                                            GUI.DrawTexture(iconRect, iconWarning.image);
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    DrawAdapterProp(
                                          converterLabel
                                        , itemLabelStyle
                                        , itemLabelWidth
                                        , popupStyle
                                        , iconStyle
                                        , adapterLabel
                                        , adapterMemberLabel
                                        , resetIconLabel
                                        , bindingParamType
                                        , adapterProp
                                        , targetMemberType
                                    );
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();

                    if (eventData is { Type: EventType.MouseDown, Button: 0 }
                        && backRect.Contains(mousePos)
                    )
                    {
                        property.SetSelectedIndex(i);
                        EditorGUILayout.EndVertical();
                        return EventResult.Consume;
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6f);
            }

            if (mouseDownIndex >= 0)
            {
                ShowRightClickContextMenu(property, mouseDownIndex);
            }

            return EventResult.None;
        }

        private static void DrawAdapterProp(
              GUIContent converterLabel
            , GUIStyle itemLabelStyle
            , float itemLabelWidth
            , GUIStyle popupStyle
            , GUIStyle iconStyle
            , GUIContent adapterLabel
            , GUIContent adapterMemberLabel
            , GUIContent resetIconLabel
            , Type bindingParamType
            , SerializedProperty adapterProp
            , Type targetMemberType
        )
        {
            if (adapterProp == null)
            {
                return;
            }

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

                var guiEnabled = GUI.enabled;
                GUI.enabled = adapterProp.managedReferenceValue != null;

                if (GUILayout.Button(resetIconLabel, iconStyle, GUILayout.Width(20)))
                {
                    if (TryCreateDefaultAdapter(targetMemberType, bindingParamType, out var adapter))
                    {
                        var serializedObject = adapterProp.serializedObject;
                        var target = serializedObject.targetObject;

                        Undo.RecordObject(target, $"Reset {adapterProp.propertyPath}");

                        adapterProp.managedReferenceValue = adapter;

                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }
                }

                GUI.enabled = guiEnabled;
            }
            EditorGUILayout.EndHorizontal();

            if (scriptableAdapter != null)
            {
                DrawScriptableAdapter(adapterProp, scriptableAdapter);
                return;
            }

            if (compositeAdapter != null)
            {
                EditorGUILayout.HelpBox(
                      "Composite Adapter is not yet supported."
                    , MessageType.Warning
                );
                return;
            }

            if (adapterType != null)
            {
                DrawAdapterType(adapterType, adapterProp, adapterMemberLabel);
            }
        }

        private static void DrawScriptableAdapter(
              SerializedProperty adapterProp
            , ScriptableAdapter scriptableAdapter
        )
        {
            if (scriptableAdapter == null)
            {
                return;
            }

            var assetProp = adapterProp.FindPropertyRelative("_asset");

            if (assetProp == null)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(assetProp, GUIContent.none, false);

            if (EditorGUI.EndChangeCheck())
            {
                var serializedObject = adapterProp.serializedObject;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawAdapterType(
              Type adapterType
            , SerializedProperty adapterProp
            , GUIContent adapterMemberLabel
        )
        {
            var fieldNames = adapterType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(static x => x.IsPublic || x.GetCustomAttribute<SerializeField>() != null)
                .Select(static x => x.Name);

            EditorGUI.BeginChangeCheck();

            EditorGUI.indentLevel++;

            foreach (var name in fieldNames)
            {
                var prop = adapterProp.FindPropertyRelative(name);
                adapterMemberLabel.text = ObjectNames.NicifyVariableName(name);
                adapterMemberLabel.tooltip = adapterMemberLabel.text;

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(adapterMemberLabel, GUILayout.Width(80));
                    EditorGUILayout.PropertyField(prop, GUIContent.none, true);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                var serializedObject = adapterProp.serializedObject;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }

        private static void ShowBindingDropdown(
              MonoViewInspector inspector
            , SerializedProperty binderProp
            , Type targetType
        )
        {
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

        private static void AddTargetToSelectedBinder(SerializedProperty binderProp)
        {
            var targetsProp = binderProp.FindPropertyRelative(PROP_PRESET_TARGETS);
            var serializedObject = targetsProp.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Add 1 item to {targetsProp.propertyPath}");

            targetsProp.arraySize += 1;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void ShowTargetDropdown(
              MonoViewInspector inspector
            , SerializedProperty binderProp
            , Type targetType
        )
        {
            var rootGo = inspector._view.gameObject;
            var menu = new TreeViewPopup(targetType.Name) {
                width = 500,
                data = binderProp,
                onApplySelectedIds = TargetMenu_AddTargets,
            };

            var tree = targetType == typeof(GameObject)
                ? (menu.Tree = new TargetGameObjectTreeView(menu.TreeViewState, rootGo))
                : (menu.Tree = new TargetComponentTreeView(menu.TreeViewState, rootGo, targetType))
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
            BuildRightClickTextMenu(menu, property);
            menu.ShowAsContext();
        }

        private static void TargetMenu_AddTargets(object data, IList<int> selectedIds)
        {
            if (data is not SerializedProperty binderProp)
            {
                return;
            }

            TryAddTargets(binderProp, selectedIds);
        }

        private void OnDropCreateTargets(Memory<UnityEngine.Object> objects)
        {
            var result = TryGetTargetType(
                  _presetBindersProp
                , out var binderProp
                , out var binderType
                , out var targetType
            );

            if (result != TargetTypeResult.Success)
            {
                DisplayDialog(result, binderType, targetType);
                return;
            }

            var span = objects.Span;
            var length = span.Length;
            var componentType = typeof(Component);
            var instanceIds = new List<int>(length);

            if (targetType == typeof(GameObject))
            {
                for (var i = 0; i < length; i++)
                {
                    var obj = span[i];

                    switch (obj)
                    {
                        case GameObject go:
                        {
                            instanceIds.Add(go.GetInstanceID());
                            continue;
                        }

                        case Component comp:
                        {
                            instanceIds.Add(comp.gameObject.GetInstanceID());
                            continue;
                        }
                    }
                }
            }
            else if (componentType.IsAssignableFrom(targetType))
            {
                for (var i = 0; i < length; i++)
                {
                    var obj = span[i];

                    switch (obj)
                    {
                        case Component comp when targetType == comp.GetType():
                        {
                            instanceIds.Add(obj.GetInstanceID());
                            continue;
                        }

                        case GameObject go when go.TryGetComponent(targetType, out var firstComp):
                        {
                            instanceIds.Add(firstComp.GetInstanceID());
                            continue;
                        }
                    }
                }
            }

            TryAddTargets(binderProp, instanceIds);
        }

        private static void TryAddTargets(SerializedProperty binderProp, IList<int> instanceIds)
        {
            if (instanceIds == null)
            {
                return;
            }

            var length = instanceIds.Count;

            if (length < 1)
            {
                return;
            }

            var targetsProp = binderProp.FindPropertyRelative(PROP_PRESET_TARGETS);
            var currentSize = targetsProp.arraySize;
            var checkIds = new HashSet<int>(currentSize + length);

            for (var i = 0; i < currentSize; i++)
            {
                var elementProp = targetsProp.GetArrayElementAtIndex(i);
                checkIds.Add(elementProp.objectReferenceInstanceIDValue);
            }

            for (var i = length - 1; i >= 0; i--)
            {
                var instanceId = instanceIds[i];

                if (checkIds.Add(instanceId) == false)
                {
                    instanceIds.RemoveAt(i);
                }
            }

            length = instanceIds.Count;

            if (length < 1)
            {
                return;
            }

            var serializedObject = targetsProp.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Add {length} items to {targetsProp.propertyPath}");

            var first = targetsProp.arraySize;
            var newSize = targetsProp.arraySize += length;

            for (var i = first; i < newSize; i++)
            {
                var elementProp = targetsProp.GetArrayElementAtIndex(i);
                elementProp.objectReferenceInstanceIDValue = instanceIds[i - first];
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

        private static DetailsToolbarButton DrawDetailsPanel_ToolbarButtons(int arraySize, bool showSimpleAdd)
        {
            var result = DetailsToolbarButton.None;

            EditorGUILayout.BeginHorizontal();
            {
                if (showSimpleAdd)
                {
                    if (GUILayout.Button(s_addIconLabel, s_toolbarLeftButtonStyle, GUILayout.Height(20)))
                    {
                        result = DetailsToolbarButton.SimpleAdd;
                    }
                }

                var addStyle = showSimpleAdd ? s_toolbarMidButtonStyle : s_toolbarLeftButtonStyle;

                if (GUILayout.Button(s_addMoreIconLabel, addStyle, GUILayout.Height(20)))
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

                if (GUILayout.Button(s_menuIconLabel, s_toolbarMenuButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    result = DetailsToolbarButton.Menu;
                }

                GUI.contentColor = guiContentColor;
            }
            EditorGUILayout.EndHorizontal();

            return result;
        }

        private static TargetTypeResult TryGetTargetType(
              SerializedArrayProperty bindersProp
            , out SerializedProperty binderProp
            , out Type binderType
            , out Type targetType
        )
        {
            binderProp = bindersProp.GetElementAt(bindersProp.SelectedIndex.Value);
            binderType = binderProp.managedReferenceValue?.GetType();

            if (binderType == null)
            {
                targetType = default;
                return TargetTypeResult.UnknownBinder;
            }

            if (s_binderToTargetTypeMap.TryGetValue(binderType, out targetType) == false)
            {
                return TargetTypeResult.NotFoundForBinder;
            }

            if (targetType == typeof(GameObject)
                || typeof(Component).IsAssignableFrom(targetType)
            )
            {
                return TargetTypeResult.Success;
            }

            targetType = default;
            return TargetTypeResult.NotGameObjectOrComponent;
        }

        private static void DisplayDialog(
              TargetTypeResult result
            , Type binderType
            , Type targetType
        )
        {
            EditorUtility.DisplayDialog(
                  "Invalid Target Type"
                , GetMessage(result, binderType, targetType)
                , "I understand"
            );
        }

        private static string GetMessage(
              TargetTypeResult result
            , Type binderType
            , Type targetType
        )
        {
            return result switch {
                TargetTypeResult.UnknownBinder => "Cannot find target type for an unknown binder",
                TargetTypeResult.NotFoundForBinder => $"Cannot find target type for the binder {binderType}!",
                TargetTypeResult.NotGameObjectOrComponent => $"{targetType} is not GameObject nor Component!",
                _ => string.Empty,
            };
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
                      "Binding Context"
                    , "No valid binding context is selected!"
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

                    if (adapterProp.managedReferenceValue == null
                        && TryCreateDefaultAdapter(selectedPropertyType, bindingParamType, out var adapter)
                    )
                    {
                        adapterProp.managedReferenceValue = adapter;
                    }

                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    return;
                }
            }

            {
                if (param is not (
                    SerializedProperty targetPropertyProp
                    , _
                    , _
                    , string selectedPropName
                    , _
                ))
                {
                    return;
                }

                var serializedObject = targetPropertyProp.serializedObject;
                var target = serializedObject.targetObject;

                Undo.RecordObject(target, $"Set {targetPropertyProp.propertyPath}");

                targetPropertyProp.stringValue = selectedPropName;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
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
                      "Binding Context"
                    , "No valid binding context is selected!"
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
            catch
            {
                // ignored
            }

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
                , true
                , true
            );

            AddAdapterTypesToMenu(
                  root
                , adapterProp
                , adapterTypeSaved
                , GetOtherAdapterTypesExcludeSourceType(targetMemberType)
                , "Other"
                , false
                , false
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
            , bool ignoreDirectoryMenu
            , bool ignoreDestinationTypeMenu
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

                node = string.IsNullOrWhiteSpace(directory) || ignoreDirectoryMenu
                    ? node
                    : node.GetOrCreateNode(directory);

                if (ignoreDestinationTypeMenu == false && adapterAttrib?.DestinationType is { } destType)
                {
                    node = node.GetOrCreateNode(destType.GetFriendlyName(), destType.FullName);
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
