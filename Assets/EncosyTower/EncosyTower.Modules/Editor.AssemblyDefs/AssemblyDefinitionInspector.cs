#if UNITY_EDITOR

using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;

namespace EncosyTower.Modules.Editor.AssemblyDefs
{
    using IItemInfo = CheckBoxWindow.IItemInfo;

    [InitializeOnLoad]
    internal static class AssemblyDefinitionInspector
    {
        private const string TITLE = "Select References";
        private const string EMPTY_MSG = "No assembly definition found in the project.";

        static AssemblyDefinitionInspector()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI -= OnGUI;
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnGUI;
        }

        private static void OnGUI(UnityEditor.Editor editor)
        {
            if (editor.target is not AssemblyDefinitionImporter importer)
            {
                return;
            }

            if (GUILayout.Button(TITLE))
            {
                SelectReferences(importer);
            }
        }

        private static void SelectReferences(AssemblyDefinitionImporter importer)
        {
            var assetPath = importer.assetPath;
            var json = File.ReadAllText(assetPath);
            var assemblyDef = new AssemblyDef();

            try
            {
                EditorJsonUtility.FromJsonOverwrite(json, assemblyDef);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(TITLE, ex.ToString(), "I understand");
                return;
            }

            if (FindItems(assetPath, assemblyDef, out var data) == false)
            {

            }

            CheckBoxWindow.OpenWindow(TITLE, data.Items, data, OnApply, new(DrawItem, ItemHeight));
        }

        private static int ItemHeight()
            => 30;

        private static void DrawItem(Rect rect, GUIContent label, int index, IItemInfo item)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(rect.height);

                var newRect = rect;
                newRect.height = 18f;
                newRect.y += (rect.height - newRect.height) / 2f;

                {
                    var toggleRect = newRect;
                    toggleRect.x += 9f;
                    toggleRect.width = 18f;
                    item.IsChecked = EditorGUI.Toggle(toggleRect, item.IsChecked);
                }

                {
                    var assetRect = newRect;
                    assetRect.x += 30f;
                    assetRect.width -= 30f + 9f;

                    EditorGUI.ObjectField(assetRect, ((ItemInfo)item).asset, typeof(AssemblyDefinitionAsset), false);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void OnApply(IReadOnlyList<IItemInfo> _, object userData)
        {
            if (userData is not Data data)
            {
                return;
            }

            var assemblyDef = data.AssemblyDef;
            var items = data.Items;

            assemblyDef.references = items
                .Where(static x => x != null)
                .Where(static x => x.IsChecked)
                .Select(x => data.UseGuid ? x.guid : x.Name)
                .ToArray();

            var json = EditorJsonUtility.ToJson(assemblyDef, true);

            File.WriteAllText(data.AssetPath, json);
            AssetDatabase.Refresh();
        }

        private static bool FindItems(string assetPath, AssemblyDef assemblyDef, out Data result)
        {
            if (assemblyDef == null)
            {
                EditorUtility.DisplayDialog(TITLE, EMPTY_MSG, "I understand");
                result = default;
                return false;
            }

            var guidStrings = AssetDatabase.FindAssets($"t:{nameof(AssemblyDefinitionAsset)}").AsSpan();

            if (guidStrings.Length < 1)
            {
                EditorUtility.DisplayDialog(TITLE, EMPTY_MSG, "I understand");
                result = default;
                return false;
            }

            var references = assemblyDef.references.AsSpan();
            var referencesLength = references.Length;
            var useGuid = references.Length > 0
                && references[0] is string firstReference
                && firstReference.StartsWith("GUID:", StringComparison.Ordinal);

            var refGuids = new HashSet<GUID>(useGuid ? referencesLength : 0);
            var refNames = new HashSet<string>(useGuid == false ? referencesLength : 0);

            if (useGuid)
            {
                for (var i = 0; i < referencesLength; i++)
                {
                    var reference = references[i];

                    if (string.IsNullOrEmpty(reference))
                    {
                        continue;
                    }

                    if (GUID.TryParse(reference.Replace("GUID:", ""), out var guid))
                    {
                        refGuids.Add(guid);
                    }
                }
            }
            else
            {
                for (var i = 0; i < referencesLength; i++)
                {
                    var reference = references[i];

                    if (string.IsNullOrWhiteSpace(reference) == false)
                    {
                        refNames.Add(reference);
                    }
                }
            }

            var items = new List<ItemInfo>(guidStrings.Length);

            for (var i = 0; i < guidStrings.Length; i++)
            {
                var guidString = guidStrings[i];
                var path = AssetDatabase.GUIDToAssetPath(guidString);
                var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);

                if (asset == false)
                {
                    continue;
                }

                if (GUID.TryParse(guidString, out var guid) == false)
                {
                    continue;
                }

                items.Add(new ItemInfo {
                    asset = asset,
                    guid = guidString,
                    IsChecked = useGuid ? refGuids.Contains(guid) : refNames.Contains(asset.name),
                });
            }

            items.Sort(ItemInfo.Compare);

            result = new(assetPath, assemblyDef, useGuid, items.ToArray());
            return true;
        }

        private class ItemInfo : IItemInfo
        {
            public AssemblyDefinitionAsset asset;
            public string guid;

            public string Name => asset.name;

            public bool IsChecked { get; set; }

            public static int Compare(ItemInfo x, ItemInfo y)
                => string.CompareOrdinal(x.Name, y.Name);
        }

        private record class Data(
              string AssetPath
            , AssemblyDef AssemblyDef
            , bool UseGuid
            , ItemInfo[] Items
        );
    }
}

#endif
