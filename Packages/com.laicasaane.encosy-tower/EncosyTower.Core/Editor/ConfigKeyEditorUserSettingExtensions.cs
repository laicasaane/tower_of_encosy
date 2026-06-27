#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.ConfigKeys;
using EncosyTower.Core;
using UnityEditor;

namespace EncosyTower.Editor.ConfigKeys
{
    [ApiForEditor]
    public static class ConfigKeyEditorUserSettingExtensions
    {
        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<string> GetEditorUserSetting(this ConfigKey<string> self)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return Option.SomeIf(string.IsNullOrEmpty(str) == false, str);
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<bool> GetEditorUserSetting(this ConfigKey<bool> self)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return Option.SomeIf(bool.TryParse(str, out var result), result);
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<int> GetEditorUserSetting(this ConfigKey<int> self)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return Option.SomeIf(int.TryParse(str, out var result), result);
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<float> GetEditorUserSetting(this ConfigKey<float> self)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return Option.SomeIf(float.TryParse(str, out var result), result);
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetEditorUserSetting(this ConfigKey<string> self, string defaultValue)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return str.NotEmptyOr(defaultValue);
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetEditorUserSetting(this ConfigKey<bool> self, bool defaultValue)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return bool.TryParse(str, out var result) ? result : defaultValue;
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetEditorUserSetting(this ConfigKey<int> self, int defaultValue)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return int.TryParse(str, out var result) ? result : defaultValue;
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetEditorUserSetting(this ConfigKey<float> self, float defaultValue)
        {
            var str = EditorUserSettings.GetConfigValue(self.Value);
            return float.TryParse(str, out var result) ? result : defaultValue;
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorUserSetting(this ConfigKey<string> self, string value)
        {
            EditorUserSettings.SetConfigValue(self.Value, value);
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorUserSetting(this ConfigKey<bool> self, bool value)
        {
            EditorUserSettings.SetConfigValue(self.Value, value ? bool.TrueString : bool.FalseString);
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorUserSetting(this ConfigKey<int> self, int value)
        {
            EditorUserSettings.SetConfigValue(self.Value, $"{value}");
        }

        [ApiForEditor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEditorUserSetting(this ConfigKey<float> self, float value)
        {
            EditorUserSettings.SetConfigValue(self.Value, $"{value}");
        }
    }
}

#endif
