using EncosyTower.Editor.Settings;
using EncosyTower.Types;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Variants.Settings
{
    internal static class VariantTypeSettingsProvider
    {
        private static VariantTypeSettingsEditor s_instance;

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = VariantTypeSettings.Instance.GetSettingsProvider(useImgui: false);
            var label = provider.Settings.GetType().GetNameWithoutSuffix(nameof(Settings));
            provider.label = ObjectNames.NicifyVariableName(label);
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Project Settings/Variant Type", priority = 80_86_00_00)]
        private static void OpenSettings()
            => VariantTypeSettings.Instance.OpenSettingsWindow();

        private static void Create(
              ScriptableObjectSettingsProvider provider
            , VisualElement root
        )
        {
            s_instance = new VariantTypeSettingsEditor(
                  provider.Settings as VariantTypeSettings
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
            s_instance?.Save();
            s_instance = null;
        }
    }
}
