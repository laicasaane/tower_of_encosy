using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.ConfigKeys
{
    public static class ConfigKeyPlayerPrefExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref<T>(this ConfigKey<T> self)
        {
            return PlayerPrefs.HasKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<string> GetPlayerPref(this ConfigKey<string> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<bool> GetPlayerPref(this ConfigKey<bool> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<int> GetPlayerPref(this ConfigKey<int> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<float> GetPlayerPref(this ConfigKey<float> self)
        {
            string key = self.Value;
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPlayerPref(this ConfigKey<string> self, string defaultValue)
        {
            return PlayerPrefs.GetString(self.Value, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetPlayerPref(this ConfigKey<bool> self, bool defaultValue)
        {
            return PlayerPrefs.GetInt(self.Value, defaultValue ? 1 : 0) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPlayerPref(this ConfigKey<int> self, int defaultValue)
        {
            return PlayerPrefs.GetInt(self.Value, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetPlayerPref(this ConfigKey<float> self, float defaultValue)
        {
            return PlayerPrefs.GetFloat(self.Value, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<string> self, string value)
        {
            PlayerPrefs.SetString(self.Value, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<bool> self, bool value)
        {
            PlayerPrefs.SetInt(self.Value, value ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<int> self, int value)
        {
            PlayerPrefs.SetInt(self.Value, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this ConfigKey<float> self, float value)
        {
            PlayerPrefs.SetFloat(self.Value, value);
        }

        public static void SetPlayerPref(this ConfigKey<string> self, string value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetString(key, value);
            }
        }

        public static void SetPlayerPref(this ConfigKey<bool> self, bool value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value ? 1 : 0);
            }
        }

        public static void SetPlayerPref(this ConfigKey<int> self, int value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value);
            }
        }

        public static void SetPlayerPref(this ConfigKey<float> self, float value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref<T>(this ConfigKey<T> self)
        {
            PlayerPrefs.DeleteKey(self.Value);
        }
    }
}
