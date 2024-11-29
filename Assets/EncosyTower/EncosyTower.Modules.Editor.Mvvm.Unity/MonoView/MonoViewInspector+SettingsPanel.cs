#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private void DrawSettingsPanel()
        {
            EditorGUILayout.BeginVertical(s_rootTabViewStyle);
            {
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(30f));
                {
                    EditorGUILayout.Space(30f);
                    DrawPanelHeaderFoldout(rect, _settingsProp, _settingsLabel);
                }
                EditorGUILayout.EndHorizontal();

                if (_settingsProp.isExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(6f);
                        EditorGUILayout.BeginVertical();
                        {
                            DrawSettingsPanel_Content();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(6f);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSettingsPanel_Content()
        {
            var settingsProp = _settingsProp;
            var names = s_settingFieldNames.AsSpan();
            var length = names.Length;

            for (var i = 0; i < length; i++)
            {
                var prop = settingsProp.FindPropertyRelative(names[i]);
                EditorGUILayout.PropertyField(prop, true);
            }
        }
    }
}

#endif
