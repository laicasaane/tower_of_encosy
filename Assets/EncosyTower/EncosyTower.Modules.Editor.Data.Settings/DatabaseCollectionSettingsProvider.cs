using EncosyTower.Modules.Editor.Data.Settings.Views;
using EncosyTower.Modules.Editor.Settings;
using EncosyTower.Modules.Types;
using UnityEditor;
using UnityEngine.Scripting;

namespace EncosyTower.Modules.Editor.Data.Settings
{
    internal static class DatabaseCollectionSettingsProvider
    {
        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            DatabaseTypeVault.Initialize();

            var provider = DatabaseCollectionSettings.Instance.GetSettingsProvider(useImgui: false);
            var label = provider.Settings.GetType().GetNameWithoutSuffix(nameof(Settings));
            provider.label = ObjectNames.NicifyVariableName(label);
            provider.activateHandler = (_, r) => DatabaseCollectionSettingsEditor.Create(provider, r);
            provider.inspectorUpdateHandler = static () => DatabaseCollectionSettingsEditor.Instance.Update();
            provider.deactivateHandler = DatabaseCollectionSettingsEditor.Dispose;

            return provider;
        }
    }
}
