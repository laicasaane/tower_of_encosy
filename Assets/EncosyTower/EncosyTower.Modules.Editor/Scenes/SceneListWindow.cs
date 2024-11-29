#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Scenes
{
    internal sealed class SceneListWindow : EditorWindow
    {
        [MenuItem("Encosy Tower/Scenes")]
        public static void OpenWindow()
        {
            var window = GetWindow<SceneListWindow>();
            window.titleContent = new GUIContent(LIST_TITLE);
            window.ShowPopup();
        }

        private static readonly string[] s_folders = new[] { "Assets" };

        private const string LIST_TITLE = "Scenes";
        private const string LIST_ZERO_MSG = "Found 0 scene...";

        private ItemInfo[] _items;
        private SimpleTableView<ItemInfo> _table;

        public void OnEnable()
        {
            if (FindItems(out var items) == false)
            {
                return;
            }

            _items = items;
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh", GUILayout.Height(22)))
            {
                OnEnable();
                return;
            }

            var items = _items.AsSpan();

            if (items.Length < 1)
            {
                EditorGUILayout.LabelField(LIST_ZERO_MSG);
                return;
            }

            BuildTable();

            EditorGUILayout.Space();

            _table.DrawTableGUI(_items, rowHeight: EditorGUIUtility.singleLineHeight * 1.7f);
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
                .SetSorting((a, b) => string.Compare(a.asset.name, b.asset.name, StringComparison.Ordinal));

            table.AddColumn("Path", 200, TableColumn_Path)
                .SetAutoResize(true)
                .SetSorting((a, b) => string.Compare(a.path, b.path, StringComparison.Ordinal));
        }

        private static void TableColumn_Empty(Rect rect, ItemInfo item)
        {

        }

        private static void TableColumn_Scene(Rect rect, ItemInfo item)
        {
            if (item == null || item.asset == false)
            {
                return;
            }

            var height = rect.height;

            rect.height = 20f;
            rect.width -= 24f;
            rect.x += 12f;
            rect.y += (height - 20f) / 2f;

            if (GUI.Button(rect, item.asset.name))
            {
                EditorSceneManager.OpenScene(item.path, OpenSceneMode.Single);
            }
        }

        private static void TableColumn_Path(Rect rect, ItemInfo item)
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
                Selection.activeObject = item.asset;
            }
        }

        private static bool FindItems(out ItemInfo[] result)
        {
            var guids = AssetDatabase.FindAssets("t:SceneAsset", s_folders).AsSpan();

            if (guids.Length < 1)
            {
                EditorUtility.DisplayDialog(LIST_TITLE, "No scene found in the project.", "I understand");
                result = default;
                return false;
            }

            var items = new List<ItemInfo>(guids.Length);

            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                if (asset == false)
                {
                    continue;
                }

                items.Add(new ItemInfo {
                    asset = asset,
                    path = path,
                });
            }

            items.Sort(Sort);

            result = items.ToArray();
            return true;

            static int Sort(ItemInfo x, ItemInfo y)
                => StringComparer.OrdinalIgnoreCase.Compare(x.path, y.path);
        }

        private class ItemInfo
        {
            public SceneAsset asset;
            public string path;
        }
    }
}

#endif
