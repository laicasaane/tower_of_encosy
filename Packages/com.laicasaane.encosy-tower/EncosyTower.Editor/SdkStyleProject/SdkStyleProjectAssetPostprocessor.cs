#if UNITY_SDK_STYLE_PROJECTS

using UnityEditor;

namespace EncosyTower.Editor.SdkStyleProject
{
    internal sealed class SdkStyleProjectAssetPostprocessor : AssetPostprocessor
    {
#if ENABLE_SDK_STYLE_PROJECTS
        public static string OnSelectingCSProjectStyle()
        {
            return "SDK"; // SDK or Legacy
        }
#endif

        [MenuItem("Encosy Tower/Project/Use Legacy style")]
        private static void UseLegacyStyleProjects()
        {
            UserBuildAPI.RemoveScriptingDefineSymbols(
                  UserBuildAPI.CurrentBuildTarget
                , "ENABLE_SDK_STYLE_PROJECTS"
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Encosy Tower/Project/Use SDK-style")]
        private static void UseSdkStyleProjects()
        {
            UserBuildAPI.AddScriptingDefineSymbols(
                  UserBuildAPI.CurrentBuildTarget
                , "ENABLE_SDK_STYLE_PROJECTS"
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Encosy Tower/Project/Use Legacy style", true)]
        private static bool CanDisableLegacyStyleProjects()
        {
#if ENABLE_SDK_STYLE_PROJECTS
            return true;
#else
            return false;
#endif
        }

        [MenuItem("Encosy Tower/Project/Use SDK-style", true)]
        private static bool CanDisableSdkStyleProjects()
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
