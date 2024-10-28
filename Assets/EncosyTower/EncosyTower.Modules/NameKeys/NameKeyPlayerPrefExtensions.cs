using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.NameKeys
{
    public static class NameKeyPlayerPrefExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref<T>(this NameKey<T> self)
        {
            return PlayerPrefs.HasKey(self.ToStringGlobal());
        }

        public static Option<string> GetPlayerPref(this NameKey<string> self)
        {
            var key = self.ToStringGlobal();

            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            return default;
        }

        public static Option<bool> GetPlayerPref(this NameKey<bool> self)
        {
            var key = self.ToStringGlobal();

            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetInt(key) != 0;
            }

            return default;
        }

        public static Option<int> GetPlayerPref(this NameKey<int> self)
        {
            var key = self.ToStringGlobal();

            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetInt(key);
            }

            return default;
        }

        public static Option<float> GetPlayerPref(this NameKey<float> self)
        {
            var key = self.ToStringGlobal();

            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetFloat(key);
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPlayerPref(this NameKey<string> self, string defaultValue)
        {
            return PlayerPrefs.GetString(self.ToStringGlobal(), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetPlayerPref(this NameKey<bool> self, bool defaultValue)
        {
            return PlayerPrefs.GetInt(self.ToStringGlobal(), defaultValue ? 1 : 0) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPlayerPref(this NameKey<int> self, int defaultValue)
        {
            return PlayerPrefs.GetInt(self.ToStringGlobal(), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetPlayerPref(this NameKey<float> self, float defaultValue)
        {
            return PlayerPrefs.GetFloat(self.ToStringGlobal(), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this NameKey<string> self, string value)
        {
            PlayerPrefs.SetString(self.ToStringGlobal(), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this NameKey<bool> self, bool value)
        {
            PlayerPrefs.SetInt(self.ToStringGlobal(), value ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this NameKey<int> self, int value)
        {
            PlayerPrefs.SetInt(self.ToStringGlobal(), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this NameKey<float> self, float value)
        {
            PlayerPrefs.SetFloat(self.ToStringGlobal(), value);
        }

        public static void SetPlayerPref(this NameKey<string> self, string value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetString(key, value);
            }
        }

        public static void SetPlayerPref(this NameKey<bool> self, bool value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value ? 1 : 0);
            }
        }

        public static void SetPlayerPref(this NameKey<int> self, int value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value);
            }
        }

        public static void SetPlayerPref(this NameKey<float> self, float value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref<T>(this NameKey<T> self)
        {
            PlayerPrefs.DeleteKey(self.ToStringGlobal());
        }
    }
}
