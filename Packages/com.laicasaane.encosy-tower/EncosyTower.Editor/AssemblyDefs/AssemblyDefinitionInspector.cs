#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EncosyTower.Editor.AssemblyDefs
{
    [InitializeOnLoad]
    internal static class AssemblyDefinitionInspector
    {
        private static GUIStyle s_headerStyle;

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
                    if (GUILayout.Button(label.WithText("Edit"), EditorStyles.miniButtonLeft))
                    {
                        EditReferences(importer);
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

        private static void SortReferences(AssemblyDefinitionImporter importer)
        {
            var infoResult = AssemblyDefinitionAPI.TryGetInfo(importer);

            if (infoResult.TryGetValue(out var assemblyDef) == false)
            {
                EditorUtility.DisplayDialog(importer.name, infoResult.GetErrorOrThrow().ToString(), "I understand");
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

        private static void EditReferences(AssemblyDefinitionImporter importer)
        {
            var infoResult = AssemblyDefinitionAPI.TryGetInfo(importer);

            if (infoResult.TryGetValue(out var assemblyDef) == false)
            {
                EditorUtility.DisplayDialog(importer.name, infoResult.GetErrorOrThrow().ToString(), "I understand");
                return;
            }

            var dataResult = AssemblyDefinitionAPI.TryGetData(importer.assetPath, assemblyDef);

            if (dataResult.TryGetValue(out var data) == false)
            {
                EditorUtility.DisplayDialog(importer.name, dataResult.GetErrorOrThrow().ToString(), "I understand");
                return;
            }

            AssemblyReferenceWindow.OpenWindow(data);
        }
    }
}

#endif
