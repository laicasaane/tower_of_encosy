// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
// https://github.com/hextantstudios/com.hextantstudios.utilities/blob/master/Editor/Settings/SettingsEditor.cs

using UnityEditor;
using EncosyTower.Modules.Settings;

namespace EncosyTower.Modules.Editor.Settings
{
    /// <summary>
    /// A custom inspector for Settings that does not draw the "Script" field.
    /// </summary>
    [CustomEditor(typeof(Settings<>), true)]
    public class SettingsEditor : UnityEditor.Editor
    {
        private static readonly string[] s_excludedFields = { "m_Script" };

        public override void OnInspectorGUI()
            => DrawDefaultInspector();

        // Draws the UI for exposed properties *without* the "Script" field.
        protected new bool DrawDefaultInspector()
        {
            if (serializedObject.targetObject == null)
            {
                return false;
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();

            DrawPropertiesExcluding(serializedObject, s_excludedFields);

            serializedObject.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }
    }
}
