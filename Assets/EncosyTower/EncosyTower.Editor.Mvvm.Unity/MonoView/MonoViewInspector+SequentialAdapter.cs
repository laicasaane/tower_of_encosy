#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EncosyTower.Annotations;
using EncosyTower.Logging;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.Mvvm.ViewBinding.Adapters;
using EncosyTower.Mvvm.ViewBinding.Adapters.Unity;
using EncosyTower.Mvvm.ViewBinding.Unity;
using EncosyTower.Types;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private static EventResult DrawSequentialAdapter(
              in EventData eventData
            , SerializedArrayProperty presetAdaptersProp
            , SerializedProperty parentAdapterProp
            , SequentialAdapter sequentialAdapter
            , Type bindingParamType
            , Type targetMemberType
            , GUIContent adapterLabel
        )
        {
            var eventResult = EventResult.None;

            presetAdaptersProp.Initialize(parentAdapterProp.FindPropertyRelative(PROP_PRESET_ADAPTERS));

            if (presetAdaptersProp.Property == null)
            {
                return eventResult;
            }

            var layoutWidth = GUILayout.ExpandWidth(true);
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginVertical(s_rootTabViewStyle, layoutWidth);
            {
                var adaptersProp = presetAdaptersProp.Property;

                var headerRect = EditorGUILayout.BeginHorizontal(layoutWidth, GUILayout.Height(22f));
                {
                    var offset = new Rect(0f, 0f, -1f, -1f);
                    var expandOffset = new Rect(0f, 0f, -1f, -2f);

                    EditorGUILayout.Space(24f);
                    DrawPanelHeaderFoldout(
                          headerRect
                        , adaptersProp
                        , s_adaptersLabel
                        , offset
                        , expandOffset
                        , s_tabLabelStyle
                    );
                }
                EditorGUILayout.EndHorizontal();

                if (presetAdaptersProp.ArraySize < 1)
                {
                    var iconRect = headerRect;
                    iconRect.x += headerRect.width - 22f;
                    iconRect.y += 4f;
                    iconRect.width = 18f;
                    iconRect.height = 18f;

                    var iconWarning = s_iconWarning;
                    iconWarning.tooltip = NO_BINDING;

                    GUI.Label(iconRect, iconWarning);
                }

                if (adaptersProp.isExpanded)
                {
                    var contentRect = EditorGUILayout.BeginVertical(layoutWidth);
                    {
                        eventResult = DrawSequentialAdapter_Content(
                              eventData
                            , contentRect
                            , presetAdaptersProp
                            , sequentialAdapter
                            , bindingParamType
                            , targetMemberType
                            , adapterLabel
                        );
                        GUILayout.Space(6f);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();

            return eventResult;
        }

        private static EventResult DrawSequentialAdapter_Content(
              in EventData eventData
            , in Rect sectionRect
            , SerializedArrayProperty presetAdaptersProp
            , SequentialAdapter sequentialAdapter
            , Type bindingParamType
            , Type targetMemberType
            , GUIContent adapterLabel
        )
        {
            var toolbarButton = DrawDetailsPanel_ToolbarButtons(presetAdaptersProp.ArraySize, false);

            switch (toolbarButton)
            {
                case DetailsToolbarButton.Add:
                {
                    ShowAdapterPropertyMenu(
                          bindingParamType
                        , targetMemberType
                        , presetAdaptersProp
                        , null
                        , null
                        , includeSequentialAdapter: false
                    );
                    break;
                }

                case DetailsToolbarButton.Remove:
                {
                    presetAdaptersProp.DeleteSelected();
                    break;
                }

                case DetailsToolbarButton.Menu:
                {
                    ShowMenuContextMenu(presetAdaptersProp);
                    break;
                }
            }

            var length = presetAdaptersProp.ArraySize;

            if (length < 1)
            {
                EditorGUILayout.LabelField(
                      "This adapter list is empty."
                    , s_noBinderStyle
                    , GUILayout.Height(30)
                    , GUILayout.MinWidth(0)
                );

                if (eventData is { Type: EventType.MouseDown, Button: 1 }
                    && sectionRect.Contains(eventData.MousePos)
                )
                {
                    ShowRightClickContextMenuEmpty(presetAdaptersProp);
                }
                return EventResult.None;
            }

            return DrawSequentialAdapter_Content_Adapters(
                  eventData
                , sectionRect
                , presetAdaptersProp
                , targetMemberType
                , bindingParamType
            );
        }

        private static EventResult DrawSequentialAdapter_Content_Adapters(
              in EventData eventData
            , in Rect sectionRect
            , SerializedArrayProperty presetAdaptersProp
            , Type targetMemberType
            , Type bindingParamType
        )
        {
            var propertyLabel = s_propertyBindingLabel;
            var commandLabel = s_commandBindingLabel;
            var itemLabelStyle = s_itemLabelStyle;

            itemLabelStyle.CalcMinMaxWidth(propertyLabel, out _, out var maxPropWidth);
            itemLabelStyle.CalcMinMaxWidth(commandLabel, out _, out var maxCmdWidth);

            var subLabelWidth = Mathf.Max(maxPropWidth, maxCmdWidth);
            var subLabelWidth1 = subLabelWidth - 9f;
            var subLabelWidth2 = subLabelWidth - 6f;
            var adapterLabel = new GUIContent();
            var adapterMemberLabel = new GUIContent();
            var adapterMemberLabelWidth = GUILayout.Width(80f);
            var indexLabel = new GUIContent();
            var indexLabelWidth = GUILayout.Width(20);
            var minWidth = GUILayout.MinWidth(0);
            var indexLabelStyle = s_indexLabelStyle;
            var selectedColor = s_selectedColor2;
            var backColor = s_backColor;
            var altBackColor = s_altBackColor;
            var popupStyle = s_popupStyle;
            var resetIconLabel = s_resetIconLabel;
            var iconStyle = s_iconButtonStyle;
            var mouseDownIndex = -1;
            var mousePos = eventData.MousePos;
            var length = presetAdaptersProp.ArraySize;
            var selectedIndex = presetAdaptersProp.SelectedIndex;

            for (var i = 0; i < length; i++)
            {
                var adapterProp = presetAdaptersProp.GetElementAt(i);

                GetAdapterData(
                      adapterProp
                    , adapterLabel
                    , out var adapterType
                    , out var scriptableAdapter
                    , out _
                );

                var isScriptable = scriptableAdapter != null;

                EditorGUILayout.Space(2f);

                var rect = EditorGUILayout.BeginVertical();
                {
                    var backRect = rect;
                    backRect.x -= 2f;
                    backRect.y -= 4f;
                    backRect.width += 4f;
                    backRect.height += 9f;

                    // Draw background and select button
                    {
                        var bgColor = i % 2 == 0 ? altBackColor : backColor;
                        bgColor = i == selectedIndex ? selectedColor : bgColor;

                        var tex = Texture2D.whiteTexture;
                        var mode = ScaleMode.StretchToFill;

                        GUI.DrawTexture(backRect, tex, mode, false, 0f, bgColor, Vector4.zero, Vector4.zero);
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        var labelRect = EditorGUILayout.BeginHorizontal(indexLabelWidth);
                        {
                            indexLabel.text = i.ToString();
                            EditorGUILayout.LabelField(indexLabel, indexLabelStyle, indexLabelWidth, minWidth);
                        }
                        EditorGUILayout.EndHorizontal();

                        var rightClickRect = labelRect;
                        rightClickRect.width += isScriptable ? subLabelWidth2 : subLabelWidth1;

                        if (eventData is { Type: EventType.MouseDown, Button: 1 }
                            && rightClickRect.Contains(mousePos)
                        )
                        {
                            mouseDownIndex = i;
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5f);
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(isScriptable ? subLabelWidth2 : subLabelWidth1);

                                if (GUILayout.Button(adapterLabel, popupStyle))
                                {
                                    ShowAdapterPropertyMenu(
                                          bindingParamType
                                        , targetMemberType
                                        , null
                                        , adapterProp
                                        , adapterType
                                        , includeSequentialAdapter: false
                                    );
                                }

                                if (GUILayout.Button(resetIconLabel, iconStyle, GUILayout.Width(20)))
                                {
                                    if (TryCreateDefaultAdapter(targetMemberType, bindingParamType, out var newAdapter))
                                    {
                                        var serializedObject = adapterProp.serializedObject;
                                        var target = serializedObject.targetObject;

                                        Undo.RecordObject(target, $"Reset {adapterProp.propertyPath}");

                                        adapterProp.managedReferenceValue = newAdapter;

                                        serializedObject.ApplyModifiedProperties();
                                        serializedObject.Update();
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(4f);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndHorizontal();

                    if (eventData is { Type: EventType.MouseDown, Button: 0 }
                        && backRect.Contains(mousePos)
                    )
                    {
                        presetAdaptersProp.SetSelectedIndex(i);
                        EditorGUILayout.EndVertical();
                        return EventResult.Consume;
                    }

                    if (scriptableAdapter != null)
                    {
                        DrawScriptableAdapter(adapterProp, scriptableAdapter);
                    }
                    else if (adapterType != null)
                    {
                        DrawAdapterType(adapterType, adapterProp, adapterMemberLabel, adapterMemberLabelWidth);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(7f);
            }

            if (mouseDownIndex >= 0)
            {
                ShowRightClickContextMenu(presetAdaptersProp, mouseDownIndex);
            }

            return EventResult.None;
        }
    }
}

#endif
