// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
// https://github.com/hextantstudios/com.hextantstudios.utilities/blob/master/Editor/Settings/SettingsExtensions.cs

using UnityEditor;
using EncosyTower.Modules.Settings;

namespace EncosyTower.Modules.Editor.Settings
{
    public static class EncosySettingsExtensions
    {
        /// <summary>
        /// Gets the <see cref="SettingsProvider"/> instance used to display settings in either
        /// <code>Edit/Preferences</code> or <code>Edit/Project Settings</code>.
        /// </summary>
        public static SettingsProvider GetSettingsProvider<T>(this Settings<T> self) where T : Settings<T>
        {
            var attribute = Settings<T>.Attribute;
            var scope = attribute.Usage == SettingsUsage.EditorUser
                ? SettingsScope.User
                : SettingsScope.Project;

            var displayPath = string.IsNullOrEmpty(attribute.DisplayPath)
                ? ObjectNames.NicifyVariableName(typeof(T).Name)
                : attribute.DisplayPath;

            return new ScriptableObjectSettingsProvider(self, scope, displayPath);
        }
    }
}
