#if UNITY_EDITOR

using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows.MonoPages;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.PageFlows.MonoPages
{
    [CustomEditor(typeof(MonoPageFlow), true)]
    public class MonoPageFlowInspector : UnityEditor.Editor
    {
        private MonoPageFlow _flow;

        private void OnEnable()
        {
            _flow = target as MonoPageFlow;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (_flow._context.useProjectSettings)
            {
                EditorGUILayout.HelpBox(
                      "The option 'Use Project Settings' has been turned on.\n" +
                      "Some configurations on the local flow context will be overwritten at runtime.\n" +
                      "If you do not wish to use configurations from Project Settings, " +
                      "please turn off the option 'Use Project Settings' on the context."
                    , MessageType.Warning
                );
            }

            ButtonOpenMonoPageFlowSettingsWindow();
        }

        protected void ButtonOpenMonoPageFlowSettingsWindow()
        {
            if (GUILayout.Button("Open Mono Page Flow Settings Window", GUILayout.Height(24)))
            {
                MonoPageFlowSettings.Instance.OpenSettingsWindow();
            }
        }
    }
}

#endif
