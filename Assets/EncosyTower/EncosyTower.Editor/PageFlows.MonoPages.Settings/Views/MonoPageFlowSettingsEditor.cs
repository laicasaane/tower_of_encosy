using System;
using EncosyTower.Editor.Settings;
using EncosyTower.Logging;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings.Views
{
    public class MonoPageFlowSettingsEditor
    {
        private readonly SerializedContext _context;

        private bool _valueUpdated;

        public MonoPageFlowSettingsEditor(
              ScriptableObject settings
            , SerializedObject serializedSettings
            , VisualElement root
        )
        {
            ViewAPI.ApplyStyleSheetsTo(root, Constants.PROJECT_SETTINGS_STYLE_SHEET);

            var resources = ViewAPI.GetResources();
            var context = _context = new SerializedContext(settings, serializedSettings);
            var titleBar = new VisualElement();
            var titleLabel = new Label() {
                text = ObjectNames.NicifyVariableName(context.Name),
            };

            titleBar.AddToClassList("project-settings-title-bar");
            titleLabel.AddToClassList("project-settings-title-bar__label");

            titleBar.Add(titleLabel);
            root.Add(titleBar);

            var container = new ScrollView();
            container.AddToClassList(Constants.MONO_PAGE_FLOW);
            root.Add(container);

            var contentContainer = container.Q("unity-content-container");

            var loaderStrategy = new EnumField("Loader Strategy", default(MonoPageLoaderStrategy));
            var messageScope = new EnumField("Message Scope", default(MonoMessageScope));
            var logEnvironment = new EnumField("Log Environment", default(LogEnvironment));

            contentContainer.Add(loaderStrategy.AddToAlignFieldClass());
            contentContainer.Add(messageScope.AddToAlignFieldClass());
            contentContainer.Add(logEnvironment.AddToAlignFieldClass());

            loaderStrategy.RegisterValueChangedCallback(OnValueChanged);
            messageScope.RegisterValueChangedCallback(OnValueChanged);
            logEnvironment.RegisterValueChangedCallback(OnValueChanged);

            loaderStrategy.BindProperty(context.GetLoaderStrategyProperty());
            messageScope.BindProperty(context.GetMessageScopeProperty());
            logEnvironment.BindProperty(context.GetLogEnvironmentProperty());
            contentContainer.Bind(serializedSettings);
        }

        public void Update()
        {
            if (_valueUpdated)
            {
                _valueUpdated = false;

                var serializedObject = _context.Object;

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                EditorUtility.SetDirty(serializedObject.targetObject);
                AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
            }
        }

        private void OnValueChanged(ChangeEvent<Enum> evt)
        {
            if (evt.newValue.Equals(evt.previousValue) == false)
            {
                _valueUpdated = true;
            }
        }

        [CustomEditor(typeof(MonoPageFlowSettings), true)]
        private sealed class Inspector : UnityEditor.Editor
        {
            private MonoPageFlowSettings _settings;

            private void OnEnable()
            {
                _settings = target as MonoPageFlowSettings;
            }

            public override VisualElement CreateInspectorGUI()
            {
                var root = new VisualElement();
                ViewAPI.ApplyStyleSheetsTo(root, Constants.PROJECT_SETTINGS_STYLE_SHEET);

                var button = new Button(OpenSettingsWindow) {
                    text = "Open Mono Page Flow Settings Window",
                };

                button.AddToClassList("button-open-settings-windows");
                root.Add(button);
                return root;
            }

            private void OpenSettingsWindow()
            {
                if (_settings.IsValid())
                {
                    _settings.OpenSettingsWindow();
                }
            }
        }
    }
}
