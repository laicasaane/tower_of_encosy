#if UNITY_EDITOR

using System;
using EncosyTower.Common;
using EncosyTower.Editor.Settings;
using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.Pooling;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings.Views
{
    internal class MonoPageFlowSettingsEditor
    {
        public const string SYMBOL_ALWAYS = "ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO";
        public const string SYMBOL_FOR_DEV = "ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV";

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

            var warnNoSubscriber = new Toggle("Warn No Subscriber");
            var loaderStrategy = new EnumField("Loader Strategy", default(MonoPageLoaderStrategy));
            var messageScope = new EnumField("Message Scope", default(MonoMessageScope));
            var logEnvironment = new EnumField("Log Environment", default(LogEnvironment));
            var poolFoldout = new Foldout { text = "GameObject Pooling Strategies" };
            var poolRentingStrategy = new EnumField("Renting", default(RentingStrategy));
            var poolReturningStrategy = new EnumField("Returning", default(ReturningStrategy));
            var callerInfoFoldout = new Foldout { text = "Caller Info Option For Publishing Page Flow Messages" };
            var callerInfoOption = new EnumField("Option", default(PubSubCallerInfoOption)) {
                value = default(PageFlowPublishingContext).CallerInfoOption,
            };

            var callerInfoOptionHelpBox = new HelpBox {
                messageType = HelpBoxMessageType.Info,
                text =
                    "Choose whether to include caller info when publishing page flow messages.\n" +
                    "Including caller info can be useful for debugging purposes, but may increase build size.\n" +
                    "- <b>Never:</b> Do not include caller info.\n" +
                    "- <b>For Development:</b> Include only for Editor and Development builds.\n" +
                    $"\tSymbol '{SYMBOL_FOR_DEV}' will be added to Player Settings.\n" +
                    "- <b>Always:</b> Include for all environments.\n" +
                    $"\tSymbol '{SYMBOL_ALWAYS}' will be added to Player Settings.",
            };

            contentContainer.Add(warnNoSubscriber.WithAlignFieldClass());
            contentContainer.Add(loaderStrategy.WithAlignFieldClass());
            contentContainer.Add(messageScope.WithAlignFieldClass());
            contentContainer.Add(logEnvironment.WithAlignFieldClass());

            contentContainer.Add(poolFoldout);
            poolFoldout.Add(poolRentingStrategy.WithAlignFieldClass());
            poolFoldout.Add(poolReturningStrategy.WithAlignFieldClass());

            contentContainer.Add(callerInfoFoldout);
            callerInfoFoldout.Add(callerInfoOption.WithAlignFieldClass());
            callerInfoFoldout.Add(callerInfoOptionHelpBox.WithAlignFieldClass());

            callerInfoFoldout.RegisterValueChangedCallback(OnValueChanged);
            warnNoSubscriber.RegisterValueChangedCallback(OnValueChanged);
            loaderStrategy.RegisterValueChangedCallback(OnValueChanged);
            poolRentingStrategy.RegisterValueChangedCallback(OnValueChanged);
            poolReturningStrategy.RegisterValueChangedCallback(OnValueChanged);
            messageScope.RegisterValueChangedCallback(OnValueChanged);
            logEnvironment.RegisterValueChangedCallback(OnValueChanged);
            callerInfoOption.RegisterValueChangedCallback(IncludeCallerInfoToggle_OnValueChanged);

            warnNoSubscriber.WithBindProperty(context.GetWarnNoSubscriber());
            loaderStrategy.WithBindProperty(context.GetLoaderStrategyProperty());
            poolRentingStrategy.WithBindProperty(context.GetPoolRentingStrategyProperty());
            poolReturningStrategy.WithBindProperty(context.GetPoolReturningStrategyProperty());
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

        private void IncludeCallerInfoToggle_OnValueChanged(ChangeEvent<Enum> evt)
        {
            if (evt.newValue is not PubSubCallerInfoOption newValue)
            {
                return;
            }

            var currentOption = default(PageFlowPublishingContext).CallerInfoOption;

            if (currentOption == newValue)
            {
                return;
            }

            var buildTargets = BuildAPI.GetSupportedNamedBuildTargets();

            foreach (var buildTarget in buildTargets)
            {
                BuildAPI.RemoveScriptingDefineSymbols(
                      buildTarget
                    , SYMBOL_FOR_DEV
                    , SYMBOL_ALWAYS
                );
            }

            var symbol = newValue switch
            {
                PubSubCallerInfoOption.ForDevelopment => SYMBOL_FOR_DEV,
                PubSubCallerInfoOption.Always => SYMBOL_ALWAYS,
                _ => string.Empty,
            };

            if (symbol.IsNotEmpty())
            {
                foreach (var buildTarget in buildTargets)
                {
                    BuildAPI.AddScriptingDefineSymbols(buildTarget, symbol);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
