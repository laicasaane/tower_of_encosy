#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using EncosyTower.Collections.Extensions;
using EncosyTower.Core;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    public static class BuildAPI
    {
        [ApiForEditor]
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

        [ApiForEditor]
        public static BuildProfile ActiveBuildProfile
        {
            get
            {
                return BuildProfile.GetActiveBuildProfile();
            }
        }

        [ApiForEditor]
        public static BuildProfileOrTarget ActiveProfileOrTarget
        {
            get
            {
                var profile = ActiveBuildProfile;
                return profile.IsValid() ? profile : ActiveNamedBuildTarget;
            }
        }

        [ApiForEditor]
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

        [ApiForEditor]
        public static NamedBuildTarget GetNamedBuildTarget(BuildTarget buildTarget)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            return namedBuildTarget;
        }

        [ApiForEditor]
        public static NamedBuildTarget GetNamedBuildTarget(BuildTargetGroup targetGroup)
        {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            return namedBuildTarget;
        }

        [ApiForEditor]
        public static HashSet<string> GetScriptingDefineSymbols(BuildProfile profile)
        {
            var result = new HashSet<string>();

            if (profile.IsValid())
            {
                result.AddRangeFast(profile.scriptingDefines.AsSpan());
            }

            return result;
        }

        [ApiForEditor]
        public static void SetScriptingDefineSymbols(BuildProfile profile, params string[] symbols)
        {
            if (profile.IsValid() == false)
            {
                return;
            }

            profile.scriptingDefines = symbols;
            EditorUtility.SetDirty(profile);
        }

        [ApiForEditor]
        public static void SetScriptingDefineSymbols(BuildProfile profile, IEnumerable<string> symbols)
        {
            if (profile.IsValid() == false)
            {
                return;
            }

            profile.scriptingDefines = symbols.ToArray();
            EditorUtility.SetDirty(profile);
        }

        [ApiForEditor]
        public static HashSet<string> GetScriptingDefineSymbols(NamedBuildTarget buildTarget)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbols(buildTarget) ?? string.Empty;
            var symbols = symbolStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(symbols);
        }

        [ApiForEditor]
        public static void SetScriptingDefineSymbols(NamedBuildTarget buildTarget, params string[] symbols)
        {
            var symbolStr = string.Join(';', symbols);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbolStr);
        }

        [ApiForEditor]
        public static void SetScriptingDefineSymbols(NamedBuildTarget buildTarget, IEnumerable<string> symbols)
        {
            var symbolStr = string.Join(';', symbols);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbolStr);
        }

        [ApiForEditor]
        public static void AddScriptingDefineSymbols(NamedBuildTarget buildTarget, params string[] symbols)
        {
            var result = GetScriptingDefineSymbols(buildTarget);
            result.UnionWith(symbols);

            SetScriptingDefineSymbols(buildTarget, result);
        }

        [ApiForEditor]
        public static void AddScriptingDefineSymbols(NamedBuildTarget buildTarget, IEnumerable<string> symbols)
        {
            var result = GetScriptingDefineSymbols(buildTarget);
            result.UnionWith(symbols);

            SetScriptingDefineSymbols(buildTarget, result);
        }

        [ApiForEditor]
        public static void RemoveScriptingDefineSymbols(NamedBuildTarget buildTarget, params string[] symbols)
        {
            var result = GetScriptingDefineSymbols(buildTarget);
            result.ExceptWith(symbols);

            SetScriptingDefineSymbols(buildTarget, result);
        }

        [ApiForEditor]
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
