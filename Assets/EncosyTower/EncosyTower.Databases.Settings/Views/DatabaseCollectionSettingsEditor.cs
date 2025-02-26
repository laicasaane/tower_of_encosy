using System;
using System.Collections.Generic;
using EncosyTower.Editor.Settings;
using EncosyTower.Types;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    using DatabaseSettings = DatabaseCollectionSettings.DatabaseSettings;

    internal partial class DatabaseCollectionSettingsEditor
    {
        public static DatabaseCollectionSettingsEditor Instance { get; private set; }

        private readonly ListView _dbListView;
        private readonly SerializedProperty _dbListProperty;
        private readonly DatabaseSettingsView _dbView;

        private bool _appliedOldIndex;

        public static DatabaseCollectionSettingsEditor Create(
              ScriptableObjectSettingsProvider provider
            , VisualElement root
        )
        {
            return Instance = new DatabaseCollectionSettingsEditor(
                  provider.Settings
                , provider.SerializedSettings
                , root
            );
        }

        public static void Dispose()
        {
            Instance = null;
        }

        private DatabaseCollectionSettingsEditor(
              ScriptableObject settings
            , SerializedObject serializedSettings
            , VisualElement root
        )
        {
            DatabaseViewAPI.ApplyStyleSheetsTo(root, Constants.PROJECT_SETTINGS_STYLE_SHEET);

            var resources = DatabaseViewAPI.GetResources();
            var context = new Context(settings);
            var titleBar = new VisualElement();
            var titleLabel = new Label() {
                text = ObjectNames.NicifyVariableName(context.Name),
            };

            titleBar.AddToClassList("project-settings-title-bar");
            titleLabel.AddToClassList("project-settings-title-bar__label");

            titleBar.Add(titleLabel);
            root.Add(titleBar);

            var scrollView = new ScrollView();
            root.Add(scrollView);

            var container = scrollView.Q("unity-content-container");
            container.AddToClassList(Constants.DATABASE_COLLECTION);

            var splitter = new TwoPaneSplitView(0, 200f, TwoPaneSplitViewOrientation.Horizontal);
            container.Add(splitter);

            var leftPanel = new VisualElement();
            leftPanel.AddToClassList(Constants.LEFT_PANEL);
            splitter.Add(leftPanel);

            var rightPanel = new VisualElement();
            rightPanel.AddToClassList(Constants.RIGHT_PANEL);
            splitter.Add(rightPanel);

            var dbListProperty = _dbListProperty
                = serializedSettings.FindProperty(nameof(DatabaseCollectionSettings._databases));

            var presetListView = new DatabasePresetListView();
            presetListView.PresetSelected += PresetListView_PresetSelected;
            rightPanel.Add(presetListView);
            rightPanel.Add(new VisualSeparator());

            var dbView = _dbView = new DatabaseSettingsView(false, resources) { userData = context };
            dbView.DatabaseTypeSelected += DbView_OnDatabaseTypeSelected;
            dbView.OtherValueUpdated += DbView_OnOtherValueUpdated;
            rightPanel.Add(dbView);

            var dbListView = _dbListView = CreateDatabaseListView(dbListProperty);
            dbListView.selectedIndicesChanged += DbListView_SelectedIndicesChanged;
            dbListView.itemsAdded += DbListView_ItemsAdded;
            dbListView.itemsRemoved += DbListView_ItemRemoved;
            dbListView.itemIndexChanged += DbListView_ItemIndexChanged;
            leftPanel.Add(dbListView);

            container.Bind(serializedSettings);
        }

        public void Update()
        {
            RefreshSelectedIndex();

            _dbView.Update();
        }

        private void RefreshSelectedIndex()
        {
            var dbListView = _dbListView;
            var dbListProperty = _dbListProperty;
            var currentIndex = (uint)dbListView.selectedIndex;
            var oldIndex = (uint)EditorPrefs.GetInt(PrefKeys.SELECTED_INDEX, -1);
            var size = (uint)dbListProperty.arraySize;

            if (size < 1)
            {
                return;
            }

            if (oldIndex != currentIndex && oldIndex < size && _appliedOldIndex == false)
            {
                _appliedOldIndex = true;
                SetSelectedIndex((int)oldIndex);
                return;
            }

            if (currentIndex >= size)
            {
                SetSelectedIndex(0);
                return;
            }
        }

        private void PresetListView_PresetSelected(DatabaseSettingsPreset preset)
        {
            if (_dbView.userData is not Context context)
            {
                return;
            }

            CopyFromPreset(context, preset);

            var serializedObject = context.SerializedObject;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);

            if (string.IsNullOrEmpty(context.Database.GetAuthorTypeProperty().stringValue))
            {
                _dbView.ToggleDisplayContainer(false);
            }
            else
            {
                _dbView.ToggleDisplayContainer(true);
            }
        }

        private void DbListView_SelectedIndicesChanged(IEnumerable<int> indices)
        {
            var dbListProperty = _dbListProperty;
            var dbView = _dbView;

            if (indices is List<int> { Count: > 0 } indexList && dbListProperty.arraySize > indexList[0])
            {
                var index = indexList[0];
                var dbProperty = dbListProperty.GetArrayElementAtIndex(index);

                Bind(dbView, dbProperty, index);
                SetSelectedIndex(index);
            }
            else
            {
                Unbind(dbView);
            }

            return;

            static void Bind(DatabaseSettingsView dbView, SerializedProperty dbProperty, int index)
            {
                var context = dbView.userData as Context;
                context.Initialize(dbProperty, index);

                dbView.visible = true;
                dbView.Bind(context);
            }

            static void Unbind(DatabaseSettingsView dbView)
            {
                dbView.visible = false;
                dbView.Unbind();
            }
        }

        private void DbListView_ItemsAdded(IEnumerable<int> indices)
        {
            var dbListProperty = _dbListProperty;

            foreach (var index in indices)
            {
                var item = dbListProperty.GetArrayElementAtIndex(index);
                var nameProperty = item.FindPropertyRelative(nameof(DatabaseSettings.name));
                var authorTypeProperty = item.FindPropertyRelative(nameof(DatabaseSettings.authorType));
                var databaseTypeProperty = item.FindPropertyRelative(nameof(DatabaseSettings.databaseType));

                nameProperty.stringValue = Constants.UNDEFINED;
                authorTypeProperty.stringValue = Constants.UNDEFINED;
                databaseTypeProperty.stringValue = Constants.UNDEFINED;
            }

            var serializedObject = dbListProperty.serializedObject;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);

            SetSelectedIndex(_dbListView.itemsSource.Count);
        }

        private void DbListView_ItemRemoved(IEnumerable<int> indices)
        {
            var dbListProperty = _dbListProperty;
            var serializedObject = dbListProperty.serializedObject;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private void DbListView_ItemIndexChanged(int oldIndex, int newIndex)
        {
            var dbListProperty = _dbListProperty;
            var serializedObject = dbListProperty.serializedObject;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);

            SetSelectedIndex(newIndex);
        }

        private void DbView_OnDatabaseTypeSelected(DatabaseSettingsView dbView, DatabaseRecord record)
        {
            var context = dbView.userData as Context;
            var serializedObject = context.SerializedObject;

            var nameProperty = context.Database.GetNameProperty();
            var authorTypeProperty = context.Database.GetAuthorTypeProperty();
            var databaseTypeProperty = context.Database.GetDatabaseTypeProperty();

            if (record?.AuthorType == null || record?.DatabaseType == null)
            {
                nameProperty.stringValue = Constants.UNDEFINED;
                authorTypeProperty.stringValue = Constants.UNDEFINED;
                databaseTypeProperty.stringValue = Constants.UNDEFINED;
                dbView.ToggleDisplayContainer(false);
            }
            else
            {
                nameProperty.stringValue = record.Name;
                authorTypeProperty.stringValue = record.AuthorType.AssemblyQualifiedName;
                databaseTypeProperty.stringValue = record.DatabaseType.AssemblyQualifiedName;
                dbView.ToggleDisplayContainer(true);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private void DbView_OnOtherValueUpdated(DatabaseSettingsView dbView)
        {
            var context = dbView.userData as Context;
            var serializedObject = context.SerializedObject;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private void SetSelectedIndex(int value)
        {
            if (_dbListView.selectedIndex != value)
            {
                _dbListView.selectedIndex = value;
            }

            EditorPrefs.SetInt(PrefKeys.SELECTED_INDEX, value);
        }

        private static ListView CreateDatabaseListView(SerializedProperty dbListProperty)
        {
            var listView = new ListView {
                bindingPath = dbListProperty.propertyPath,
                reorderable = true,
                allowAdd = true,
                allowRemove = true,
                showAddRemoveFooter = true,
                showFoldoutHeader = false,
                showBorder = true,
                horizontalScrollingEnabled = false,
                showBoundCollectionSize = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                selectionType = SelectionType.Single,
                makeItem = MakeItem,
                bindItem = (root, index) => BindItem(root, index, dbListProperty),
            };

            listView.AddToClassList(Constants.DATABASE_SELECTOR_LIST);

            return listView;

            static VisualElement MakeItem()
            {
                var root = new VisualElement();
                root.AddToClassList(Constants.DATABASE_SELECTOR);

                var label = new Label { text = Constants.UNDEFINED };
                label.AddToClassList(Constants.NAME);

                root.Add(label);
                return root;
            }

            static void BindItem(VisualElement root, int index, SerializedProperty dbListProperty)
            {
                var nameProperty = dbListProperty.GetArrayElementAtIndex(index)
                    .FindPropertyRelative(nameof(DatabaseSettings.name));

                var label = root.Q<Label>(className: Constants.NAME);

                /// README
                Notes.ToPreventTwoWayBindingWhenSearch();

                var labelNotifier = (INotifyValueChanged<string>)label;
                labelNotifier.SetValueWithoutNotify(nameProperty.stringValue);

                label.Unbind();
                label.TrackPropertyValue(nameProperty, prop => {
                    labelNotifier.SetValueWithoutNotify(prop.stringValue);
                });
            }
        }

        private class Context : DatabaseSettingsContext
        {
            public readonly Type Type;
            public readonly string Name;

            private readonly DatabaseCollectionSettings _settings;

            public Context(ScriptableObject so)
            {
                _settings = so as DatabaseCollectionSettings;

                Type = _settings.GetType();
                Name = Type.GetNameWithoutSuffix("Settings");
            }

            public void Initialize(SerializedProperty databaseProperty, int databaseIndex)
                => Initialize(_settings._databases[databaseIndex], databaseProperty);
        }

        [CustomEditor(typeof(DatabaseCollectionSettings), true)]
        private sealed class Editor : UnityEditor.Editor
        {
            private DatabaseCollectionSettings _settings;

            private void OnEnable()
            {
                _settings = target as DatabaseCollectionSettings;
            }

            public override VisualElement CreateInspectorGUI()
            {
                var root = new VisualElement();
                DatabaseViewAPI.ApplyStyleSheetsTo(root, Constants.PROJECT_SETTINGS_STYLE_SHEET);

                var button = new Button(OpenSettingsWindow) {
                    text = "Open Database Collection Settings Window",
                };

                button.AddToClassList("button-open-settings-windows");
                root.Add(button);
                return root;
            }

            private void OpenSettingsWindow()
            {
                if (_settings.IsValid())
                {
                    _settings.OpenSettingsWindow();
                }
            }
        }
    }
}
