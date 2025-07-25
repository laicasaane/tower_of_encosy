#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Scenes
{
    internal sealed class SceneListWindow : EditorWindow
    {
        [MenuItem("Encosy Tower/Scenes")]
        public static void OpenWindow()
        {
            var window = GetWindow<SceneListWindow>();
            window.titleContent = new GUIContent("Scenes");
            window.ShowPopup();
        }

        private static readonly string[] s_folders = new[] { "Assets" };

        [SerializeField] private ThemeStyleSheet _themeStyleSheet;

        private SimpleTableView<ItemInfo> _table;

        private void CreateGUI()
        {
            rootVisualElement.styleSheets.Add(_themeStyleSheet);

            rootVisualElement.Add(new Button(Refresh) {
                text = "Refresh",
                name = "refresh-button",
            });

            CreateTable();
            Refresh();
        }

        private void Refresh()
        {
            FindItems(_table.Items);
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

            column = table.AddColumn("Open Scene"
                , 200
                , TableColumn_MakeCell_Scene
                , TableColumn_BindCell_Scene
                , header: new(ItemInfo.CompareNames)
            );

            column.resizable = true;
            column.sortable = true;

            column = table.AddColumn("Locate Asset"
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

        private static Button TableColumn_MakeCell_Scene()
        {
            var button = new Button();
            button.AddToClassList("scene-button");

            button.clicked += () => {
                if (button.userData is string path && path.IsNotEmpty())
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                }
            };

            return button;
        }

        private static void TableColumn_BindCell_Scene(VisualElement element, ItemInfo item)
        {
            if (element is not Button button || item is null)
            {
                return;
            }

            button.text = item.asset.name;
            button.userData = item.path;
        }

        private static Label TableColumn_MakeCell_Path()
        {
            var label = new Label();
            label.AddToClassList("scene-path");

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

            var guids = AssetDatabase.FindAssets("t:SceneAsset", s_folders).AsSpan();

            if (guids.Length < 1)
            {
                result = default;
                return;
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
                    name = asset.name,
                    path = path,
                });
            }

            items.Sort(Sort);
            result.AddRange(items);
            return;

            static int Sort(ItemInfo x, ItemInfo y)
                => StringComparer.OrdinalIgnoreCase.Compare(x.path, y.path);
        }

        [Serializable]
        private class ItemInfo
        {
            public SceneAsset asset;
            public string name;
            public string path;

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
