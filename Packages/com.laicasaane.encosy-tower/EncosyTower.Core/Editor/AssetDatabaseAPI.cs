#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.IO;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor
{
    public static class AssetDatabaseAPI
    {
        /// <summary>
        /// Find all objects by a filter.
        /// </summary>
        public static FasterList<T> FindAllObjects<T>(string filter)
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets(filter);
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

        /// <summary>
        /// Find all objects by a filter.
        /// </summary>
        public static void FindAllObjects<T>(string filter, ICollection<T> result)
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets(filter);
            result.TryIncreaseCapacityToFast(candidates.Length);

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
        }

        /// <summary>
        /// Find first object by a filter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> FindFirstObject<T>(string filter)
            where T : UnityEngine.Object
            => FindFirstObject<T>(filter, out var obj) ? obj : Option.None;

        /// <summary>
        /// Find first object by a filter.
        /// </summary>
        public static bool FindFirstObject<T>(string filter, out T result)
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets(filter);

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
        /// Find first object by global qualified type name (i.e. with `global::` prefix).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> FindFirstObjectByGlobalQualifiedTypeName<T>()
            where T : UnityEngine.Object
            => FindFirstObjectByGlobalQualifiedTypeName<T>(out var obj) ? obj : Option.None;

        /// <summary>
        /// Find first object by global qualified type name (i.e. with `global::` prefix).
        /// </summary>
        public static bool FindFirstObjectByGlobalQualifiedTypeName<T>(out T result)
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
        public static bool FindFirstObjectByNameAndGlobalQualifiedTypeName<T>(
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
        public static FasterList<T> FindAllObjectsByGlobalQualifiedTypeName<T>()
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

        /// <summary>
        /// Find all objects by global qualified type name (i.e. with `global::` prefix).
        /// </summary>
        public static void FindAllObjectsByGlobalQualifiedTypeName<T>(ICollection<T> result)
            where T : UnityEngine.Object
        {
            var candidates = AssetDatabase.FindAssets($"t:global::{typeof(T).FullName}");
            result.TryIncreaseCapacityToFast(candidates.Length);

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
        }

        public static bool CreateThenSaveScriptableObjectToFile<T>(
              string fileName
            , string relativeFolderPath
            , out T result
            , Action<T> onCreate = null
            , bool overwriteIfExist = false
            , bool displayErrorDialog = true
            , bool refreshAssetDatabase = true
            , Logging.ILogger logger = null
        )
            where T : UnityEngine.ScriptableObject
        {
            var type = typeof(T);

            if (type.IsAbstract)
            {
                logger?.LogError($"Cannot create an instance of the abstract type '{type}'");

                if (displayErrorDialog)
                {
                    EditorUtility.DisplayDialog(
                          $"Create Then Save ScriptableObject To File"
                        , $"Cannot create an instance of the abstract type '{type}'"
                        , "I understand"
                    );
                }
                result = default;
                return false;
            }

            var filePath = Path.Combine(relativeFolderPath, $"{fileName}.asset");

            RootPath rootPath = EditorAPI.ProjectPath;
            var absoluteFilePath = rootPath.GetFileAbsolutePath(filePath);

            if (File.Exists(absoluteFilePath))
            {
                if (overwriteIfExist == false)
                {
                    logger?.LogError(
                        $"A file of the same name '{fileName}' already exists at '{relativeFolderPath}'.\n" +
                        $"Please choose a new name."
                    );

                    if (displayErrorDialog)
                    {
                        EditorUtility.DisplayDialog(
                              $"Create Then Save ScriptableObject To File"
                            , $"A file of the same name '{fileName}' already exists at '{relativeFolderPath}'.\n" +
                              $"Please choose a new name."
                            , "I understand"
                        );
                    }

                    result = null;
                    return false;
                }

                File.Delete(absoluteFilePath);
            }

            try
            {
                result = ScriptableObject.CreateInstance<T>();
                onCreate?.Invoke(result);

                AssetDatabase.CreateAsset(result, filePath);

                if (refreshAssetDatabase)
                {
                    AssetDatabase.Refresh();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogException(ex);

                if (displayErrorDialog)
                {
                    EditorUtility.DisplayDialog(
                          $"Create Then Save ScriptableObject To File"
                        , ex.ToString()
                        , "I understand"
                    );
                }

                result = null;
                return false;
            }
        }

        public static bool SaveScriptableObjectToFile<T>(
              T asset
            , string fileName
            , string relativeFolderPath
            , bool overwriteIfExist = false
            , bool displayErrorDialog = true
            , bool refreshAssetDatabase = true
            , Logging.ILogger logger = null
        )
            where T : UnityEngine.ScriptableObject
        {
            if (asset.IsInvalid())
            {
                logger?.LogError("Asset must not be null.");

                if (displayErrorDialog)
                {
                    EditorUtility.DisplayDialog(
                          $"Save ScriptableObject To File"
                        , $"Asset must not be null."
                        , "I understand"
                    );
                }

                return false;
            }

            var filePath = Path.Combine(relativeFolderPath, $"{fileName}.asset");

            RootPath rootPath = EditorAPI.ProjectPath;
            var absoluteFilePath = rootPath.GetFileAbsolutePath(filePath);

            if (File.Exists(absoluteFilePath))
            {
                if (overwriteIfExist == false)
                {
                    logger?.LogError(
                        $"A file of the same name '{fileName}' already exists at '{relativeFolderPath}'.\n" +
                        $"Please choose a new name."
                    );

                    if (displayErrorDialog)
                    {
                        EditorUtility.DisplayDialog(
                              $"Save ScriptableObject To File"
                            , $"A file of the same name '{fileName}' already exists at '{relativeFolderPath}'.\n" +
                              $"Please choose a new name."
                            , "I understand"
                        );
                    }

                    return false;
                }

                File.Delete(absoluteFilePath);
            }

            try
            {
                AssetDatabase.CreateAsset(asset, filePath);

                if (refreshAssetDatabase)
                {
                    AssetDatabase.Refresh();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogException(ex);

                if (displayErrorDialog)
                {
                    EditorUtility.DisplayDialog(
                          $"Save ScriptableObject To File"
                        , ex.ToString()
                        , "I understand"
                    );
                }

                return false;
            }
        }
    }
}

#endif
