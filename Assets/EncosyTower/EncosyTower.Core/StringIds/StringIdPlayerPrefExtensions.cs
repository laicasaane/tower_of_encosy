using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public static class StringIdPlayerPrefExtensions
    {
        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this StringId<string> self)
        {
            return PlayerPrefs.HasKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this StringId<bool> self)
        {
            return PlayerPrefs.HasKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this StringId<int> self)
        {
            return PlayerPrefs.HasKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Returns true if the given key exists in PlayerPrefs, otherwise returns false.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref(this StringId<float> self)
        {
            return PlayerPrefs.HasKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<string> GetPlayerPref(this StringId<string> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : default;
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<bool> GetPlayerPref(this StringId<bool> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key) != 0;
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<int> GetPlayerPref(this StringId<int> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : default;
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<float> GetPlayerPref(this StringId<float> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : default;
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPlayerPref(this StringId<string> self, string defaultValue)
        {
            return PlayerPrefs.GetString(self.ToStringGlobal(), defaultValue);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetPlayerPref(this StringId<bool> self, bool defaultValue)
        {
            return PlayerPrefs.GetInt(self.ToStringGlobal(), defaultValue ? 1 : 0) != 0;
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPlayerPref(this StringId<int> self, int defaultValue)
        {
            return PlayerPrefs.GetInt(self.ToStringGlobal(), defaultValue);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetPlayerPref(this StringId<float> self, float defaultValue)
        {
            return PlayerPrefs.GetFloat(self.ToStringGlobal(), defaultValue);
        }

        /// <summary>
        /// Sets a single string value for the preference identified by the given key.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<string> self, string value)
        {
            PlayerPrefs.SetString(self.ToStringGlobal(), value);
        }

        /// <summary>
        /// Sets a single boolean value for the preference identified by the given key.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<bool> self, bool value)
        {
            PlayerPrefs.SetInt(self.ToStringGlobal(), value ? 1 : 0);
        }

        /// <summary>
        /// Sets a single integer value for the preference identified by the given key.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<int> self, int value)
        {
            PlayerPrefs.SetInt(self.ToStringGlobal(), value);
        }

        /// <summary>
        /// Sets a single float value for the preference identified by the given key.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<float> self, float value)
        {
            PlayerPrefs.SetFloat(self.ToStringGlobal(), value);
        }

        /// <summary>
        /// Sets a single string value for the preference identified by the given key.
        /// </summary>
        /// <param name="overrideExisting">
        /// Determines whether the value of the given key can be overridden if existing.
        /// </param>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        public static void SetPlayerPref(this StringId<string> self, string value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

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
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        public static void SetPlayerPref(this StringId<bool> self, bool value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

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
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        public static void SetPlayerPref(this StringId<int> self, int value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

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
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        public static void SetPlayerPref(this StringId<float> self, float value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref<T>(this StringId<T> self)
        {
            PlayerPrefs.DeleteKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this StringId<string> self)
        {
            PlayerPrefs.DeleteKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this StringId<bool> self)
        {
            PlayerPrefs.DeleteKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this StringId<int> self)
        {
            PlayerPrefs.DeleteKey(self.ToStringGlobal());
        }

        /// <summary>
        /// Removes the given key from the PlayerPrefs.
        /// If the key does not exist, DeleteKey has no impact.
        /// </summary>
        /// <remarks>
        /// This API uses <see cref="GlobalStringVault"/> to convert <paramref name="self"/> into key value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref(this StringId<float> self)
        {
            PlayerPrefs.DeleteKey(self.ToStringGlobal());
        }
    }
}
