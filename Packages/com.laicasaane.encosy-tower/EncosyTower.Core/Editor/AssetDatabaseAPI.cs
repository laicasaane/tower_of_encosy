#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EncosyTower.Collections;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor
{
    public static class AssetDatabaseAPI
    {
        /// <summary>
        /// Find first object by global qualified type name (i.e. with `global::` prefix).
        /// </summary>
        public static bool FindFirstObjectByGlobalQualifiedType<T>(out T result)
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets($"t:global::{typeof(T).FullName}");

            if (candidates.Length > 0)
            {
                foreach (var candidate in candidates)
                {
                    var path = AssetDatabase.GUIDToAssetPath(candidate);
                    var asset = AssetDatabase.LoadAssetAtPath<T>(path);

                    if (asset.IsInvalid())
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

        /// <summary>
        /// Find first object by name and global qualified type name (i.e. with `global::` prefix).
        /// </summary>
        public static bool FindFirstObjectByNameAndGlobalQualifiedType<T>(
              string name
            , out T result
            , out string resultPath
        )
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets($"{name} t:global::{typeof(T).FullName}");

            if (candidates.Length > 0)
            {
                foreach (var candidate in candidates)
                {
                    var path = AssetDatabase.GUIDToAssetPath(candidate);
                    var asset = AssetDatabase.LoadAssetAtPath<T>(path);

                    if (asset.IsInvalid())
                    {
                        continue;
                    }

                    result = asset;
                    resultPath = path;
                    return true;
                }
            }

            result = default;
            resultPath = default;
            return false;
        }

        /// <summary>
        /// Find all objects by global qualified type name (i.e. with `global::` prefix).
        /// </summary>
        public static FasterList<T> FindAllObjectsByGlobalQualifiedType<T>()
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets($"t:global::{typeof(T).FullName}");
            var result = new FasterList<T>(candidates.Length);

            foreach (var candidate in candidates)
            {
                var path = AssetDatabase.GUIDToAssetPath(candidate);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset.IsInvalid())
                {
                    continue;
                }

                result.Add(asset);
            }

            return result;
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
