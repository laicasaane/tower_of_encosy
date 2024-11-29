#if UNITY_EDITOR

using System;
using EncosyTower.Modules.Mvvm.ViewBinding;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private void DrawContextPanel()
        {
            if (_contextInspector == null
                && _contextProp.managedReferenceValue is IBindingContext context
                && s_contextToInspectorMap.TryGetValue(context.GetType(), out var inspectorType)
            )
            {
                _contextInspector = Activator.CreateInstance(inspectorType) as BindingContextInspector;
                _contextInspector.ContextType = context.GetType();
                _contextInspector.OnEnable(_view, serializedObject, _contextProp);
            }

            EditorGUILayout.BeginVertical(s_rootTabViewStyle);
            {
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
                {
                    EditorGUILayout.Space(22);
                    DrawContextHeader(rect);
                }
                EditorGUILayout.EndHorizontal();

                if (_contextInspector != null)
                {
                    EditorGUILayout.Space(6f);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(6f);
                        EditorGUILayout.BeginVertical();
                        {
                            _contextInspector.OnInspectorGUI();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(6f);
                }
                else
                {
                    GUILayout.Label("No binding context is chosen.", s_noBinderStyle, GUILayout.Height(30));
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawContextHeader(in Rect rect)
        {
            var buttonSize = 30f;

            // Draw background
            {
                var backRect = rect;
                backRect.x += 1f;
                backRect.y += 1f;
                backRect.width -= 3f;
                backRect.height += 3f;

                var tex = Texture2D.whiteTexture;
                var mode = ScaleMode.StretchToFill;
                var borders = Vector4.zero;
                var radius = new Vector4(3f, 3f, 0f, 0f);

                GUI.DrawTexture(backRect, tex, mode, false, 0f, s_headerColor, borders, radius);
            }

            // Draw label
            {
                var labelRect = rect;
                labelRect.y += 1f;

                _contextLabel.text = _contextInspector == null
                    ? "<Invalid Binding Context>"
                    : ObjectNames.NicifyVariableName(_contextInspector.ContextType.Name);

                GUI.Label(labelRect, _contextLabel, s_rootTabLabelStyle);
            }

            {
                var btnRect = rect;
                btnRect.x += rect.width - buttonSize - 1f;
                btnRect.width = buttonSize;

                if (GUI.Button(btnRect, s_chooseIconLabel, s_chooseContextButtonStyle))
                {
                    s_contextPropRef.Prop = _contextProp;
                    s_contextPropRef.Inspector = this;

                    var menu = s_contextMenu;
                    menu.width = 250;
                    menu.height = 350;
                    menu.maxHeight = 600;
                    menu.showSearch = true;
                    menu.Show(Event.current.mousePosition);
                }
            }
        }
    }
}

#endif
