using System;
using EncosyTower.UnityExtensions;

namespace EncosyTower.Settings
{
    public static class SettingsAPI
    {
        /// <summary>
        /// Returns the full asset path to the settings file.
        /// </summary>
        public static string GetSettingsPath(SettingsUsage usage)
        {
            const string ROOT = "Assets/Settings";

            return usage switch {
                SettingsUsage.RuntimeProject => $"{ROOT}/Resources/",
                SettingsUsage.EditorProject => $"{ROOT}/Editor/",
                SettingsUsage.EditorUser => $"{ROOT}/Editor/User/{ApplicationAPI.GetProjectFolderName()}/",
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
