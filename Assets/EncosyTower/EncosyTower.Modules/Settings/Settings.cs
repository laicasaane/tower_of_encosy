// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
// https://github.com/hextantstudios/com.hextantstudios.utilities/blob/master/Runtime/Settings/Settings.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Editor;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Types;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Settings
{
    /// <summary>
    /// Base class for project/users settings. Use the <code>[Settings]</code> attribute to
    /// specify its usage, display path, and filename.
    /// <list type="bullet">
    /// <item>Derived classes MUST be placed in a file with the same name as the class.</item>
    /// <item>Settings are stored in <code>Assets/Settings/</code> folder.</item>
    /// <item>The user folder <code>Assets/Settings/Editor/User/</code> MUST be excluded from source control.</item>
    /// <item>
    ///     User settings will be placed in a subdirectory named the same as the current project folder
    ///     so that shallow cloning (symbolic links to the <code>Assets/</code> folder) can be used
    ///     when testing multiplayer games.
    /// </item>
    /// </list>
    /// </summary>
    /// <seealso href="https://HextantStudios.com/unity-custom-settings/"/>
    public abstract class Settings<T> : ScriptableObject where T : Settings<T>
    {
        private static SettingsAttribute s_attribute;
        private static T s_instance;

        public static SettingsAttribute Attribute => s_attribute
            ??= typeof(T).GetCustomAttribute<SettingsAttribute>(true);

        public static T Instance => s_instance.IsValid() ? s_instance : Initialize();

        /// <summary>
        /// Loads or creates the settings instance and stores it in <see cref="Instance"/>.
        /// </summary>
        protected static T Initialize()
        {
            // If the instance is already valid, return it. Needed if called from a
            // derived class that wishes to ensure the settings are initialized.
            if (s_instance.IsValid())
            {
                return s_instance;
            }

            var type = typeof(T);
            var attribute = Attribute ?? throw new InvalidOperationException(
                $"[Settings] attribute is missing for type: {type.Name}"
            );

            // Attempt to load the settings asset.
            var filename = attribute.Filename ?? type.Name;
            var path = $"{SettingsAPI.GetSettingsPath(attribute.Usage)}{filename}.asset";

            if (attribute.Usage == SettingsUsage.RuntimeProject)
            {
                s_instance = Resources.Load<T>(filename);
            }
#if UNITY_EDITOR
            else
            {
                s_instance = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            // Return the instance if it was the load was successful.
            if (s_instance.IsValid())
            {
                return s_instance;
            }

            // Move settings if its path changed (type renamed or attribute changed)
            // while the editor was running. This must be done manually if the
            // change was made outside the editor.
            if (AssetDatabaseAPI.FindFirstObjectByGlobalQualifiedType<T>(out var instance))
            {
                var oldPath = AssetDatabase.GetAssetPath(instance);
                var result = AssetDatabase.MoveAsset(oldPath, path);

                if (string.IsNullOrEmpty(result))
                {
                    return s_instance = instance;
                }
                else
                {
                    DevLoggerAPI.LogWarningFormat(
                        $"Failed to move previous settings asset '{oldPath}' to '{path}'. " +
                        $"A new settings asset will be created."
                        , s_instance
                    );
                }
            }
#endif

            // Create the settings instance if it was not loaded or found.
            if (s_instance.IsValid())
            {
                return s_instance;
            }

            s_instance = CreateInstance<T>();

#if UNITY_EDITOR
            // Verify the derived class is in a file with the same name.
            var script = MonoScript.FromScriptableObject(s_instance);

            if (script.IsInvalid() || string.Equals(script.name, type.Name) == false)
            {
                DestroyImmediate(s_instance);
                s_instance = null;

                throw new InvalidOperationException(
                    $"Settings-derived class and filename must match: {type.Name}"
                );
            }

            // Create a new settings instance if it was not found.
            // Create the directory as Unity does not do this itself.
            Directory.CreateDirectory(Path.Combine(
                  Directory.GetCurrentDirectory()
                , Path.GetDirectoryName(path)
            ));

            // Create the asset only in the editor.
            AssetDatabase.CreateAsset(s_instance, path);
#endif

            return s_instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static string GetNameWithoutSuffix()
            => typeof(T).GetNameWithoutSuffix(nameof(Settings<T>));

        /// <summary>
        /// Called to validate settings changes.
        /// </summary>
        protected virtual void OnValidate() { }

        /// <summary>
        /// Sets the specified setting to the desired value and marks the settings
        /// so that it will be saved.
        /// </summary>
        protected void Set<TSetting>(ref TSetting setting, TSetting value)
        {
            if (EqualityComparer<TSetting>.Default.Equals(setting, value))
            {
                return;
            }

            setting = value;
            OnValidate();
            SetDirty();
        }

        /// <summary>
        /// Marks the settings dirty so that it will be saved.
        /// </summary>
        protected new void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Base class for settings contained by a <see cref="Settings{T}"/> instance.
        /// </summary>
        [Serializable]
        public abstract class SubSettings
        {
            /// <summary>
            /// Called when a setting is modified.
            /// </summary>
            protected virtual void OnValidate() { }

            /// <summary>
            /// Sets the specified setting to the desired value and marks the settings
            /// instance so that it will be saved.
            /// </summary>
            protected void Set<TSetting>(ref TSetting setting, TSetting value)
            {
                if (EqualityComparer<TSetting>.Default.Equals(setting, value))
                {
                    return;
                }

                setting = value;
                OnValidate();
                Instance.SetDirty();
            }
        }
    }
}
