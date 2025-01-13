// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
// https://github.com/hextantstudios/com.hextantstudios.utilities/blob/master/Editor/Settings/ScriptableObjectSettingsProvider.cs

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor.Settings
{
    using Editor = UnityEditor.Editor;

    /// <summary>
    /// An extension for <see cref="SettingsProvider"/> to display settings
    /// for classes derived from <see cref="ScriptableObject"/>.
    /// </summary>
    public class ScriptableObjectSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedSettings;

        /// <summary>
        /// True if the keywords set has been built.
        /// </summary>
        private bool _keywordsBuilt;

        /// <summary>
        /// Cached editor used to render inspector GUI.
        /// </summary>
        private Editor _editor;

        /// <summary>
        /// The settings instance being edited.
        /// </summary>
        public readonly ScriptableObject Settings;

        /// <summary>
        /// The SerializedObject settings instance.
        /// </summary>
        public SerializedObject SerializedSettings => _serializedSettings ??= new(Settings);

        public ScriptableObjectSettingsProvider(ScriptableObject settings, SettingsScope scope, string displayPath)
            : base(displayPath, scope)
            => Settings = settings;

        // Called when the settings are displayed in the UI.
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _editor = Editor.CreateEditor(Settings);
            base.OnActivate(searchContext, rootElement);
        }

        // Called when the settings are no longer displayed in the UI.
        public override void OnDeactivate()
        {
            Object.DestroyImmediate(_editor);
            _editor = null;
            base.OnDeactivate();
        }

        // Displays the settings.
        public override void OnGUI(string searchContext)
        {
            if (Settings == null || _editor == null) return;

            // Set label width and indentation to match other settings.
            EditorGUIUtility.labelWidth = 250;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Space(10);

            // Draw the editor's GUI.
            _editor.OnInspectorGUI();

            // Reset label width and indent.
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;
        }

        // Build the set of keywords on demand from the settings fields.
        public override bool HasSearchInterest(string searchContext)
        {
            if (_keywordsBuilt == false)
            {
                keywords = GetSearchKeywordsFromSerializedObject(SerializedSettings);
                _keywordsBuilt = true;
            }

            return base.HasSearchInterest(searchContext);
        }
    }
}
