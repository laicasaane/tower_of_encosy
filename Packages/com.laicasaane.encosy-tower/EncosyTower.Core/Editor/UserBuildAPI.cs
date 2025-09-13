#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace EncosyTower.Editor
{
    public static class UserBuildAPI
    {
        public static NamedBuildTarget CurrentBuildTarget
        {
            get
            {
#if UNITY_SERVER
                return NamedBuildTarget.Server;
#else
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                return namedBuildTarget;
#endif
            }
        }

        public static HashSet<string> GetScriptingDefineSymbols(NamedBuildTarget buildTarget)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            var symbols = symbolStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(symbols);
        }

        public static void SetScriptingDefineSymbols(NamedBuildTarget buildTarget, params string[] symbols)
        {
            var symbolStr = string.Join(';', symbols);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbolStr);
            AssetDatabase.SaveAssets();
        }

        public static void SetScriptingDefineSymbols(NamedBuildTarget buildTarget, IEnumerable<string> symbols)
        {
            var symbolStr = string.Join(';', symbols);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbolStr);
            AssetDatabase.SaveAssets();
        }

        public static void AddScriptingDefineSymbols(NamedBuildTarget buildTarget, params string[] symbols)
        {
            var result = GetScriptingDefineSymbols(buildTarget);
            result.UnionWith(symbols);

            SetScriptingDefineSymbols(buildTarget, result);
        }

        public static void AddScriptingDefineSymbols(NamedBuildTarget buildTarget, IEnumerable<string> symbols)
        {
            var result = GetScriptingDefineSymbols(buildTarget);
            result.UnionWith(symbols);

            SetScriptingDefineSymbols(buildTarget, result);
        }

        public static void RemoveScriptingDefineSymbols(NamedBuildTarget buildTarget, params string[] symbols)
        {
            var result = GetScriptingDefineSymbols(buildTarget);
            result.ExceptWith(symbols);

            SetScriptingDefineSymbols(buildTarget, result);
        }

        public static void RemoveScriptingDefineSymbols(NamedBuildTarget buildTarget, IEnumerable<string> symbols)
        {
            var result = GetScriptingDefineSymbols(buildTarget);
            result.ExceptWith(symbols);

            SetScriptingDefineSymbols(buildTarget, result);
        }
    }
}

#endif
