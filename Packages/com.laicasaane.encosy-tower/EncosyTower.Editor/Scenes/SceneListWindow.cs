#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor.UIElements;
using EncosyTower.Search;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Scenes
{
    internal sealed class SceneListWindow : EditorWindow
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/Scenes";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(SceneListWindow);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.uss";

        private static readonly string[] s_folders1 = new[] { "Assets" };
        private static readonly string[] s_folders2 = new[] { "Assets", "Packages" };

        private readonly List<ItemInfo> _items = new();

        private ItemCollectionAsset _asset;
        private SerializedObject _serializedObject;
        private SimpleTableView<ItemInfo> _table;
        private Toggle _includePackagesToggle;

        [MenuItem("Encosy Tower/Scenes", priority = 83_67_00_00)]
        public static void OpenWindow()
        {
            var window = GetWindow<SceneListWindow>();
            window.titleContent = new GUIContent("Scenes");
            window.ShowPopup();
        }

        private void CreateGUI()
        {
            rootVisualElement.WithEditorStyleSheet(THEME_STYLE_SHEET);

            var toolbar = new Toolbar();
            rootVisualElement.Add(toolbar);

            toolbar.Add(new ToolbarButton(Refresh) {
                text = "Refresh",
                name = "refresh-button",
            });

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(_includePackagesToggle = new ToolbarToggle {
                text = "Include Packages",
                tooltip = "Include all embedded packages",
                name = "include-packages-toggle",
                value = true,
            });

            _includePackagesToggle.RegisterValueChangedCallback(IncludePackagesToggle_OnValueChanged);

            var searchField = new ToolbarSearchField {
                name = "search-field"
            };

            searchField.RegisterValueChangedCallback(SearchField_OnValueChanged);

            toolbar.Add(searchField);

            CreateTable();
            Refresh();
        }

        private void Refresh()
        {
            FindItems(_items, _includePackagesToggle.value);

            _asset.items.Clear();
            _asset.items.AddRange(_items);
        }

        private void IncludePackagesToggle_OnValueChanged(ChangeEvent<bool> evt)
        {
            Refresh();
        }

        private void SearchField_OnValueChanged(ChangeEvent<string> evt)
        {
            _table.Unbind();

            var dest = _asset.items;
            dest.Clear();

            FuzzySearchAPI.Search(_items, evt.newValue, dest, new SearchValidator());

            _table.Bind(_serializedObject);
        }

        private void CreateTable()
        {
            var table = _table = new SimpleTableView<ItemInfo>() {
                bindingPath = nameof(ItemCollectionAsset.items),
                showBoundCollectionSize = false,
            }.WithEditorStyleSheets();

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

            _asset = CreateInstance<ItemCollectionAsset>();
            _asset.items = _table.Items;

            _serializedObject = new SerializedObject(_asset);
            _table.Bind(_serializedObject);
        }

        private static Button TableColumn_MakeCell_Scene()
        {
            var button = new Button();
            button.AddToClassList("asset-button");

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
            label.AddToClassList("asset-path");

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

        private static void FindItems(List<ItemInfo> result, bool includePackages)
        {
            result.Clear();

            var folders = includePackages ? s_folders2 : s_folders1;
            var guids = AssetDatabase.FindAssets("t:SceneAsset", folders).AsSpan();

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
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);

                if (package != null && package.source != PackageSource.Embedded)
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                if (asset.IsInvalid())
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

        private readonly struct SearchValidator : ISearchValidator<ItemInfo>
        {
            public bool IsSearchable(ItemInfo item)
                => item.path.IsNotEmpty();

            public string GetSearchableString(ItemInfo item)
                => item.name;
        }
    }
}

#endif
