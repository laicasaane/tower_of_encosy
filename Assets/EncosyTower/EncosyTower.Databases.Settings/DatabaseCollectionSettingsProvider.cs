using EncosyTower.Databases.Settings.Views;
using EncosyTower.Editor.Settings;
using EncosyTower.Types;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings
{
    internal static class DatabaseCollectionSettingsProvider
    {
        private static DatabaseCollectionSettingsEditor s_instance;

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            DatabaseTypeVault.Initialize();

            var provider = DatabaseCollectionSettings.Instance.GetSettingsProvider(useImgui: false);
            var label = provider.Settings.GetType().GetNameWithoutSuffix(nameof(Settings));
            provider.label = ObjectNames.NicifyVariableName(label);
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        private static void Create(
              ScriptableObjectSettingsProvider provider
            , VisualElement root
        )
        {
            s_instance = new DatabaseCollectionSettingsEditor(
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
            s_instance?.Save();
            s_instance = null;
        }
    }
}
