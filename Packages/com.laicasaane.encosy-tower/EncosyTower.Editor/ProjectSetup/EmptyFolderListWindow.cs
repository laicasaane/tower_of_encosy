#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EncosyTower.Collections.Extensions;
using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.ProjectSetup
{
    public sealed class EmptyFolderListWindow : EditorWindow
    {
        [MenuItem("Encosy Tower/Empty Folders")]
        public static void OpenWindow()
        {
            var window = GetWindow<EmptyFolderListWindow>();
            window.titleContent = new GUIContent("Empty Folders");
            window.ShowPopup();
        }

        [SerializeField] private ThemeStyleSheet _themeStyleSheet;

        private SimpleTableView<ItemInfo> _table;

        private void CreateGUI()
        {
            rootVisualElement.WithStyleSheet(_themeStyleSheet);

            var buttonGroup = new VisualElement() {
                name = "button-group",
            };

            rootVisualElement.Add(buttonGroup);

            buttonGroup.Add(new Button(Refresh) {
                text = "Refresh",
                name = "refresh-button",
            });

            buttonGroup.Add(new Button(Delete) {
                text = "Delete",
                name = "delete-button",
            });

            CreateTable();
            Refresh();
        }

        private void Refresh()
        {
            FindItems(_table.Items);
        }

        private void Delete()
        {
            var span = _table.Items.AsSpan();
            var length = span.Length;
            var items = new List<ItemInfo>(length);
            var paths = new List<string>(length);

            for (var i = length - 1; i >= 0; i--)
            {
                var item = span[i];

                if (item.selected == false)
                {
                    items.Add(item);
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(item.asset);
                paths.Add(path);
            }

            _table.Items.Clear();
            _table.Items.AddRange(items);

            if (paths.Count < 1)
            {
                return;
            }

            var failed = new List<string>(paths.Count);
            AssetDatabase.DeleteAssets(paths.ToArray(), failed);
        }

        private void CreateTable()
        {
            var table = _table = new() {
                bindingPath = nameof(ItemCollectionAsset.items),
                showBoundCollectionSize = false,
            };

            rootVisualElement.Add(table);

            var column = table.AddColumn(" ", 10);
            column.maxWidth = 10;

            column = table.AddColumn("Delete?"
                , 60
                , TableColumn_MakeCell_Select
                , TableColumn_BindCell_Select
            );

            column.maxWidth = 60;

            column = table.AddColumn("Folder"
                , 200
                , TableColumn_MakeCell_Folder
                , TableColumn_BindCell_Folder
                , header: new(ItemInfo.CompareNames)
            );

            column.resizable = true;
            column.sortable = true;

            column = table.AddColumn("Path"
                , 200
                , TableColumn_MakeCell_Path
                , TableColumn_BindCell_Path
                , header: new(ItemInfo.ComparePaths)
            );

            column.resizable = true;
            column.sortable = true;

            var asset = CreateInstance<ItemCollectionAsset>();
            asset.items = _table.Items;

            var serializedObject = new SerializedObject(asset);
            _table.Bind(serializedObject);
        }

        private static Toggle TableColumn_MakeCell_Select()
        {
            var field = new Toggle();
            field.AddToClassList("select-field");

            return field;
        }

        private static void TableColumn_BindCell_Select(VisualElement element, ItemInfo item)
        {
            if (element is not Toggle field || item is null)
            {
                return;
            }

            field.value = item.selected;
        }

        private static ObjectField TableColumn_MakeCell_Folder()
        {
            var field = new ObjectField {
                allowSceneObjects = false
            };

            field.AddToClassList("folder-field");

            return field;
        }

        private static void TableColumn_BindCell_Folder(VisualElement element, ItemInfo item)
        {
            if (element is not ObjectField field || item is null)
            {
                return;
            }

            field.value = item.asset;
        }

        private static Label TableColumn_MakeCell_Path()
        {
            var label = new Label();
            label.AddToClassList("folder-path");

            label.RegisterCallback<PointerDownEvent>(_ => {
                if (label.userData is SceneAsset asset && asset.IsValid())
                {
                    EditorGUIUtility.PingObject(asset);
                }
            });

            return label;
        }

        private static void TableColumn_BindCell_Path(VisualElement element, ItemInfo item)
        {
            if (element is not Label label || item is null)
            {
                return;
            }

            label.text = item.path;
            label.userData = item.asset;
        }

        private static void FindItems(List<ItemInfo> result)
        {
            result.Clear();

            var directoryInfo = new DirectoryInfo(Application.dataPath);
            var rootPath = Path.Combine(Application.dataPath, "..");
            var items = new List<ItemInfo>();

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

                if (asset.IsInvalid())
                {
                    continue;
                }

                items.Add(new ItemInfo {
                    asset = asset,
                    path = path,
                    selected = true,
                });
            }

            if (items.Count < 1)
            {
                result = default;
                return;
            }

            result.AddRange(items);
            return;

            static bool IsNotMeta(FileInfo file)
            {
                return file.FullName.EndsWith(".meta", StringComparison.Ordinal) == false;
            }
        }

        [Serializable]
        private class ItemInfo
        {
            public DefaultAsset asset;
            public string path;
            public bool selected;

            public static int CompareNames(ItemInfo a, ItemInfo b)
                => string.Compare(a.asset.name, b.asset.name, StringComparison.Ordinal);

            public static int ComparePaths(ItemInfo a, ItemInfo b)
                => string.Compare(a.path, b.path, StringComparison.Ordinal);
        }

        private class ItemCollectionAsset : ScriptableObject
        {
            public List<ItemInfo> items;
        }
    }
}

#endif
