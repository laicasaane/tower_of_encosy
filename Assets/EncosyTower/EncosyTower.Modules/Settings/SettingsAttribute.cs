// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
// https://github.com/hextantstudios/com.hextantstudios.utilities/blob/master/Runtime/Settings/SettingsAttribute.cs

using System;

namespace EncosyTower.Modules.Settings
{
    /// <summary>
    /// Specifies the settings type, path in the settings UI, and optionally its
    /// filename. If the filename is not set, the type's name is used.
    /// </summary>
    /// <remarks>
    /// The displayPath can use a path separator '/' to create a Settings instance
    /// that is grouped or nested under another.
    /// For example <code>Services/My Project Settings</code>.
    /// </remarks>
    public sealed class SettingsAttribute : Attribute
    {
        /// <summary>
        /// The type of settings (how and when they are used).
        /// </summary>
        public readonly SettingsUsage Usage;

        /// <summary>
        /// The display name and optional path in the settings dialog.
        /// </summary>
        public readonly string DisplayPath;

        /// <summary>
        /// The filename used to store the settings. If null, the type's name is used.
        /// </summary>
        public readonly string Filename;

        public SettingsAttribute(SettingsUsage usage, string displayPath = null, string filename = null)
        {
            Usage = usage;
            Filename = filename;
            DisplayPath = displayPath;
        }
    }
}
