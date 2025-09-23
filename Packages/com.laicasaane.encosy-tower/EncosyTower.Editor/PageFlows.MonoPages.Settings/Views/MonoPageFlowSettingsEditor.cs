#if UNITY_EDITOR

using System;
using EncosyTower.Editor.Settings;
using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.Pooling;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings.Views
{
    internal class MonoPageFlowSettingsEditor
    {
        public static readonly string ProjectSettingsUssClassName = "project-settings";
        public static readonly string ProjectSettingsTitleBarUssClassName = $"{ProjectSettingsUssClassName}-title-bar";
        public static readonly string ProjectSettingsTitleBarLabelUssClassName = $"{ProjectSettingsTitleBarUssClassName}__label";
        public static readonly string UssClassName = "mono-page-flow";

        private readonly SerializedContext _context;

        private bool _valueUpdated;

        public MonoPageFlowSettingsEditor(
              ScriptableObject settings
            , SerializedObject serializedSettings
            , VisualElement root
        )
        {
            root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
            root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

            var context = _context = new SerializedContext(settings, serializedSettings);
            var titleBar = new VisualElement();
            var titleLabel = new Label() {
                text = ObjectNames.NicifyVariableName(context.Name),
            };

            titleBar.AddToClassList(ProjectSettingsTitleBarUssClassName);
            titleLabel.AddToClassList(ProjectSettingsTitleBarLabelUssClassName);

            titleBar.Add(titleLabel);
            root.Add(titleBar);

            var container = new ScrollView();
            container.AddToClassList(UssClassName);
            root.Add(container);

            var contentContainer = container.Q("unity-content-container");

            var slimPublishingContext = new Toggle("Slim Publishing Context");
            var ignoreEmptySubscriber = new Toggle("Ignore Empty Subscriber");
            var loaderStrategy = new EnumField("Loader Strategy", default(MonoPageLoaderStrategy));
            var pooledStrategy = new EnumField("Pooled GameObject Strategy", default(PooledGameObjectStrategy));
            var messageScope = new EnumField("Message Scope", default(MonoMessageScope));
            var logEnvironment = new EnumField("Log Environment", default(LogEnvironment));

            contentContainer.Add(slimPublishingContext.WithAlignFieldClass());
            contentContainer.Add(ignoreEmptySubscriber.WithAlignFieldClass());
            contentContainer.Add(loaderStrategy.WithAlignFieldClass());
            contentContainer.Add(pooledStrategy.WithAlignFieldClass());
            contentContainer.Add(messageScope.WithAlignFieldClass());
            contentContainer.Add(logEnvironment.WithAlignFieldClass());

            slimPublishingContext.RegisterValueChangedCallback(OnValueChanged);
            ignoreEmptySubscriber.RegisterValueChangedCallback(OnValueChanged);
            loaderStrategy.RegisterValueChangedCallback(OnValueChanged);
            pooledStrategy.RegisterValueChangedCallback(OnValueChanged);
            messageScope.RegisterValueChangedCallback(OnValueChanged);
            logEnvironment.RegisterValueChangedCallback(OnValueChanged);

            slimPublishingContext.WithBindProperty(context.GetSlimPublishingContext());
            ignoreEmptySubscriber.WithBindProperty(context.GetIgnoreEmptySubscriber());
            loaderStrategy.WithBindProperty(context.GetLoaderStrategyProperty());
            pooledStrategy.WithBindProperty(context.GetPooledGameObjectStrategyProperty());
            messageScope.WithBindProperty(context.GetMessageScopeProperty());
            logEnvironment.WithBindProperty(context.GetLogEnvironmentProperty());

            contentContainer.WithBind(serializedSettings);
        }

        public void Update()
        {
            if (_valueUpdated == false)
            {
                return;
            }

            _valueUpdated = false;

            var serializedObject = _context.Object;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (evt == null)
            {
                return;
            }

            if (evt.newValue.Equals(evt.previousValue) == false)
            {
                _valueUpdated = true;
            }
        }

        private void OnValueChanged(ChangeEvent<Enum> evt)
        {
            if (evt == null)
            {
                return;
            }

            try
            {
                if (evt.newValue.Equals(evt.previousValue) == false)
                {
                    _valueUpdated = true;
                }
            }
            catch { }
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
                root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
                root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

                var button = new Button(OpenSettingsWindow) {
                    text = "Open Mono Page Flow Settings Window",
                };

                button.AddToClassList("button-open-settings-window");
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

#endif
