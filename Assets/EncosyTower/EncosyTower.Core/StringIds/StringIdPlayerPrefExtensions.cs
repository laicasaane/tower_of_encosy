using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public static class StringIdPlayerPrefExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPlayerPref<T>(this StringId<T> self)
        {
            return PlayerPrefs.HasKey(self.ToStringGlobal());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<string> GetPlayerPref(this StringId<string> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<bool> GetPlayerPref(this StringId<bool> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<int> GetPlayerPref(this StringId<int> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<float> GetPlayerPref(this StringId<float> self)
        {
            var key = self.ToStringGlobal();
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPlayerPref(this StringId<string> self, string defaultValue)
        {
            return PlayerPrefs.GetString(self.ToStringGlobal(), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetPlayerPref(this StringId<bool> self, bool defaultValue)
        {
            return PlayerPrefs.GetInt(self.ToStringGlobal(), defaultValue ? 1 : 0) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPlayerPref(this StringId<int> self, int defaultValue)
        {
            return PlayerPrefs.GetInt(self.ToStringGlobal(), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetPlayerPref(this StringId<float> self, float defaultValue)
        {
            return PlayerPrefs.GetFloat(self.ToStringGlobal(), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<string> self, string value)
        {
            PlayerPrefs.SetString(self.ToStringGlobal(), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<bool> self, bool value)
        {
            PlayerPrefs.SetInt(self.ToStringGlobal(), value ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<int> self, int value)
        {
            PlayerPrefs.SetInt(self.ToStringGlobal(), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerPref(this StringId<float> self, float value)
        {
            PlayerPrefs.SetFloat(self.ToStringGlobal(), value);
        }

        public static void SetPlayerPref(this StringId<string> self, string value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetString(key, value);
            }
        }

        public static void SetPlayerPref(this StringId<bool> self, bool value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value ? 1 : 0);
            }
        }

        public static void SetPlayerPref(this StringId<int> self, int value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, value);
            }
        }

        public static void SetPlayerPref(this StringId<float> self, float value, bool overrideExisting)
        {
            var key = self.ToStringGlobal();

            if (overrideExisting || PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeletePlayerPref<T>(this StringId<T> self)
        {
            PlayerPrefs.DeleteKey(self.ToStringGlobal());
        }
    }
}
