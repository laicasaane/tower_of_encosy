using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.ConfigKeys
{
    public static class ConfigKeyPlayerPrefExtensions
    {
        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this ConfigKey<string> self)
        {
            return PlayerPrefs.HasKey(self.Value);
        }

        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this ConfigKey<bool> self)
        {
            return PlayerPrefs.HasKey(self.Value);
        }

        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this ConfigKey<int> self)
        {
            return PlayerPrefs.HasKey(self.Value);
        }

        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this ConfigKey<float> self)
        {
            return PlayerPrefs.HasKey(self.Value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<string> GetPlayerPref(this ConfigKey<string> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : default(Option<string>);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<bool> GetPlayerPref(this ConfigKey<bool> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) ? new Option<bool>(PlayerPrefs.GetInt(key) != 0) : default;
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<int> GetPlayerPref(this ConfigKey<int> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : default(Option<int>);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<float> GetPlayerPref(this ConfigKey<float> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : default(Option<float>);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPlayerPref(this ConfigKey<string> self, string defaultValue)
        {
            return PlayerPrefs.GetString(self.Value, defaultValue);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetPlayerPref(this ConfigKey<bool> self, bool defaultValue)
        {
            return PlayerPrefs.GetInt(self.Value, defaultValue ? 1 : 0) != 0;
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPlayerPref(this ConfigKey<int> self, int defaultValue)
        {
            return PlayerPrefs.GetInt(self.Value, defaultValue);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetPlayerPref(this ConfigKey<float> self, float defaultValue)
        {
            return PlayerPrefs.GetFloat(self.Value, defaultValue);
        }

        /// <summary>
        /// Sets a single string value for the preference identified by the given key.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<string> self, string value)
        {
            PlayerPrefs.SetString(self.Value, value);
        }

        /// <summary>
        /// Sets a single boolean value for the preference identified by the given key.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<bool> self, bool value)
        {
            PlayerPrefs.SetInt(self.Value, value ? 1 : 0);
        }

        /// <summary>
        /// Sets a single integer value for the preference identified by the given key.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<int> self, int value)
        {
            PlayerPrefs.SetInt(self.Value, value);
        }

        /// <summary>
        /// Sets a single float value for the preference identified by the given key.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<float> self, float value)
        {
            PlayerPrefs.SetFloat(self.Value, value);
        }

        /// <summary>
        /// Sets a single string value for the preference identified by the given key.
        /// </summary>
        /// <param name="overrideExisting">
        /// Determines whether the value of the given key can be overridden if existing.
        /// </param>
        public static void SetPlayerPref(this ConfigKey<string> self, string value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetString(key, value);
            }
        }

        /// <summary>
        /// Sets a single boolean value for the preference identified by the given key.
        /// </summary>
        /// <param name="overrideExisting">
        /// Determines whether the value of the given key can be overridden if existing.
        /// </param>
        public static void SetPlayerPref(this ConfigKey<bool> self, bool value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Sets a single integer value for the preference identified by the given key.
        /// </summary>
        /// <param name="overrideExisting">
        /// Determines whether the value of the given key can be overridden if existing.
        /// </param>
        public static void SetPlayerPref(this ConfigKey<int> self, int value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value);
            }
        }

        /// <summary>
        /// Sets a single float value for the preference identified by the given key.
        /// </summary>
        /// <param name="overrideExisting">
        /// Determines whether the value of the given key can be overridden if existing.
        /// </param>
        public static void SetPlayerPref(this ConfigKey<float> self, float value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this ConfigKey<string> self)
        {
            PlayerPrefs.DeleteKey(self.Value);
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this ConfigKey<bool> self)
        {
            PlayerPrefs.DeleteKey(self.Value);
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this ConfigKey<int> self)
        {
            PlayerPrefs.DeleteKey(self.Value);
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this ConfigKey<float> self)
        {
            PlayerPrefs.DeleteKey(self.Value);
        }
    }
}
