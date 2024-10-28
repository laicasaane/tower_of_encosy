#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor.ProjectSetup
{
    public sealed class EmptyFolderListWindow : EditorWindow
    {
        [MenuItem("Encosy Tower/Empty Folders")]
        public static void OpenWindow()
        {
            var window = GetWindow<EmptyFolderListWindow>();
            window.titleContent = new GUIContent(LIST_TITLE);
            window.ShowPopup();
        }

        private const string LIST_TITLE = "Empty Folders";
        private const string LIST_ZERO_MSG = "Found 0 empty folder...";

        private EmptyFolderInfo[] _folders;
        private SimpleTableView<EmptyFolderInfo> _table;

        public void OnEnable()
        {
            if (FindEmptyFolders(out var folders) == false)
            {
                return;
            }

            _folders = folders;
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Refresh", GUILayout.Height(22)))
                {
                    OnEnable();
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                if (GUILayout.Button("Delete", GUILayout.Height(22)))
                {
                    Delete();
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            var folders = _folders.AsSpan();

            if (folders.Length < 1)
            {
                EditorGUILayout.LabelField(LIST_ZERO_MSG);
                return;
            }

            BuildTable();

            EditorGUILayout.Space();

            _table.DrawTableGUI(_folders, rowHeight: EditorGUIUtility.singleLineHeight * 1.7f);
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

            table.AddColumn("Delete?", 60, TableColumn_Select)
                .SetMaxWidth(60);

            table.AddColumn("Folder", 200, TableColumn_Folder)
                .SetAutoResize(true)
                .SetSorting((a, b) => string.Compare(a.asset.name, b.asset.name, StringComparison.Ordinal));

            table.AddColumn("Path", 200, TableColumn_Path)
                .SetAutoResize(true)
                .SetSorting((a, b) => string.Compare(a.path, b.path, StringComparison.Ordinal));
        }

        private static void TableColumn_Empty(Rect rect, EmptyFolderInfo item)
        {

        }

        private static void TableColumn_Select(Rect rect, EmptyFolderInfo item)
        {
            if (item == null)
            {
                return;
            }

            rect.x += 30f - (GUI.skin.toggle.fixedWidth / 2f);
            rect.width = GUI.skin.toggle.fixedWidth;

            item.selected = EditorGUI.Toggle(rect, item.selected);
        }

        private static void TableColumn_Folder(Rect rect, EmptyFolderInfo item)
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

            EditorGUI.ObjectField(rect, item.asset, typeof(DefaultAsset), false);
        }

        private static void TableColumn_Path(Rect rect, EmptyFolderInfo item)
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

        private void Delete()
        {
            var span = _folders.AsSpan();
            var length = span.Length;
            var folders = new List<EmptyFolderInfo>(length);
            var paths = new List<string>(length);

            for (var i = length - 1; i >= 0; i--)
            {
                var folder = span[i];

                if (folder.selected == false)
                {
                    folders.Add(folder);
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(folder.asset);
                paths.Add(path);
            }

            _folders = folders.ToArray();

            if (paths.Count < 1)
            {
                return;
            }

            var failed = new List<string>(paths.Count);
            AssetDatabase.DeleteAssets(paths.ToArray(), failed);
        }

        private static bool FindEmptyFolders(out EmptyFolderInfo[] result)
        {
            var directoryInfo = new DirectoryInfo(Application.dataPath);
            var rootPath = Path.Combine(Application.dataPath, "..");
            var folders = new List<EmptyFolderInfo>();

            foreach (var subDirectory in directoryInfo.GetDirectories("*.*", SearchOption.AllDirectories))
            {
                if (subDirectory.Exists == false)
                {
                    continue;
                }

                var files = subDirectory.GetFiles("*.*", SearchOption.AllDirectories);

                if (files.Length > 0 && files.Any(IsNotMeta))
                {
                    continue;
                }

                var path = Path.GetRelativePath(rootPath, subDirectory.FullName);
                var guid = AssetDatabase.AssetPathToGUID(path);

                if (string.IsNullOrWhiteSpace(guid))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);

                if (asset == false)
                {
                    continue;
                }

                folders.Add(new EmptyFolderInfo {
                    asset = asset,
                    path = path,
                    selected = true,
                });
            }

            if (folders.Count < 1)
            {
                EditorUtility.DisplayDialog(LIST_TITLE, "No empty folder found in the project.", "I understand");
                result = default;
                return false;
            }

            result = folders.ToArray();
            return true;

            static bool IsNotMeta(FileInfo file)
            {
                return file.FullName.EndsWith(".meta", StringComparison.Ordinal) == false;
            }
        }

        private class EmptyFolderInfo
        {
            public DefaultAsset asset;
            public string path;
            public bool selected;
        }
    }
}

#endif
