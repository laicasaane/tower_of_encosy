using System;
using System.Collections.Generic;
using Module.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Module.Core.Scenes.Editor
{
    internal sealed class SceneListWindow : EditorWindow
    {
        [MenuItem("Core/Scene List")]
        private static void OpenWindow()
        {
            var window = GetWindow<SceneListWindow>();
            window.titleContent = new GUIContent(LIST_TITLE);
            window.ShowPopup();
        }

        private static readonly string[] s_folders = new[] { "Assets" };

        private const string LIST_TITLE = "Scene List";
        private const string LIST_ZERO_MSG = "Found 0 scene...";

        private SceneInfo[] _scenes;
        private SimpleTableView<SceneInfo> _table;

        private static bool FindScenes(out SceneInfo[] result)
        {
            var guids = AssetDatabase.FindAssets("t:SceneAsset", s_folders).AsSpan();

            if (guids.Length < 1)
            {
                EditorUtility.DisplayDialog(LIST_TITLE, "No scene found in the project.", "OK");
                result = default;
                return false;
            }

            var scenes = new List<SceneInfo>(guids.Length);

            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                if (scene == false)
                {
                    continue;
                }

                scenes.Add(new SceneInfo {
                    scene = scene,
                    path = path,
                });
            }

            result = scenes.ToArray();
            return true;
        }

        private void OnEnable()
        {
            if (FindScenes(out var scenes) == false)
            {
                return;
            }

            _scenes = scenes;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh", GUILayout.Height(22)))
            {
                OnEnable();
                return;
            }

            var scenes = _scenes.AsSpan();

            if (scenes.Length < 1)
            {
                EditorGUILayout.LabelField(LIST_ZERO_MSG);
                return;
            }

            BuildTable();

            EditorGUILayout.Space();

            _table.DrawTableGUI(_scenes, rowHeight: EditorGUIUtility.singleLineHeight * 1.7f);
            _table.ResizeToFit();
        }

        private void BuildTable()
        {
            if (_table != null)
            {
                return;
            }

            var table = _table = new();

            table.AddColumn(" ", 10, TableColumn_Empty)
                .SetMaxWidth(10);

            table.AddColumn("Scene", 200, TableColumn_Scene)
                .SetAutoResize(true)
                .SetSorting((a, b) => string.Compare(a.scene.name, b.scene.name, StringComparison.Ordinal));

            table.AddColumn("Path", 200, TableColumn_Path)
                .SetAutoResize(true)
                .SetSorting((a, b) => string.Compare(a.path, b.path, StringComparison.Ordinal));
        }

        private static void TableColumn_Empty(Rect rect, SceneInfo item)
        {

        }

        private static void TableColumn_Scene(Rect rect, SceneInfo item)
        {
            if (item == null || item.scene == false)
            {
                return;
            }

            var height = rect.height;

            rect.height = 20f;
            rect.width -= 24f;
            rect.x += 12f;
            rect.y += (height - 20f) / 2f;

            if (GUI.Button(rect, item.scene.name))
            {
                EditorSceneManager.OpenScene(item.path, OpenSceneMode.Single);
            }
        }

        private static void TableColumn_Path(Rect rect, SceneInfo item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.path))
            {
                return;
            }

            var label = new GUIContent(item.path);
            EditorStyles.linkLabel.CalcMinMaxWidth(label, out _, out var maxW);

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.width = Mathf.Min(rect.width, maxW);
            rect.y += 4f;

            if (EditorGUI.LinkButton(rect, label))
            {
                Selection.activeObject = item.scene;
            }
        }

        private class SceneInfo
        {
            public SceneAsset scene;
            public string path;
        }
    }
}
