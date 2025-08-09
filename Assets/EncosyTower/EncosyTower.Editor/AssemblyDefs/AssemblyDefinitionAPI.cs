#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditorInternal;

namespace EncosyTower.Editor.AssemblyDefs
{
    public static class AssemblyDefinitionAPI
    {
        private const string NO_ASMDEF_MSG = "No assembly definition found in the project.";

        public static Result<AssemblyDefinitionInfo> TryGetInfo([NotNull] AssemblyDefinitionImporter importer)
        {
            var assetPath = importer.assetPath;
            string json;

            try
            {
                json = File.ReadAllText(importer.assetPath);
            }
            catch (Exception ex)
            {
                return ex;
            }

            var tryResult = TryGetInfo(json);

            if (tryResult.TryGetValue(out var result))
            {
                return result;
            }

            if (tryResult.TryGetError(out var error))
            {
                return error;
            }

            return new Error($"Unexpected error while parsing {assetPath}");
        }

        public static Result<AssemblyDefinitionInfo> TryGetInfo(string json)
        {
            try
            {
                var assemblyDef = new AssemblyDefinitionInfo();

                EditorJsonUtility.FromJsonOverwrite(json, assemblyDef);

                return assemblyDef;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static Result<AssemblyData> TryGetData(
              [NotNull] string assetPath
            , [NotNull] AssemblyDefinitionInfo assemblyDef
        )
        {
            var assemblyAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath);

            return assemblyAsset.IsInvalid()
                ? new FileNotFoundException($"Assembly Definition asset not found at {assetPath}")
                : TryGetData(assetPath, assemblyAsset, assemblyDef);
        }

        public static Result<AssemblyData> TryGetData(
              [NotNull] string assetPath
            , [NotNull] AssemblyDefinitionAsset assemblyAsset
            , [NotNull] AssemblyDefinitionInfo assemblyDef
        )
        {
            var guidStrings = AssetDatabase
                .FindAssets($"t:{nameof(AssemblyDefinitionAsset)}")
                .AsSpan();

            var guidStringsLength = guidStrings.Length;

            if (guidStringsLength < 1)
            {
                return new FileNotFoundException(NO_ASMDEF_MSG);
            }

            var references = assemblyDef.references.AsSpan();
            var useGuid = UseGuid(references);

            var refGuids = new HashSet<GUID>(references.Length);
            var refNames = new HashSet<string>(references.Length);

            if (useGuid)
            {
                foreach (var reference in references)
                {
                    if (reference.IsNotEmpty()
                        && GUID.TryParse(reference.Replace("GUID:", ""), out var guid)
                    )
                    {
                        refGuids.Add(guid);
                    }
                }
            }
            else
            {
                foreach (var reference in references)
                {
                    if (string.IsNullOrWhiteSpace(reference) == false)
                    {
                        refNames.Add(reference);
                    }
                }
            }

            var guidToRefMap = new Dictionary<GUID, AssemblyReferenceData>(guidStringsLength);
            var nameToRefMap = new Dictionary<string, AssemblyReferenceData>(guidStringsLength);
            var otherReferences = new List<AssemblyReferenceData>(guidStringsLength);
            var allReferences = new List<AssemblyReferenceData>(guidStringsLength);
            var filteredReferences = new List<AssemblyReferenceData>(guidStringsLength);
            var suggestedSet = new HashSet<GUID>(guidStringsLength);

            for (var i = 0; i < guidStringsLength; i++)
            {
                var guidString = guidStrings[i];
                var path = AssetDatabase.GUIDToAssetPath(guidString);
                var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);

                if (asset.IsInvalid()
                    || GUID.TryParse(guidString, out var guid) == false
                    || TryGetInfo(asset.text).TryGetValue(out var info) == false
                )
                {
                    continue;
                }

                var data = new AssemblyReferenceData {
                    guid = guid,
                    asset = asset,
                    name = info.name,
                    guidString = guidString,
                    selected = useGuid ? refGuids.Contains(guid) : refNames.Contains(info.name),
                };

                guidToRefMap[guid] = data;
                nameToRefMap[info.name] = data;

                // If the reference is selected, add it to the `allReferences` list.
                // When we finally combine that list with `otherReferences`,
                // the selected references will be grouped at the top.

                if (data.selected)
                {
                    allReferences.Add(data);
                }
                else
                {
                    otherReferences.Add(data);
                }
            }

            foreach (var refData in allReferences)
            {
                var refInfoResult = TryGetInfo(refData.asset.text);

                if (refInfoResult.TryGetValue(out var refInfo) == false)
                {
                    continue;
                }

                var refInfoUseGuid = UseGuid(refInfo.references.AsSpan());

                if (refInfoUseGuid)
                {
                    foreach (var suggestedRef in refInfo.references)
                    {
                        if (string.IsNullOrEmpty(suggestedRef)
                            || GUID.TryParse(suggestedRef.Replace("GUID:", ""), out var guid) == false
                            || guidToRefMap.TryGetValue(guid, out var suggestedData) == false
                            || suggestedData.selected
                            || suggestedSet.Contains(suggestedData.guid)
                        )
                        {
                            continue;
                        }

                        filteredReferences.Add(suggestedData);
                        suggestedSet.Add(suggestedData.guid);
                    }
                }
                else
                {
                    foreach (var suggestedRef in refInfo.references)
                    {
                        if (string.IsNullOrWhiteSpace(suggestedRef)
                            || nameToRefMap.TryGetValue(suggestedRef, out var suggestedData) == false
                            || suggestedData.selected
                            || suggestedSet.Contains(suggestedData.guid)
                        )
                        {
                            continue;
                        }

                        filteredReferences.Add(suggestedData);
                        suggestedSet.Add(suggestedData.guid);
                    }
                }
            }

            allReferences.Sort(AssemblyReferenceData.CompareName);
            allReferences.Insert(0, new() { IsHeader = true, headerText = "Pre-selected References" });

            filteredReferences.Sort(AssemblyReferenceData.CompareName);
            filteredReferences.Insert(0, new() { IsHeader = true, headerText = "Other References" });
            filteredReferences.InsertRange(0, allReferences);

            allReferences.Add(new() { IsHeader = true, headerText = "Other References" });
            otherReferences.Sort(AssemblyReferenceData.CompareName);
            allReferences.AddRange(otherReferences);

            var result = new AssemblyData(
                  assetPath
                , assemblyAsset
                , assemblyDef
                , useGuid
                , allReferences
                , filteredReferences
            );

            return result;

            static bool UseGuid(Span<string> references)
            {
                return references.Length > 0
                    && references[0] is { } firstReference
                    && firstReference.StartsWith("GUID:", StringComparison.Ordinal);
            }
        }
    }
}

#endif
