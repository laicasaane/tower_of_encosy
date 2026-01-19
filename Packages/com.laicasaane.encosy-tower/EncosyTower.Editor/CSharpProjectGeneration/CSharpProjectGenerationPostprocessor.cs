#if UNITY_SDK_STYLE_PROJECTS

using UnityEditor;

namespace EncosyTower.Editor.CSharpProjectGeneration
{
    internal sealed class CSharpProjectGenerationPostprocessor : AssetPostprocessor
    {
        public static string OnSelectingCSProjectStyle()
        {
#if ENABLE_SDK_STYLE_PROJECTS
            return "SDK";
#else
            return "Legacy";
#endif
        }
    }

    internal static class CSharpProjectGenerationMenu
    {
        private const string ENABLE_SDK_STYLE_PROJECTS = nameof(ENABLE_SDK_STYLE_PROJECTS);

        [MenuItem("Encosy Tower/Project Settings/CSharp/Switch to Legacy", priority = 80_67_00_01)]
        private static void SwitchToLegacy()
        {
            var buildTargets = BuildAPI.GetSupportedNamedBuildTargets();

            foreach (var buildTarget in buildTargets)
            {
                BuildAPI.RemoveScriptingDefineSymbols(buildTarget, ENABLE_SDK_STYLE_PROJECTS);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Encosy Tower/Project Settings/CSharp/Switch to SDK-style", priority = 80_67_00_00)]
        private static void SwitchToSdkStyle()
        {
            var buildTargets = BuildAPI.GetSupportedNamedBuildTargets();

            foreach (var buildTarget in buildTargets)
            {
                BuildAPI.RemoveScriptingDefineSymbols(buildTarget, ENABLE_SDK_STYLE_PROJECTS);
            }

            foreach (var buildTarget in buildTargets)
            {
                BuildAPI.AddScriptingDefineSymbols(buildTarget, ENABLE_SDK_STYLE_PROJECTS);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Encosy Tower/Project Settings/CSharp/Switch to Legacy", true)]
        private static bool CanDisableLegacy()
        {
#if ENABLE_SDK_STYLE_PROJECTS
            return true;
#else
            return false;
#endif
        }

        [MenuItem("Encosy Tower/Project Settings/CSharp/Switch to SDK-style", true)]
        private static bool CanDisableSdkStyle()
        {
#if ENABLE_SDK_STYLE_PROJECTS
            return false;
#else
            return true;
#endif
        }
    }
}

#endif
