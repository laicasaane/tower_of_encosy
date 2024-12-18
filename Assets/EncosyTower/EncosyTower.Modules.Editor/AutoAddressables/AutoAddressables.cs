// MIT License
//
// Copyright (c) 2020 Per Holmes
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

// https://github.com/perholmes/UnityAutoBundles

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EncosyTower.Modules.Logging;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Build.AnalyzeRules
{
    internal class AutoAddressables : AnalyzeRule
    {
        /// <summary>
        /// Group settings template to use for newly created groups.
        /// </summary>
        private const string TEMPLATE = "Packed Assets";

        /// <summary>
        /// Created groups will have this prefix. Do not change after starting to use it.
        /// </summary>
        private const string AUTO_GROUP_PREFIX = "auto-";

        /// <summary>
        /// Name of the folder that will be scanned.
        /// </summary>
        private const string AUTO_FOLDER_NAME = "AutoAddressables";

        private const string AUTO_DIRECTORY = $"Assets/{AUTO_FOLDER_NAME}";

        private const string AUTO_DIRECTORY_PATH = $"{AUTO_DIRECTORY}/";

        private const string ASSET_FILTER = "t:AnimationClip " +
            "t:AudioClip " +
            "t:AudioMixer " +
            "t:ComputeShader " +
            "t:Font " +
            "t:FontAsset " +
            "t:GUISkin " +
            "t:Material " +
            "t:Mesh " +
            "t:Model " +
            "t:PhysicMaterial " +
            "t:Prefab " +
            "t:Scene " +
            "t:Shader " +
            "t:Sprite " +
            "t:SpriteAtlas " +
            "t:ScriptableObject " +
            "t:Texture " +
            "t:VideoClip";

        /// <summary>
        /// Assets with this label are always included no matter what.
        /// </summary>
        private const string FORCE_LABEL = "ForceAddressable";

        private static readonly string[] s_ignoreExtensions = { ".fbx", ".psd" };
        private static readonly string[] s_alwaysIncludeExtensions = { ".unity" };

        private readonly List<string> _groupsToCreate = new();
        private readonly List<string> _groupsToRemove = new();
        private readonly List<AssetAction> _assetActions = new();

        public override bool CanFix => true;

        public override string ruleName => nameof(AutoAddressables);

        private void ClearOurData()
        {
            _groupsToCreate.Clear();
            _groupsToRemove.Clear();
            _assetActions.Clear();
        }

        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            ClearAnalysis();
            ClearOurData();

            var results = new List<AnalyzeResult>();
            var projectRoot = Application.dataPath[..^"Assets/".Length];

            if (BuildUtility.CheckModifiedScenesAndAskToSave() == false)
            {
                DevLoggerAPI.LogError("Cannot run Analyze with unsaved scenes.");

                results.Add(new AnalyzeResult {
                    resultName = $"{ruleName}: Cannot run Analyze with unsaved scenes."
                });

                return results;
            }

            // Get all immediate folders in AUTO_DIRECTORY
            var folders = AssetDatabase.GetSubFolders(AUTO_DIRECTORY);
            var folderNames = new HashSet<string>();

            foreach (var folder in folders)
            {
                folderNames.Add(Path.GetFileName(folder));
            }

            // Get all addressable groups carrying the (Auto) prefix
            var autoGroups = new HashSet<string>();

            foreach (var group in settings.groups)
            {
                if (group.name.StartsWith(AUTO_GROUP_PREFIX))
                {
                    autoGroups.Add(group.name);
                }
            }

            // Collect all groups that must be created or moved
            foreach (var folder in folderNames)
            {
                var autoName = $"{AUTO_GROUP_PREFIX}{folder}";

                if (autoGroups.Contains(autoName) == false)
                {
                    _groupsToCreate.Add(autoName);
                    results.Add(new AnalyzeResult() {
                        resultName = $"Create group \"{autoName}\""
                    });
                }
            }

            // Collect all groups that must be removed
            foreach (var groupName in autoGroups)
            {
                var baseName = groupName[AUTO_GROUP_PREFIX.Length..];

                if (folderNames.Contains(baseName) == false)
                {
                    _groupsToRemove.Add(groupName);
                    results.Add(new AnalyzeResult() {
                        resultName = $"Remove group \"{groupName}\""
                    });
                }
            }

            // Get all assets
            var allGuids = AssetDatabase.FindAssets(ASSET_FILTER, new [] { AUTO_DIRECTORY });
            var neverBundle = new HashSet<string>();

            // Only include assets that pass basic filtering, like file extension.
            // Result is "assetPaths", the authoritative list of assets we're considering bundling.
            var assetPaths = new HashSet<string>();

            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (ShouldIgnoreAsset(path))
                {
                    neverBundle.Add(path.ToLower());
                }
                else
                {
                    assetPaths.Add(path);
                }
            }

            // Collect all parents of all assets in preparation for not bundling assets with one ultimate non-scene parent.
            // Map from asset guid to all its parents
            var parents = new Dictionary<string, HashSet<string>>();

            foreach (var path in assetPaths)
            {
                var dependencies = AssetDatabase.GetDependencies(path);

                foreach (var asset in dependencies)
                {
                    if (asset == path)
                    {
                        // Ignore self
                        continue;
                    }

                    if (ShouldIgnoreAsset(asset))
                    {
                        continue;
                    }

                    if (parents.ContainsKey(asset) == false)
                    {
                        parents.Add(asset, new HashSet<string>());
                    }

                    parents[asset].Add(path);
                }
            }

            // Unbundle assets with zero parents
            foreach (var asset in assetPaths)
            {
                if (parents.ContainsKey(asset) == false)
                {
                    neverBundle.Add(asset.ToLower());
                }
            }

            if (neverBundle.Count > 0)
            {
                DevLoggerAPI.LogInfo($"{neverBundle.Count} asset(s) have zero parents and will not be bundled.");
            }

            var floor = neverBundle.Count;

            // Unbundle assets with one parent
            foreach (var asset in assetPaths)
            {
                if (parents.ContainsKey(asset) && parents[asset].Count == 1)
                {
                    neverBundle.Add(asset.ToLower());
                }
            }

            if (neverBundle.Count - floor > 0)
            {
                DevLoggerAPI.LogInfo($"{neverBundle.Count - floor} asset(s) have one parent and will not be bundled.");
            }

            floor = neverBundle.Count;

            // Unbundle assets with one ultimate parent
            var ultimateParents = new Dictionary<string, HashSet<string>>();

            foreach (var asset in assetPaths)
            {
                if (neverBundle.Contains(asset.ToLower()))
                {
                    continue;
                }

                ultimateParents[asset] = new HashSet<string>();

                // Iterate all the way to the top for this asset.
                // Assemble a list of all ultimate parents of this asset.

                var parentsToCheck = new List<string>();
                parentsToCheck.AddRange(parents[asset].ToList());

                while (parentsToCheck.Count != 0)
                {
                    var checking = parentsToCheck[0];
                    parentsToCheck.RemoveAt(0);

                    if (parents.ContainsKey(checking) == false)
                    {
                        // If asset we're checking doesn't itself have any parents, this is the end.
                        ultimateParents[asset].Add(checking);
                    }
                    else
                    {
                        parentsToCheck.AddRange(parents[checking]);
                    }
                }
            }

            // Unbundle all assets that don't have two or more required objects as ultimate parents.
            // Objects with one included parent will still get included if needed, just not as a separate Addressable.
            foreach (var pair in ultimateParents)
            {
                var requiredParents = 0;

                foreach (var ultiParent in pair.Value)
                {
                    if (AlwaysIncludeAsset(ultiParent))
                    {
                        requiredParents++;
                    }
                }

                if (requiredParents <= 1)
                {
                    neverBundle.Add(pair.Key.ToLower());
                }
            }

            if (neverBundle.Count - floor > 0)
            {
                DevLoggerAPI.LogInfo(
                    $"{neverBundle.Count - floor} asset(s) have zero or one ultimate parents and will not be bundled."
                );
            }

            floor = neverBundle.Count;

            // Skip assets that are too small. This is a tradeoff between individual access to files,
            // versus the game not having an open file handle for every single 2 KB thing. We're choosing
            // to duplicate some things by baking them into multiple bundles, even though it requires
            // more storage and bandwidth.
            var tooSmallCount = 0;

            foreach (var asset in assetPaths)
            {
                if (neverBundle.Contains(asset.ToLower()))
                {
                    continue;
                }

                var diskPath = $"{projectRoot}/{asset}";
                var fileInfo = new System.IO.FileInfo(diskPath);

                if (fileInfo.Length < 10000)
                {
                    tooSmallCount++;
                    neverBundle.Add(asset.ToLower());
                }
            }

            if (tooSmallCount > 0)
            {
                DevLoggerAPI.LogInfo($"{tooSmallCount} asset(s) are too small and will not be bundled.");
            }

            // Collect all assets to create as addressables
            var expresslyBundled = new HashSet<string>();

            foreach (var folder in folderNames)
            {
                var assetGuids = AssetDatabase.FindAssets(ASSET_FILTER, new [] { $"{AUTO_DIRECTORY}/{folder}" });

                // Schedule creation/moving of assets that exist
                foreach (var guid in assetGuids)
                {
                    var addrPath = AssetDatabase.GUIDToAssetPath(guid);

                    // Skip assets we're never bundling
                    var lowerPath = addrPath.ToLower();

                    if (neverBundle.Contains(lowerPath) && AlwaysIncludeAsset(lowerPath) == false)
                    {
                        continue;
                    }

                    // Remove the Assets/AutoBundles/ part of assets paths.
                    var shortPath = addrPath;

                    if (shortPath.StartsWith(AUTO_DIRECTORY_PATH))
                    {
                        shortPath = shortPath[AUTO_DIRECTORY_PATH.Length..];
                    }

                    // Create asset creation/moving action.
                    var autoGroup = AUTO_GROUP_PREFIX + folder;

                    _assetActions.Add(new AssetAction() {
                        create = true,
                        inGroup = autoGroup,
                        assetGuid = guid,
                        addressablePath = shortPath
                    });

                    var entry = settings.FindAssetEntry(guid);

                    if (entry == null)
                    {
                        results.Add(new AnalyzeResult() {
                            resultName = $"Add: {shortPath}"
                        });
                    }
                    else
                    {
                        results.Add(new AnalyzeResult() {
                            resultName = $"Keep or move: {shortPath}"
                        });
                    }

                    expresslyBundled.Add(shortPath);
                }
            }

            // Schedule removal of assets in auto folders that exist as addressables but aren't expressly bundled.
            foreach (var folder in folderNames)
            {
                var autoName = $"{AUTO_GROUP_PREFIX}{folder}";
                var group = settings.FindGroup(autoName);

                if (group == null)
                {
                    continue;
                }

                var result = new List<AddressableAssetEntry>();
                group.GatherAllAssets(result, true, false, true);

                foreach (var entry in result)
                {
                    if (entry.IsSubAsset)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.guid))
                    {
                        DevLoggerAPI.LogWarning($"Entry \"{entry.address}\" has no GUID!");
                    }

                    if (expresslyBundled.Contains(entry.address) == false)
                    {
                        _assetActions.Add(new AssetAction() {
                            create = false,
                            inGroup = autoName,
                            assetGuid = entry.guid,
                            addressablePath = entry.address,
                        });

                        // Print removal message without preamble
                        results.Add(new AnalyzeResult() {
                            resultName = $"Remove: {entry.address}"
                        });
                    }
                }
            }

            return results;
        }

        public override void FixIssues(AddressableAssetSettings settings)
        {
            // Load template used for creating new groups
            var groupTemplates = settings.GroupTemplateObjects;
            AddressableAssetGroupTemplate foundTemplate = null;

            foreach (var template in groupTemplates)
            {
                if (template.name == TEMPLATE)
                {
                    foundTemplate = template as AddressableAssetGroupTemplate;
                    break;
                }
            }

            if (foundTemplate == null)
            {
                DevLoggerAPI.LogError($"Group template \"{TEMPLATE}\" not found. Aborting!");
                return;
            }

            // Create groups
            foreach (var groupName in _groupsToCreate)
            {
                // TODO: I don't know enough about schemas, so schemasToCopy is set to null here.
                var newGroup = settings.CreateGroup(groupName, false, false, true, null, foundTemplate.GetTypes());
                foundTemplate.ApplyToAddressableAssetGroup(newGroup);
            }

            // Remove groups
            foreach (var groupName in _groupsToRemove)
            {
                foreach (var group in settings.groups)
                {
                    if (group.name == groupName)
                    {
                        settings.RemoveGroup(group);
                        break;
                    }
                }
            }

            // Collect current group names
            var groups = new Dictionary<string, AddressableAssetGroup>();

            foreach (var group in settings.groups)
            {
                groups.Add(group.name, group);
            }

            // Create and remove assets
            foreach (var action in _assetActions)
            {
                if (groups.ContainsKey(action.inGroup) == false)
                {
                    continue;
                }

                if (action.create)
                {
                    var entry = settings.CreateOrMoveEntry(action.assetGuid, groups[action.inGroup]);
                    entry.SetAddress(action.addressablePath);
                }
                else
                {
                    var entry = settings.FindAssetEntry(action.assetGuid);
                    if (entry != null)
                    {
                        settings.RemoveAssetEntry(action.assetGuid);
                    }
                    else
                    {
                        DevLoggerAPI.LogWarning($"Asset GUID did not produce an entry: {action.assetGuid}");
                    }
                }
            }

            ClearAnalysis();
            ClearOurData();
        }

        private static bool ShouldIgnoreAsset(string path)
        {
            var lowerPath = path.ToLower();

            foreach (var ext in s_alwaysIncludeExtensions)
            {
                if (lowerPath.EndsWith(ext))
                {
                    return false;
                }
            }

            foreach (var ext in s_ignoreExtensions)
            {
                if (lowerPath.EndsWith(ext))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool AlwaysIncludeAsset(string path)
        {
            var lowerPath = path.ToLower();

            foreach (var ext in s_alwaysIncludeExtensions)
            {
                if (lowerPath.EndsWith(ext))
                {
                    return true;
                }
            }

            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            var labels = AssetDatabase.GetLabels(asset);

            if (labels.Contains(FORCE_LABEL))
            {
                return true;
            }

            return false;
        }

        private struct AssetAction
        {
            public bool create;     // True = create, false = remove addressable asset
            public string inGroup;  // Group name with (Auto) prefix.
            public string addressablePath;
            public string assetGuid;
        }

        [InitializeOnLoad]
        private static class RegisterBuildBundleLayout
        {
            static RegisterBuildBundleLayout()
            {
                AnalyzeSystem.RegisterNewRule<AutoAddressables>();
            }
        }
    }
}
