#if UNITY_EDITOR

// MIT License
//
// Copyright (c) 2024 Mika Notarnicola
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// https://github.com/thebeardphantom/Runtime-TypeCache

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EncosyTower.Modules.Types.Internals;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EncosyTower.Modules.Types.Editor
{
    /// <summary>
    /// Responsible for creating the <see cref="SerializedTypeCacheAsset"/> at build time.
    /// </summary>
    internal sealed class SerializedTypeCacheBuilder : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>
        /// The name of which the debug copies of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_NAME = nameof(SerializedTypeCacheAsset);

        /// <summary>
        /// The path at which the transitory <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_PATH = $"Assets/Resources/{ASSET_FILE_NAME}.asset";

        /// <summary>
        /// The name of which the debug copies of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_NAME_DEBUG = $"{ASSET_FILE_NAME}_Debug";

        /// <summary>
        /// The path at which the debug copy of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_PATH_DEBUG = $"Temp/{ASSET_FILE_NAME_DEBUG}.asset";

        /// <summary>
        /// The path at which the debug json copy of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_JSON_FILE_PATH_DEBUG = $"Temp/{ASSET_FILE_NAME_DEBUG}.json";

        /// <inheritdoc />
        int IOrderedCallback.callbackOrder { get; }

        /// <summary>
        /// Cleans up after the <see cref="IPreprocessBuildWithReport.OnPreprocessBuild" /> function.
        /// </summary>
        private static void DeleteAndRemoveAsset()
        {
            AssetDatabase.DeleteAsset(ASSET_FILE_PATH);

            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(static obj => obj == false || obj is SerializedTypeCacheAsset);

            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
        }

        /// <inheritdoc />
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            try
            {
                // Generate type cache asset
                var asset = ScriptableObject.CreateInstance<SerializedTypeCacheAsset>();
                asset._cache.Regenerate();

                // Serialize asset to temp folder for debug inspection
                asset.name = ASSET_FILE_NAME_DEBUG;

                InternalEditorUtility.SaveToSerializedFileAndForget(
                      new Object[] { asset }
                    , ASSET_FILE_NAME_DEBUG
                    , true
                );

                // Serialize to json for debug inspection
                var json = JsonUtility.ToJson(asset, true);
                json = Regex.Replace(json, "(<|>|k__BackingField)", "");

                File.WriteAllText(ASSET_JSON_FILE_PATH_DEBUG, json);

                // Create asset so preloaded assets can actually use it
                asset.name = ASSET_FILE_NAME;
                AssetDatabase.CreateAsset(asset, ASSET_FILE_PATH);

                var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();

                preloadedAssets.Add(asset);
                preloadedAssets.RemoveAll(static obj => obj == false);

                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
            catch (Exception)
            {
                DeleteAndRemoveAsset();
                throw;
            }
        }

        /// <inheritdoc />
        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
            DeleteAndRemoveAsset();
        }
    }
}

#endif
