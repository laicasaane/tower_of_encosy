using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Collections.LowLevel.ILSupport.CodeGen
{
    internal static class EncosyTowerMenu
    {
        [MenuItem("Encosy Tower/Go To Assembly")]
        public static void GoToAssembly()
        {
            var path = Path.Combine(
                  Application.dataPath
                , $"../Library/ScriptAssemblies/{Constants.ASSEMBLY_NAME}.dll"
            );

            if (File.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Encosy Tower/Go To Encosy Package Plugins")]
        public static void GoToEncosyPackagePlugins()
        {
            var path = Path.Combine(
                  Application.dataPath
                , $"../../../Packages/com.laicasaane.encosy-tower/Plugins/{Constants.ASSEMBLY_NAME}"
            );

            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Encosy Tower/Go To Bcl.RuntimeUnsafe")]
        public static void GoToBclRuntimeUnsafe()
        {
            var path = Path.Combine(
                  Application.dataPath
                , $"../../../Plugins/Bcl.Extensions/Bcl.RuntimeUnsafe"
            );

            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
        }
    }
}
