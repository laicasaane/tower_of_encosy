#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor
{
    public static class AssetDatabaseAPI
    {
        /// <summary>
        /// Find object by full type name with `global::` prefix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool FindObjectByGlobalPrefixType<T>(out T result)
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets($"t:global::{typeof(T).FullName}");

            if (candidates.Length > 0)
            {
                foreach (var candidate in candidates)
                {
                    var path = AssetDatabase.GUIDToAssetPath(candidate);
                    var asset = AssetDatabase.LoadAssetAtPath<T>(path);

                    if (!asset)
                    {
                        continue;
                    }

                    result = asset;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public static bool TryCreateScriptableObject<T>(out T result)
            where T : UnityEngine.ScriptableObject
        {
            var type = typeof(T);
            var name = ObjectNames.NicifyVariableName(type.Name);

            if (type.IsAbstract)
            {
                EditorUtility.DisplayDialog(
                      $"Creating {name}"
                    , $"Cannot create an instance of abstract type '{name}'"
                    , "I understand"
                );

                result = default;
                return false;
            }

            var choose = EditorUtility.DisplayDialog(
                  $"Creating {name}"
                , $"Cannot find any instance of '{name}' in this project"
                , "Create"
                , "I understand"
            );

            if (choose == false)
            {
                result = default;
                return false;
            }

            var path = EditorUtility.OpenFolderPanel(
                  "Select a folder"
                , "Assets"
                , ""
            );

            if (string.IsNullOrWhiteSpace(path))
            {
                result = default;
                return false;
            }

            var relativePath = Path.GetRelativePath(Application.dataPath, path);
            relativePath = Path.Combine("Assets", relativePath, $"{type.Name}.asset");

            result = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(result, relativePath);
            return true;
        }

        public static bool TryCreateScriptableObject([NotNull] Type type, out ScriptableObject result)
        {
            var name = ObjectNames.NicifyVariableName(type.Name);

            if (type.IsAbstract)
            {
                EditorUtility.DisplayDialog(
                      $"Creating {name}"
                    , $"Cannot create an instance of abstract type '{name}'"
                    , "I understand"
                );

                result = default;
                return false;
            }

            var choose = EditorUtility.DisplayDialog(
                  $"Creating {name}"
                , $"Cannot find any instance of '{name}' in this project"
                , "Create"
                , "I understand"
            );

            if (choose == false)
            {
                result = default;
                return false;
            }

            var path = EditorUtility.OpenFolderPanel(
                  "Select a folder"
                , "Assets"
                , ""
            );

            if (string.IsNullOrWhiteSpace(path))
            {
                result = default;
                return false;
            }

            var relativePath = Path.GetRelativePath(Application.dataPath, path);
            relativePath = Path.Combine("Assets", relativePath, $"{type.Name}.asset");

            result = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(result, relativePath);
            return true;
        }
    }
}

#endif
