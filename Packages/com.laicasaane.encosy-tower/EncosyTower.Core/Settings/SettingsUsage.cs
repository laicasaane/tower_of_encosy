// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
// https://github.com/hextantstudios/com.hextantstudios.utilities/blob/master/Runtime/Settings/SettingsUsage.cs

namespace EncosyTower.Settings
{
    /// <summary>
    /// Specifies how the settings are used and when they are available.
    /// </summary>
    public enum SettingsUsage
    {
        /// <summary>
        /// Project-wide settings available at runtime.
        /// For example <code>Project Settings/Time</code>.
        /// </summary>
        RuntimeProject,

        /// <summary>
        /// Project-wide settings available only in the editor.
        /// For example <code>Project Settings/Version Control</code>.
        /// </summary>
        EditorProject,

        /// <summary>
        /// User-specific settings available only in the editor.
        /// For example <code>Preferences/Scene View</code>.
        /// </summary>
        EditorUser,
    }
}
