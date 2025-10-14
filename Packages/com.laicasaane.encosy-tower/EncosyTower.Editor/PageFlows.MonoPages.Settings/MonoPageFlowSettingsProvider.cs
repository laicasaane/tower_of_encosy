#if UNITY_EDITOR

using EncosyTower.Editor.PageFlows.MonoPages.Settings.Views;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.Types;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings
{
    internal static class MonoPageFlowSettingsProvider
    {
        private static MonoPageFlowSettingsEditor s_instance;

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = MonoPageFlowSettings.Instance.GetSettingsProvider(useImgui: false);
            var label = provider.Settings.GetType().GetNameWithoutSuffix(nameof(Settings));
            provider.label = ObjectNames.NicifyVariableName(label);
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Project Settings/Mono Page Flow", priority = 80_77_00_00)]
        private static void OpenSettings()
            => MonoPageFlowSettings.Instance.OpenSettingsWindow();

        private static void Create(
              ScriptableObjectSettingsProvider provider
            , VisualElement root
        )
        {
            s_instance = new MonoPageFlowSettingsEditor(
                  provider.Settings
                , provider.SerializedSettings
                , root
            );
        }

        private static void Update()
        {
            s_instance?.Update();
        }

        private static void Dispose()
        {
            s_instance = null;
        }
    }
}

#endif
