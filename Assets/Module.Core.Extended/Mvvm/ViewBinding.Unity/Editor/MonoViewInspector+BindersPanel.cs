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
        private void DrawBindersPanel()
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
                    DrawBindersPanel_Content();
                    GUILayout.Space(6f);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBindersLabel(GUILayoutOption guiWidth, Rect rect)
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
                    RemoveSelectedBinder();
                }

                GUI.enabled = guiEnabled;

                var guiContentColor = GUI.contentColor;
                GUI.contentColor = _menuColor;

                if (GUILayout.Button(_menuLabel, _toolbarMenuButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    s_bindersPropRef.Prop = _presetBindersProp;
                    s_bindersPropRef.Inspector = this;

                    var menu = new GenericMenu();
                    menu.AddItem(_clearLabel, false, ClearBinders, s_bindersPropRef);
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

        private static void ClearBinders(object userData)
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

        private void RemoveSelectedBinder()
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

            Undo.RecordObject(target, $"Remove binder at {property.propertyPath}[{index}]");

            property.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            var newLength = length - 1;

            if (index >= newLength)
            {
                SetSelectedBinderIndex(newLength > 0 ? newLength - 1 : default(int?));
            }
        }

        private void DrawBindersPanel_Content()
        {
            var bindersLength = _presetBindersProp.arraySize;
            var guiWidth1 = GUILayout.MinWidth(206);

            var rect = EditorGUILayout.BeginVertical(guiWidth1);
            {
                var itemRect = rect;
                var guiHeight = GUILayout.Height(itemRect.height = 26);
                var guiWidth2 = GUILayout.MinWidth(itemRect.width = 200);

                if (bindersLength < 1)
                {
                    GUILayout.Label("This binder list is empty.", _noBinderStyle, guiHeight, guiWidth2);
                }
                else
                {
                    DrawBindersPanel_Content_Binders(guiHeight, guiWidth2, itemRect);
                }

                EditorGUILayout.Space(1f);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBindersPanel_Content_Binders(
              GUILayoutOption guiHeight
            , GUILayoutOption guiWidth
            , Rect itemRect
        )
        {
            var bindersProp = _presetBindersProp;
            var bindersLength = bindersProp.arraySize;
            var binderSelectedButtonStyle = _binderSelectedButtonStyle;
            var binderButtonStyle = _binderButtonStyle;
            var indexLabelStyle = _binderIndexLabelStyle;
            var selectedColor = _selectedColor;
            var indexColor = indexLabelStyle.normal.textColor;
            var selectedBinderIndex = _selectedBinderIndex;
            var iconWarning = _iconWarning;

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
                var bindingsProp = binderProp.FindPropertyRelative("_presetBindings");
                var targetsProp = binderProp.FindPropertyRelative("_presetTargets");

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

                GUI.backgroundColor = isSelected ? selectedColor : newBackColor;

                if (GUILayout.Button(labelAttrib.Label, buttonStyle, guiHeight, guiWidth))
                {
                    SetSelectedBinderIndex(i);
                }

                GUI.backgroundColor = guiBackColor;

                // Draw index label
                {
                    var rect = itemRect;
                    rect.x += 3f;
                    rect.y += rect.height * i;

                    GUI.Label(rect, $"{i}", indexLabelStyle);
                }

                var showWarning = bindingsProp.arraySize < 1 || targetsProp.arraySize < 1;

                if (bindingsProp.arraySize < 1 && targetsProp.arraySize < 1)
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
                    var iconRect = itemRect;
                    iconRect.x += itemRect.width - 16f;
                    iconRect.y += (itemRect.height * i) + 5f;
                    iconRect.width = 18f;
                    iconRect.height = 18f;

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
        }
    }
}

#endif
