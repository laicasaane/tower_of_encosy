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

        private static GUIStyle s_separatorStyle;

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

            CheckBoxWindow.OpenWindow(TITLE, data.Items, data, OnApply, new(DrawItem, ItemHeight, DrawSeparator));
        }

        private static int ItemHeight()
            => 22;

        private static void DrawItem(Rect rect, GUIContent label, int index, IItemInfo item)
        {
            GUILayout.Space(2f);
            EditorGUILayout.BeginHorizontal();
            {
                item.IsChecked = EditorGUILayout.ToggleLeft(label, item.IsChecked);
                EditorGUILayout.ObjectField(((ItemInfo)item).asset, typeof(AssemblyDefinitionAsset), false);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawSeparator(Rect rect, GUIContent label, int index, IItemInfo item)
        {
            s_separatorStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter
            };

            label.text = ((ItemInfo)item).separatorText;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, s_separatorStyle);
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
                .Where(static x => x.Separator == false)
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

            var guidStrings = AssetDatabase
                .FindAssets($"t:{nameof(AssemblyDefinitionAsset)}")
                .AsSpan();

            var guidStringsLength = guidStrings.Length;

            if (guidStringsLength < 1)
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

            var items = new List<ItemInfo>(guidStringsLength);
            var checkedItems = new List<ItemInfo>(guidStringsLength);

            for (var i = 0; i < guidStringsLength; i++)
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

                var item = new ItemInfo {
                    asset = asset,
                    guid = guidString,
                    IsChecked = useGuid ? refGuids.Contains(guid) : refNames.Contains(asset.name),
                };

                if (item.IsChecked)
                {
                    checkedItems.Add(item);
                }
                else
                {
                    items.Add(item);
                }
            }

            items.Sort(ItemInfo.CompareName);
            checkedItems.Sort(ItemInfo.CompareName);
            checkedItems.Insert(0, new() { Separator = true, separatorText = "Pre-selected References" });
            checkedItems.Add(new() { Separator = true, separatorText = "Other References" });
            checkedItems.AddRange(items);

            result = new(assetPath, assemblyDef, useGuid, checkedItems.ToArray());
            return true;
        }

        private class ItemInfo : IItemInfo
        {
            public AssemblyDefinitionAsset asset;
            public string guid;
            public string separatorText;

            public string Name => asset ? asset.name : string.Empty;

            public bool IsChecked { get; set; }

            public bool Separator { get; set; }

            public static int CompareName(ItemInfo x, ItemInfo y)
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
