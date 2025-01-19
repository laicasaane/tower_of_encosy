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

        private readonly bool _imguiEnabled;

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

        public ScriptableObjectSettingsProvider(
              ScriptableObject settings
            , SettingsScope scope
            , string displayPath
            , bool useImgui
        )
            : base(displayPath, scope)
        {
            Settings = settings;
            _imguiEnabled = useImgui;
        }

        /// <inheritdoc/>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            if (_imguiEnabled)
            {
                _editor = Editor.CreateEditor(Settings);
            }

            base.OnActivate(searchContext, rootElement);
        }

        /// <inheritdoc/>
        public override void OnDeactivate()
        {
            if (_imguiEnabled && _editor.IsValid())
            {
                Object.DestroyImmediate(_editor);
            }

            _editor = null;

            base.OnDeactivate();
        }

        /// <inheritdoc/>
        public override void OnGUI(string searchContext)
        {
            if (Settings.IsInvalid() || _imguiEnabled == false || _editor.IsInvalid())
            {
                return;
            }

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

        /// <inheritdoc/>
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
