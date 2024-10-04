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

        private readonly GUIContent _clearBindersLabel = new("Clear");
        private readonly GUIContent _copyBindersLabel = new("Copy");
        private readonly GUIContent _pasteBindersLabel = new("Paste");
        private readonly GUIContent _editSubtitleLabel = new("Edit Subtitle _f2");
        private readonly GUIContent _copyBinderLabel = new("Copy _^c");
        private readonly GUIContent _pasteBinderLabel = new("Paste _^v");
        private readonly GUIContent _deleteBinderLabel = new("Delete _del");

        private void DrawBindersPanel(in EventData eventData)
        {
            var guiWidth = GUILayout.MinWidth(200);

            EditorGUILayout.BeginVertical(_rootTabViewStyle, guiWidth);
            {
                var rect = EditorGUILayout.BeginHorizontal(_bindersHeaderStyle, guiWidth, GUILayout.Height(30));
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
            var label = _bindersLabel;
            var labelStyle = _rootTabLabelStyle;

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

                GUI.DrawTexture(backRect, tex, mode, false, 0f, _headerColor, borders, radius);
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

                var tex = _iconBinding.image;
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
                if (GUILayout.Button(_addLabel, _toolbarLeftButtonStyle, GUILayout.Height(20)))
                {
                    ShowBinderDropdown();
                }

                var guiEnabled = GUI.enabled;
                GUI.enabled = _presetBindersProp.arraySize > 0;

                if (GUILayout.Button(_removeSelectedLabel, _toolbarMidButtonStyle, GUILayout.Height(20)))
                {
                    DeleteSelectedBinder();
                }

                GUI.enabled = guiEnabled;

                var guiContentColor = GUI.contentColor;
                GUI.contentColor = _menuColor;

                if (GUILayout.Button(_menuLabel, _toolbarMenuButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    var propRef = new BindersPropRef(_presetBindersProp, this);
                    var menu = new GenericMenu();
                    menu.AddItem(_copyBindersLabel, false, Menu_OnCopyBinders, propRef);

                    if (ValidatePasteBinders())
                    {
                        menu.AddItem(_pasteBindersLabel, false, Menu_OnPasteBinders, propRef);
                    }
                    else
                    {
                        menu.AddDisabledItem(_pasteBinderLabel);
                    }

                    menu.AddItem(_clearBindersLabel, false, Menu_OnClearBinders, propRef);
                    menu.ShowAsContext();
                }

                GUI.contentColor = guiContentColor;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowBinderDropdown()
        {
            s_bindersPropRef.Prop = _presetBindersProp;
            s_bindersPropRef.Inspector = this;

            var menu = s_binderMenu;
            menu.height = 350;
            menu.maxHeight = 600;
            menu.showSearch = true;
            menu.Show(Event.current.mousePosition);
        }

        private static void Menu_OnClearBinders(object userData)
        {
            if (userData is not BindersPropRef propRef)
            {
                return;
            }

            var property = propRef.Prop;
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Clear {property.propertyPath}");

            property.ClearArray();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            propRef.Inspector.SetSelectedBinderIndex(null);
        }

        private static void Menu_OnCopyBinders(object userData)
        {
            if (userData is not BindersPropRef propRef)
            {
                return;
            }

            propRef.Inspector.CopyBinders();
        }

        private static void Menu_OnPasteBinders(object userData)
        {
            if (userData is not BindersPropRef propRef)
            {
                return;
            }

            propRef.Inspector.PasteBinders();
        }

        private static void BinderMenu_AddBinder(object userData)
        {
            if (userData is not BinderMenuItem menuItem)
            {
                return;
            }

            var (binderType, _, instance) = menuItem;
            var property = instance.Prop;
            var inspector = instance.Inspector;
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var lastIndex = property.arraySize;

            Undo.RecordObject(target, $"Add {binderType} to {property.propertyPath}");

            property.arraySize++;

            var elementProp = property.GetArrayElementAtIndex(lastIndex);
            elementProp.managedReferenceValue = Activator.CreateInstance(binderType);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            inspector.SetSelectedBinderIndex(lastIndex);
        }

        private void DeleteSelectedBinder()
        {
            var property = _presetBindersProp;
            var length = property.arraySize;
            var index = _selectedBinderIndex ?? length - 1;

            if ((uint)index >= (uint)length)
            {
                return;
            }

            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, $"Delete binder at {property.propertyPath}[{index}]");

            property.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            var newLength = length - 1;

            if (index >= newLength)
            {
                SetSelectedBinderIndex(newLength > 0 ? newLength - 1 : default(int?));
            }
        }

        private void DrawBindersPanel_Content(in EventData eventData)
        {
            var bindersLength = _presetBindersProp.arraySize;
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
                    GUILayout.Label("This binder list is empty.", _noBinderStyle, guiHeight, guiWidth2);
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
            var bindersLength = bindersProp.arraySize;
            var binderSelectedButtonStyle = _binderSelectedButtonStyle;
            var binderButtonStyle = _binderButtonStyle;
            var indexLabelStyle = _binderIndexLabelStyle;
            var selectedColor = _selectedColor;
            var indexColor = indexLabelStyle.normal.textColor;
            var selectedBinderIndex = _selectedBinderIndex;
            var iconWarning = _iconWarning;
            var applyIconLabel = _applyIconLabel;
            var cancelIconLabel = _cancelIconLabel;
            var buttonLabel = new GUIContent();
            var indexLabel = new GUIContent();
            var subtitleState = ButtonState.None;
            var mousePos = eventData.MousePos;
            var mouseDownIndex = -1;
            var offsetY = 0f;

            for (var i = 0; i < bindersLength; i++)
            {
                var binderProp = bindersProp.GetArrayElementAtIndex(i);

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
                        SetSelectedBinderIndex(i);
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
                SetSelectedBinderIndex(mouseDownIndex);

                s_bindersPropRef.Prop = _presetBindersProp;
                s_bindersPropRef.Inspector = this;

                var propRef = new BindersPropRef(_presetBindersProp, this);
                var menu = new GenericMenu();
                menu.AddItem(_editSubtitleLabel, false, Menu_OnEditBinderSubtitle, propRef);
                menu.AddSeparator(string.Empty);
                menu.AddItem(_copyBinderLabel, false, Menu_OnCopyBinder, propRef);

                if (ValidatePasteSingleBinder())
                {
                    menu.AddItem(_pasteBinderLabel, false, Menu_OnPasteBinder, propRef);
                }
                else
                {
                    menu.AddDisabledItem(_pasteBinderLabel);
                }

                menu.AddItem(_deleteBinderLabel, false, Menu_OnDeleteBinder, propRef);
                menu.ShowAsContext();
            }
        }

        private static void Menu_OnEditBinderSubtitle(object userData)
        {
            if (userData is not BindersPropRef propRef)
            {
                return;
            }

            var inspector = propRef.Inspector;

            if (inspector.ValidateSelectedBinderIndex() == false)
            {
                return;
            }

            inspector.SetSelectedSubtitleIndex(inspector._selectedBinderIndex);
        }

        private static void Menu_OnCopyBinder(object userData)
        {
            if (userData is not BindersPropRef propRef)
            {
                return;
            }

            propRef.Inspector.CopySingleBinder();
        }

        private static void Menu_OnPasteBinder(object userData)
        {
            if (userData is not BindersPropRef propRef)
            {
                return;
            }

            propRef.Inspector.PasteSingleBinder();
        }

        private static void Menu_OnDeleteBinder(object userData)
        {
            if (userData is not BindersPropRef propRef)
            {
                return;
            }

            propRef.Inspector.DeleteSelectedBinder();
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
