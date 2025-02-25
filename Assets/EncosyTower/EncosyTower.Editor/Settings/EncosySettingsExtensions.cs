// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
// https://github.com/hextantstudios/com.hextantstudios.utilities/blob/master/Editor/Settings/SettingsExtensions.cs

using UnityEditor;
using EncosyTower.Settings;
using EncosyTower.Types;

namespace EncosyTower.Editor.Settings
{
    public static class EncosySettingsExtensions
    {
        private const string PREFERENCES_PATH = "Preferences/";
        private const string PROJECT_PATH = "Project/";

        /// <summary>
        /// Gets the <see cref="SettingsProvider"/> instance used to display settings in either
        /// <code>Edit/Preferences</code> or <code>Edit/Project Settings</code>.
        /// </summary>
        public static ScriptableObjectSettingsProvider GetSettingsProvider<T>(this Settings<T> self, bool useImgui)
            where T : Settings<T>
        {
            var attribute = Settings<T>.Attribute;
            var scope = attribute.Usage == SettingsUsage.EditorUser
                ? SettingsScope.User
                : SettingsScope.Project;

            var path = attribute.Usage == SettingsUsage.EditorUser
                ? PREFERENCES_PATH
                : PROJECT_PATH;

            var displayPath = string.IsNullOrEmpty(attribute.DisplayPath)
                ? ObjectNames.NicifyVariableName(typeof(T).GetNameWithoutSuffix(nameof(Settings<T>)))
                : attribute.DisplayPath;

            return new ScriptableObjectSettingsProvider(self, scope, $"{path}{displayPath}", useImgui);
        }

        public static void OpenSettingsWindow<T>(this Settings<T> self)
            where T : Settings<T>
        {
            var attribute = Settings<T>.Attribute;
            var scope = attribute.Usage == SettingsUsage.EditorUser
                ? SettingsScope.User
                : SettingsScope.Project;

            var path = attribute.Usage == SettingsUsage.EditorUser
                ? PREFERENCES_PATH
                : PROJECT_PATH;

            var displayPath = string.IsNullOrEmpty(attribute.DisplayPath)
                ? ObjectNames.NicifyVariableName(typeof(T).GetNameWithoutSuffix(nameof(Settings<T>)))
                : attribute.DisplayPath;

            var settingsPath = $"{path}{displayPath}";
            SettingsService.OpenProjectSettings(settingsPath);
        }
    }
}
