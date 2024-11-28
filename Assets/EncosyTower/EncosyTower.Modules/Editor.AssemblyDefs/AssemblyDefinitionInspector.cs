#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EncosyTower.Modules.Editor.AssemblyDefs
{
    using IItemInfo = CheckBoxWindow.IItemInfo;

    [InitializeOnLoad]
    internal static class AssemblyDefinitionInspector
    {
        private const string SELECT_REFS_LABEL = "Select References";
        private const string NO_ASMDEF_MSG = "No assembly definition found in the project.";

        private static GUIStyle s_headerStyle;
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

            s_headerStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
            };

            var label = new GUIContent();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField(label.WithText("References"), s_headerStyle);

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(label.WithText("Select"), EditorStyles.miniButtonLeft))
                    {
                        SelectReferences(importer);
                    }

                    if (GUILayout.Button(label.WithText("Sort"), EditorStyles.miniButtonRight))
                    {
                        SortReferences(importer);
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3f);
            }
            EditorGUILayout.EndVertical();
        }

        private static bool TryGetAssemblyDef(AssemblyDefinitionImporter importer, out AssemblyDef result)
        {
            var json = File.ReadAllText(importer.assetPath);
            var assemblyDef = new AssemblyDef();

            try
            {
                EditorJsonUtility.FromJsonOverwrite(json, assemblyDef);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(importer.name, ex.ToString(), "I understand");
                result = null;
                return false;
            }

            result = assemblyDef;
            return true;
        }

        private static void SortReferences(AssemblyDefinitionImporter importer)
        {
            if (TryGetAssemblyDef(importer, out var assemblyDef) == false)
            {
                return;
            }

            var span = assemblyDef.references.AsSpan();
            var length = span.Length;
            var useGuid = span.Length > 0
                && span[0] is { } firstReference
                && firstReference.StartsWith("GUID:", StringComparison.Ordinal);

            var pairs = new List<(string name, string guid)>(useGuid == false ? length : 0);

            if (useGuid)
            {
                for (var i = 0; i < length; i++)
                {
                    var reference = span[i];

                    if (string.IsNullOrEmpty(reference))
                    {
                        continue;
                    }

                    var guid = reference.Replace("GUID:", "");
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);

                    if (asset == false)
                    {
                        continue;
                    }

                    pairs.Add((asset.name, guid));
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    var reference = span[i];

                    if (string.IsNullOrWhiteSpace(reference) == false)
                    {
                        pairs.Add((reference, reference));
                    }
                }
            }

            pairs.Sort(Compare);

            assemblyDef.references = pairs
                .Select(x => useGuid ? x.guid : x.name)
                .ToArray();

            var json = EditorJsonUtility.ToJson(assemblyDef, true);

            File.WriteAllText(importer.assetPath, json);
            AssetDatabase.Refresh();
            return;

            static int Compare((string name, string guid) x, (string name, string guid) y)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name);
            }
        }

        private static void SelectReferences(AssemblyDefinitionImporter importer)
        {
            if (TryGetAssemblyDef(importer, out var assemblyDef) == false)
            {
                return;
            }

            if (FindItems(importer.assetPath, assemblyDef, out var data) == false)
            {
                return;
            }

            CheckBoxWindow.OpenWindow(SELECT_REFS_LABEL, data.Items, data, OnApply, new(DrawItem, ItemHeight, DrawSeparator));
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
            var items = data.Items.ToList();

            items.Sort(Compare);

            assemblyDef.references = items
                .Where(static x => x != null)
                .Where(static x => x.Separator == false)
                .Where(static x => x.IsChecked)
                .Select(x => data.UseGuid ? x.guid : x.Name)
                .ToArray();

            var json = EditorJsonUtility.ToJson(assemblyDef, true);

            File.WriteAllText(data.AssetPath, json);
            AssetDatabase.Refresh();
            return;

            static int Compare(ItemInfo x, ItemInfo y)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
            }
        }

        private static bool FindItems(string assetPath, AssemblyDef assemblyDef, out Data result)
        {
            if (assemblyDef == null)
            {
                EditorUtility.DisplayDialog(SELECT_REFS_LABEL, NO_ASMDEF_MSG, "I understand");
                result = default;
                return false;
            }

            var guidStrings = AssetDatabase
                .FindAssets($"t:{nameof(AssemblyDefinitionAsset)}")
                .AsSpan();

            var guidStringsLength = guidStrings.Length;

            if (guidStringsLength < 1)
            {
                EditorUtility.DisplayDialog(SELECT_REFS_LABEL, NO_ASMDEF_MSG, "I understand");
                result = default;
                return false;
            }

            var references = assemblyDef.references.AsSpan();
            var referencesLength = references.Length;
            var useGuid = references.Length > 0
                && references[0] is { } firstReference
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

            public bool Separator { get; init; }

            public static int CompareName(ItemInfo x, ItemInfo y)
                => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
        }

        private record Data(
              string AssetPath
            , AssemblyDef AssemblyDef
            , bool UseGuid
            , ItemInfo[] Items
        );
    }
}

#endif
