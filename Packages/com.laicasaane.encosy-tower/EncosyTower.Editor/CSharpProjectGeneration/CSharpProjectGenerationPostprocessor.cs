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
        [MenuItem("Encosy Tower/Project Settings/CSharp/Switch to Legacy", priority = 80_67_00_01)]
        private static void SwitchToLegacy()
        {
            UserBuildAPI.RemoveScriptingDefineSymbols(
                  UserBuildAPI.CurrentBuildTarget
                , "ENABLE_SDK_STYLE_PROJECTS"
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Encosy Tower/Project Settings/CSharp/Switch to SDK-style", priority = 80_67_00_00)]
        private static void SwitchToSdkStyle()
        {
            UserBuildAPI.RemoveScriptingDefineSymbols(
                  UserBuildAPI.CurrentBuildTarget
                , "ENABLE_SDK_STYLE_PROJECTS"
            );

            UserBuildAPI.AddScriptingDefineSymbols(
                  UserBuildAPI.CurrentBuildTarget
                , "ENABLE_SDK_STYLE_PROJECTS"
            );

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
