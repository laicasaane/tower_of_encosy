#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.ConfigKeys;
using UnityEditor;

namespace EncosyTower.Editor.ConfigKeys
{
    public static class ConfigKeyEditorPrefExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasEditorPref(this ConfigKey<string> self)
        {
            return EditorPrefs.HasKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasEditorPref(this ConfigKey<bool> self)
        {
            return EditorPrefs.HasKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasEditorPref(this ConfigKey<int> self)
        {
            return EditorPrefs.HasKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasEditorPref(this ConfigKey<float> self)
        {
            return EditorPrefs.HasKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<string> GetEditorPref(this ConfigKey<string> self)
        {
            string key = self.Value;
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetString(key) : Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<bool> GetEditorPref(this ConfigKey<bool> self)
        {
            string key = self.Value;
            return EditorPrefs.HasKey(key) ? new Option<bool>(EditorPrefs.GetInt(key) != 0) : Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<int> GetEditorPref(this ConfigKey<int> self)
        {
            string key = self.Value;
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetInt(key) : Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<float> GetEditorPref(this ConfigKey<float> self)
        {
            string key = self.Value;
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetEditorPref(this ConfigKey<string> self, string defaultValue)
        {
            return EditorPrefs.GetString(self.Value, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetEditorPref(this ConfigKey<bool> self, bool defaultValue)
        {
            return EditorPrefs.GetInt(self.Value, defaultValue ? 1 : 0) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetEditorPref(this ConfigKey<int> self, int defaultValue)
        {
            return EditorPrefs.GetInt(self.Value, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetEditorPref(this ConfigKey<float> self, float defaultValue)
        {
            return EditorPrefs.GetFloat(self.Value, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorPref(this ConfigKey<string> self, string value)
        {
            EditorPrefs.SetString(self.Value, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorPref(this ConfigKey<bool> self, bool value)
        {
            EditorPrefs.SetInt(self.Value, value ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorPref(this ConfigKey<int> self, int value)
        {
            EditorPrefs.SetInt(self.Value, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorPref(this ConfigKey<float> self, float value)
        {
            EditorPrefs.SetFloat(self.Value, value);
        }

        public static void SetEditorPref(this ConfigKey<string> self, string value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || EditorPrefs.HasKey(key) == false)
            {
                EditorPrefs.SetString(key, value);
            }
        }

        public static void SetEditorPref(this ConfigKey<bool> self, bool value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || EditorPrefs.HasKey(key) == false)
            {
                EditorPrefs.SetInt(key, value ? 1 : 0);
            }
        }

        public static void SetEditorPref(this ConfigKey<int> self, int value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || EditorPrefs.HasKey(key) == false)
            {
                EditorPrefs.SetInt(key, value);
            }
        }

        public static void SetEditorPref(this ConfigKey<float> self, float value, bool overrideExisting)
        {
            string key = self.Value;

            if (overrideExisting || EditorPrefs.HasKey(key) == false)
            {
                EditorPrefs.SetFloat(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteEditorPref(this ConfigKey<string> self)
        {
            EditorPrefs.DeleteKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteEditorPref(this ConfigKey<bool> self)
        {
            EditorPrefs.DeleteKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteEditorPref(this ConfigKey<int> self)
        {
            EditorPrefs.DeleteKey(self.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteEditorPref(this ConfigKey<float> self)
        {
            EditorPrefs.DeleteKey(self.Value);
        }
    }
}

#endif
