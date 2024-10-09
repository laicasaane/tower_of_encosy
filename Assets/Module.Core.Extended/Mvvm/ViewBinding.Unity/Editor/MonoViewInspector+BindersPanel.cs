#if UNITY_EDITOR

using System;
using System.Reflection;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private const string BINDER_SUBTITLE_LABEL = "{0}\n<size=10>{1}</size>";

        private enum ButtonState
        {
            None,
            Apply,
            Cancel,
        }

        private void DrawBindersPanel(in EventData eventData)
        {
            var guiWidth = GUILayout.MinWidth(200);

            EditorGUILayout.BeginVertical(s_rootTabViewStyle, guiWidth);
            {
                var rect = EditorGUILayout.BeginHorizontal(s_bindersHeaderStyle, guiWidth, GUILayout.Height(30));
                {
                    DrawBindersLabel(guiWidth, rect);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(GUILayout.MinWidth(206));
                {
                    DrawBindersPanel_Toolbar();
                    DrawBindersPanel_Content(eventData);
                    GUILayout.Space(6f);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBindersLabel(GUILayoutOption guiWidth, in Rect rect)
        {
            var label = s_bindersLabel;
            var labelStyle = s_rootTabLabelStyle;

            // Draw background
            {
                var backRect = rect;
                backRect.x += 1f;
                backRect.y += 1f;
                backRect.width -= 3f;
                backRect.height -= 1f;

                var tex = Texture2D.whiteTexture;
                var mode = ScaleMode.StretchToFill;
                var borders = Vector4.zero;
                var radius = new Vector4(3f, 3f, 0f, 0f);

                GUI.DrawTexture(backRect, tex, mode, false, 0f, s_headerColor, borders, radius);
            }

            // Draw icon
            {
                labelStyle.CalcMinMaxWidth(label, out var minWidth, out _);
                var minHeight = labelStyle.CalcHeight(label, minWidth);

                var iconRect = rect;
                iconRect.x += 5f;
                iconRect.y += (rect.height - minHeight) / 2f - 3f;
                iconRect.width = 20f;
                iconRect.height = 20f;

                var tex = s_iconBinding.image;
                var mode = ScaleMode.ScaleToFit;
                var borders = Vector4.zero;
                var radius = Vector4.zero;

                GUI.DrawTexture(iconRect, tex, mode, true, 1f, Color.white, borders, radius);
            }

            EditorGUILayout.LabelField(label, labelStyle, guiWidth, GUILayout.Height(26));
        }

        private void DrawBindersPanel_Toolbar()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(s_addLabel, s_toolbarLeftButtonStyle, GUILayout.Height(20)))
                {
                    ShowBinderDropdown();
                }

                var guiEnabled = GUI.enabled;
                GUI.enabled = _presetBindersProp.ArraySize > 0;

                if (GUILayout.Button(s_removeSelectedLabel, s_toolbarMidButtonStyle, GUILayout.Height(20)))
                {
                    _presetBindersProp.DeleteSelected();
                }

                GUI.enabled = guiEnabled;

                var guiContentColor = GUI.contentColor;
                GUI.contentColor = s_menuColor;

                if (GUILayout.Button(s_menuLabel, s_toolbarMenuButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(s_copyAllLabel, false, Menu_OnCopyAll, _presetBindersProp);

                    if (_presetBindersProp.ValidatePasteAll())
                    {
                        menu.AddItem(s_pasteAllLabel, false, Menu_OnPasteAll, _presetBindersProp);
                    }
                    else
                    {
                        menu.AddDisabledItem(s_pasteAllLabel);
                    }

                    menu.AddItem(s_clearAllLabel, false, Menu_OnClearAll, _presetBindersProp);
                    menu.ShowAsContext();
                }

                GUI.contentColor = guiContentColor;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowBinderDropdown()
        {
            s_bindersPropRef.Prop = _presetBindersProp.Property;
            s_bindersPropRef.Inspector = this;

            var menu = s_binderMenu;
            menu.width = 250;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.Show(Event.current.mousePosition);
        }

        private static void BinderMenu_AddBinder(object userData)
        {
            if (userData is not MenuItemBinder menuItem)
            {
                return;
            }

            var (binderType, _, propRef) = menuItem;
            var property = propRef.Prop;
            var inspector = propRef.Inspector;
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var lastIndex = property.arraySize;

            Undo.RecordObject(target, $"Add {binderType} to {property.propertyPath}");

            property.arraySize++;

            var elementProp = property.GetArrayElementAtIndex(lastIndex);
            elementProp.managedReferenceValue = Activator.CreateInstance(binderType);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            inspector._presetBindersProp.SetSelectedIndex(lastIndex);
        }

        private void DrawBindersPanel_Content(in EventData eventData)
        {
            var bindersLength = _presetBindersProp.ArraySize;
            var guiWidth1 = GUILayout.MinWidth(206);

            var rect = EditorGUILayout.BeginVertical(guiWidth1);
            {
                var expandHeight = 13;
                var itemRect = rect;
                itemRect.height = 26;
                itemRect.width = 200;

                var guiHeight = GUILayout.Height(itemRect.height);
                var guiHeightExpand = GUILayout.Height(itemRect.height + expandHeight);
                var guiWidth2 = GUILayout.MinWidth(itemRect.width);

                if (bindersLength < 1)
                {
                    GUILayout.Label("This binder list is empty.", s_noBinderStyle, guiHeight, guiWidth2);

                    if (eventData.Type == EventType.MouseDown
                        && eventData.Button == 1
                        && rect.Contains(eventData.MousePos)
                    )
                    {
                        var menu = new GenericMenu();

                        if (_presetBindersProp.ValidatePasteSingle())
                        {
                            menu.AddItem(s_pasteItemLabel, false, Menu_OnPasteSingle, _presetBindersProp);
                        }
                        else
                        {
                            menu.AddDisabledItem(s_pasteItemLabel);
                        }

                        menu.ShowAsContext();
                    }
                }
                else
                {
                    DrawBindersPanel_Content_Binders(
                          guiHeight
                        , guiHeightExpand
                        , guiWidth2
                        , itemRect
                        , expandHeight
                        , eventData
                    );
                }

                EditorGUILayout.Space(1f);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBindersPanel_Content_Binders(
              GUILayoutOption guiHeight
            , GUILayoutOption guiHeightExpand
            , GUILayoutOption guiWidth
            , in Rect origItemRect
            , float itemExpandHeight
            , in EventData eventData
        )
        {
            var subtitleIndex = eventData.Key == KeyCode.Escape ? null : _selectedSubtitleIndex;
            var bindersProp = _presetBindersProp;
            var bindersLength = bindersProp.ArraySize;
            var binderSelectedButtonStyle = s_binderSelectedButtonStyle;
            var binderButtonStyle = s_binderButtonStyle;
            var indexLabelStyle = s_binderIndexLabelStyle;
            var selectedColor = s_selectedColor;
            var indexColor = indexLabelStyle.normal.textColor;
            var selectedBinderIndex = bindersProp.SelectedIndex;
            var iconWarning = s_iconWarning;
            var applyIconLabel = s_applyLabel;
            var cancelIconLabel = s_cancelLabel;
            var buttonLabel = new GUIContent();
            var indexLabel = new GUIContent();
            var subtitleState = ButtonState.None;
            var mousePos = eventData.MousePos;
            var mouseDownIndex = -1;
            var offsetY = 0f;

            for (var i = 0; i < bindersLength; i++)
            {
                var binderProp = bindersProp.GetElementAt(i);

                if (binderProp.managedReferenceValue is not MonoBinder binder)
                {
                    continue;
                }

                var type = binder.GetType();
                var labelAttrib = type.GetCustomAttribute<LabelAttribute>();
                var isSelected = selectedBinderIndex == i;
                var buttonStyle = isSelected ? binderSelectedButtonStyle : binderButtonStyle;
                var guiBackColor = GUI.backgroundColor;
                var newBackColor = guiBackColor * (i % 2 == 0 ? 1f : .8f);
                var normalColor = buttonStyle.normal.textColor;
                var activeColor = buttonStyle.active.textColor;
                var hoverColor = buttonStyle.hover.textColor;
                var subtitleProp = binderProp.FindPropertyRelative(PROP_SUBTITLE);
                var bindingsProp = binderProp.FindPropertyRelative(PROP_PRESET_BINDINGS);
                var targetsProp = binderProp.FindPropertyRelative(PROP_PRESET_TARGETS);
                var itemRect = origItemRect;
                itemRect.y += offsetY;
                offsetY += itemRect.height;

                if (EditorGUIUtility.isProSkin == false && isSelected)
                {
                    buttonStyle.normal.textColor
                        = buttonStyle.active.textColor
                        = buttonStyle.hover.textColor
                        = indexLabelStyle.normal.textColor
                        = indexLabelStyle.active.textColor
                        = indexLabelStyle.hover.textColor
                        = Color.white;
                }

                var showingSubtitle = false;

                if (subtitleIndex.HasValue && subtitleIndex.Value == i)
                {
                    DrawBinderSubtitleField(
                          applyIconLabel
                        , cancelIconLabel
                        , ref subtitleState
                        , out showingSubtitle
                    );
                }
                else
                {
                    GUI.backgroundColor = isSelected ? selectedColor : newBackColor;
                    GUILayoutOption buttonHeight;

                    if (string.IsNullOrWhiteSpace(subtitleProp.stringValue) == false)
                    {
                        buttonLabel.text = string.Format(BINDER_SUBTITLE_LABEL, labelAttrib.Label, subtitleProp.stringValue);
                        buttonHeight = guiHeightExpand;
                        itemRect.height += itemExpandHeight;
                        offsetY += itemExpandHeight;
                    }
                    else
                    {
                        buttonLabel.text = labelAttrib.Label;
                        buttonHeight = guiHeight;
                    }

                    if (GUILayout.Button(buttonLabel, buttonStyle, buttonHeight, guiWidth))
                    {
                        bindersProp.SetSelectedIndex(i);
                        SetSelectedSubtitleIndex(null);
                    }

                    GUI.backgroundColor = guiBackColor;
                }

                if (eventData.Type == EventType.MouseDown
                    && eventData.Button == 1
                    && itemRect.Contains(mousePos)
                )
                {
                    mouseDownIndex = i;
                }

                // Draw index label
                if (showingSubtitle == false)
                {
                    var indexRect = itemRect;
                    indexRect.x += 3f;
                    indexRect.width = 30f;

                    indexLabel.text = i.ToString();
                    GUI.Label(indexRect, indexLabel, indexLabelStyle);
                }

                var showWarning = bindingsProp.arraySize < 1 || targetsProp.arraySize < 1;

                if (showingSubtitle)
                {
                    showWarning = false;
                }
                else if (bindingsProp.arraySize < 1 && targetsProp.arraySize < 1)
                {
                    iconWarning.tooltip = NO_BINDING_TARGET;
                }
                else if (bindingsProp.arraySize < 1)
                {
                    iconWarning.tooltip = NO_BINDING;
                }
                else if (targetsProp.arraySize < 1)
                {
                    iconWarning.tooltip = NO_TARGET;
                }

                // Draw warning icon
                if (showWarning)
                {
                    var size = 18f;
                    var iconRect = itemRect;
                    iconRect.x += itemRect.width - 16f;
                    iconRect.y += (iconRect.height - size) / 2f;
                    iconRect.width = size;
                    iconRect.height = size;

                    GUI.Label(iconRect, iconWarning);
                }

                if (EditorGUIUtility.isProSkin == false && isSelected)
                {
                    buttonStyle.normal.textColor = normalColor;
                    buttonStyle.active.textColor = activeColor;
                    buttonStyle.hover.textColor = hoverColor;
                    indexLabelStyle.normal.textColor = indexColor;
                    indexLabelStyle.active.textColor = indexColor;
                    indexLabelStyle.hover.textColor = indexColor;
                }
            }

            if (_selectedSubtitleProp != null && subtitleState == ButtonState.Apply)
            {
                ApplyBinderSubtitle();
            }

            if (subtitleState == ButtonState.Cancel)
            {
                SetSelectedSubtitleIndex(null);
            }

            if (mouseDownIndex >= 0)
            {
                bindersProp.SetSelectedIndex(mouseDownIndex);

                var propRef = new ArrayPropertyRef(bindersProp, this);
                var menu = new GenericMenu();
                menu.AddItem(s_editSubtitleLabel, false, Menu_OnEditBinderSubtitle, propRef);
                menu.AddSeparator(string.Empty);
                menu.AddItem(s_copyItemLabel, false, Menu_OnCopySelected, bindersProp);

                if (bindersProp.ValidatePasteSingle())
                {
                    menu.AddItem(s_pasteItemLabel, false, Menu_OnPasteSingle, bindersProp);
                }
                else
                {
                    menu.AddDisabledItem(s_pasteItemLabel);
                }

                menu.AddItem(s_deleteItemLabel, false, Menu_OnDeleteSelected, bindersProp);
                menu.ShowAsContext();
            }
        }

        private static void Menu_OnEditBinderSubtitle(object userData)
        {
            if (userData is not ArrayPropertyRef propRef)
            {
                return;
            }

            if (propRef.Prop.ValidateSelectedIndex() == false)
            {
                return;
            }

            propRef.Inspector.SetSelectedSubtitleIndex(propRef.Prop.SelectedIndex);
        }

        private void DrawBinderSubtitleField(
              GUIContent applyIconLabel
            , GUIContent cancelIconLabel
            , ref ButtonState subtitleState
            , out bool showingSubtitle
        )
        {
            showingSubtitle = true;

            GUILayout.Space(2f);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(4f);

                GUI.SetNextControlName(_subtitleControlName);
                _binderSubtitle = GUILayout.TextField(_binderSubtitle);

                var btnWidth = GUILayout.Width(20);
                var btnHeight = GUILayout.Height(20);

                if (GUILayout.Button(applyIconLabel, EditorStyles.iconButton, btnWidth, btnHeight))
                {
                    subtitleState = ButtonState.Apply;
                }

                if (GUILayout.Button(cancelIconLabel, EditorStyles.iconButton, btnWidth, btnHeight))
                {
                    subtitleState = ButtonState.Cancel;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        private void ApplyBinderSubtitle()
        {
            var property = _selectedSubtitleProp;
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Set subtitle to {property.propertyPath}");

            property.stringValue = _binderSubtitle.Trim();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            SetSelectedSubtitleIndex(null);
        }
    }
}

#endif
