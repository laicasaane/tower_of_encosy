#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace EncosyTower.Editor
{
    public static class BuildAPI
    {
        public static NamedBuildTarget ActiveNamedBuildTarget
        {
            get
            {
#if UNITY_SERVER
                return NamedBuildTarget.Server;
#else
                return GetNamedBuildTarget(EditorUserBuildSettings.activeBuildTarget);
#endif
            }
        }

        public static HashSet<NamedBuildTarget> GetSupportedNamedBuildTargets()
        {
            var buildTargets = BuildTargetExtensions.Values.AsSpan();
            var result = new HashSet<NamedBuildTarget>(buildTargets.Length);

            foreach (var buildTarget in buildTargets)
            {
                try
                {
                    var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

                    if (buildTargetGroup == BuildTargetGroup.Unknown
                        || BuildPipeline.IsBuildTargetSupported(buildTargetGroup, buildTarget) == false
                    )
                    {
                        continue;
                    }

                    var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                    result.Add(namedBuildTarget);
                }
                catch
                {
                    // Ignore unsupported target groups
                }
            }

            return result;
        }

        public static NamedBuildTarget GetNamedBuildTarget(BuildTarget buildTarget)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            return namedBuildTarget;
        }

        public static NamedBuildTarget GetNamedBuildTarget(BuildTargetGroup targetGroup)
        {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            return namedBuildTarget;
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
        }

        public static void SetScriptingDefineSymbols(NamedBuildTarget buildTarget, IEnumerable<string> symbols)
        {
            var symbolStr = string.Join(';', symbols);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbolStr);
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

        [MenuItem("Encosy Tower/Build Pipeline/List Supported Build Targets", priority = 66_80_00_00)]
        private static void ListSupportedBuildTargets()
        {
            var namedBuildTargets = GetSupportedNamedBuildTargets();

            foreach (var namedBuildTarget in namedBuildTargets)
            {
                UnityEngine.Debug.Log(namedBuildTarget.TargetName);
            }
        }
    }
}

#endif
