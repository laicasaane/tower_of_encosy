#if UNITY_EDITOR

using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows.MonoPages;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.PageFlows.MonoPages
{
    [CustomEditor(typeof(MonoPageCodex), true)]
    public class MonoPageCodexInspector : UnityEditor.Editor
    {
        private MonoPageCodex _codex;

        private void OnEnable()
        {
            _codex = target as MonoPageCodex;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (_codex._flowContext.useProjectSettings)
            {
                EditorGUILayout.HelpBox(
                      "The option 'Use Project Settings' has been turned ON.\n" +
                      "Some configurations on the local flow context will be overwritten at runtime.\n" +
                      "If you do not wish to use configurations from Project Settings, " +
                      "please turn it OFF."
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
