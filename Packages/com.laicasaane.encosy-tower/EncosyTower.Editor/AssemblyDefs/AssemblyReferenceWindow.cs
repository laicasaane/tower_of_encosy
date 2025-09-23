#if UNITY_EDITOR

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EncosyTower.Editor.UIElements;
using EncosyTower.Search;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.AssemblyDefs
{
    internal class AssemblyReferenceWindow : EditorWindow
    {
        [SerializeField] private ThemeStyleSheet _themeStyleSheet;
        [SerializeField] private StyleSheet _darkThemeStyleSheet;
        [SerializeField] private StyleSheet _lightThemeStyleSheet;

        private TabView _tabView;
        private AssemblyReferenceTab _tabAll;
        private AssemblyReferenceTab _tabFiltered;

        public static void OpenWindow(AssemblyData assemblyData)
        {
            var window = GetWindow<AssemblyReferenceWindow>();
            window.Initialize(assemblyData);
            window.ShowAuxWindow();
        }

        private void Initialize(AssemblyData assemblyData)
        {
            titleContent = new("Assembly References");
            wantsMouseMove = true;

            var allReferences = (IReadOnlyList<AssemblyReferenceData>)assemblyData?.AllReferences
                ?? Array.Empty<AssemblyReferenceData>();

            var filteredReferences = (IReadOnlyList<AssemblyReferenceData>)assemblyData?.FilteredReferences
                ?? Array.Empty<AssemblyReferenceData>();

            OnCreateGUI();

            _tabAll.Update(
                assemblyData.AssetPath,
                assemblyData.Asset,
                assemblyData.AssemblyDefinition,
                assemblyData.UseGuid,
                allReferences
            );

            _tabFiltered.Update(
                assemblyData.AssetPath,
                assemblyData.Asset,
                assemblyData.AssemblyDefinition,
                assemblyData.UseGuid,
                filteredReferences
            );
        }

        private void OnCreateGUI()
        {
            if (_tabView != null)
            {
                return;
            }

            rootVisualElement.WithStyleSheet(_themeStyleSheet);
            rootVisualElement.WithEditorStyleSheet(_darkThemeStyleSheet, _lightThemeStyleSheet);

            _tabAll = new AssemblyReferenceTab("All", Close);
            _tabFiltered = new AssemblyReferenceTab("Filtered", Close);

            _tabView = new TabView();
            _tabView.Add(_tabAll);
            _tabView.Add(_tabFiltered);

            rootVisualElement.Add(_tabView);
        }

        private class ReferenceField : VisualElement
        {
            public static readonly string ReferenceFieldUssClassName = "reference-field";
            public static readonly string ReferenceToggleUssClassName = "reference-toggle";
            public static readonly string ReferenceLabelUssClassName = "reference-label";
            public static readonly string ReferenceObjectUssClassName = "reference-object";
            public static readonly string ReferenceBarUssClassName = "reference-bar";
            public static readonly string ItemAsHeaderUssClassName = "item-as-header";

            public bool isHeader = false;
            public AssemblyReferenceData data;

            private readonly Toggle _toggle;
            private readonly Label _label;
            private readonly ObjectField _objectField;
            private readonly VisualElement _bar;

            public ReferenceField(string label, AssemblyReferenceData data, bool isSeparator = false)
            {
                AddToClassList(ReferenceFieldUssClassName);

                _toggle = new(string.Empty);
                _toggle.AddToClassList(ReferenceToggleUssClassName);
                _toggle.RegisterValueChangedCallback(Toggle_OnValueChanged);

                Add(_toggle);

                _bar = new();
                _bar.AddToClassList(ReferenceBarUssClassName);

                _label = new(label);
                _label.AddToClassList(ReferenceLabelUssClassName);

                _objectField = new(string.Empty) {
                    objectType = typeof(AssemblyDefinitionAsset)
                };

                _objectField.AddToClassList(ReferenceObjectUssClassName);

                _bar.Add(_label);
                _bar.Add(_objectField);

                Add(_bar);

                UpdateHeader(label, data, isSeparator);
            }

            public void UpdateHeader(string label, AssemblyReferenceData data, bool isHeader)
            {
                this.isHeader = isHeader;
                this.data = data;

                if (data == null)
                {
                    return;
                }

                EnableInClassList(ItemAsHeaderUssClassName, isHeader);

                focusable = !isHeader;
                _toggle.enabledSelf = !isHeader;
                _label.focusable = !isHeader;
                _objectField.enabledSelf = !isHeader;

                _toggle.value = !isHeader && data.selected;
                _label.text = isHeader ? data.headerText : label;
                _objectField.value = isHeader ? null : data.asset;
            }

            private void Toggle_OnValueChanged(ChangeEvent<bool> evt)
            {
                data.selected = evt.newValue;
            }
        }

        private class AssemblyReferenceTab : Tab
        {
            public static readonly string ListViewUssClassName = "list-view";
            public static readonly string ListEmptyLabelUssClassName = "list-empty-label";
            public static readonly string AssemblyContainerUssClassName = "assembly-container";
            public static readonly string SelectButtonUssClassName = "select-button";
            public static readonly string SearchFieldUssClassName = "search-field";
            public static readonly string ToolFieldUssClassName = "tool-field";
            public static readonly string ApplyButtonUssClassName = "apply-button";
            public static readonly string CancelButtonUssClassName = "cancel-button";
            public static readonly string ApplyButtonGroupUssClassName = "apply-button-group";
            public static readonly string ApplyGroupUssClassName = "apply-group";
            public static readonly string ApplyAssemblyUssClassName = "apply-assembly";

            private readonly List<AssemblyReferenceData> _listViewItemSource = new();
            private readonly List<AssemblyReferenceData> _references = new();

            private readonly Label _emptyLabel;
            private readonly VisualElement _container;
            private readonly ListView _listView;

            private readonly Action _onClose;
            private readonly ObjectField _applyAssembly;

            private string _assetPath;
            private AssemblyDefinitionInfo _assemblyDef;
            private bool _useGuid;

            public AssemblyReferenceTab(string label, Action onClose) : base(label)
            {
                _onClose = onClose;

                _emptyLabel = new Label("This list is empty");
                _emptyLabel.AddToClassList(ListEmptyLabelUssClassName);
                Add(_emptyLabel);

                _container = new VisualElement();
                _container.AddToClassList(AssemblyContainerUssClassName);
                Add(_container);

                var selectButton = new Button(SelectButton_OnClick) {
                    text = "Select all",
                };

                selectButton.AddToClassList(SelectButtonUssClassName);

                var deselectButton = new Button(DeselectButton_OnClick) {
                    text = "Deselect all"
                };

                deselectButton.AddToClassList(SelectButtonUssClassName);

                var searchField = new ToolbarSearchField();
                searchField.AddToClassList(SearchFieldUssClassName);
                searchField.RegisterValueChangedCallback(SearchField_OnValueChanged);

                var toolField = new VisualElement();
                toolField.AddToClassList(ToolFieldUssClassName);
                toolField.Add(selectButton);
                toolField.Add(deselectButton);
                toolField.Add(searchField);

                _container.Add(toolField);

                _listView = new() {
                    itemsSource = _listViewItemSource,
                    fixedItemHeight = 22.5f,
                    makeItem = ListView_OnMakeItem,
                    bindItem = ListView_OnBindItem,
                };

                _listView.AddToClassList(ListViewUssClassName);

                _container.Add(_listView);

                var applyButton = new Button(ApplyButton_OnClick) {
                    text = "OK",
                };

                applyButton.AddToClassList(ApplyButtonUssClassName);

                var cancelButton = new Button(CancelButton_OnClick) {
                    text = "Cancel"
                };

                cancelButton.AddToClassList(CancelButtonUssClassName);

                var buttonGroup = new VisualElement();
                buttonGroup.AddToClassList(ApplyButtonGroupUssClassName);
                buttonGroup.Add(applyButton);
                buttonGroup.Add(cancelButton);

                var applyGroup = new VisualElement();
                applyGroup.AddToClassList(ApplyGroupUssClassName);

                _applyAssembly = new("Apply to assembly") {
                    objectType = typeof(AssemblyDefinitionAsset)
                };

                _applyAssembly.AddToClassList(ApplyAssemblyUssClassName);

                applyGroup.Add(_applyAssembly);
                applyGroup.Add(buttonGroup);

                _container.Add(applyGroup);
            }

            public void Update(
                  string assetPath
                , AssemblyDefinitionAsset asset
                , AssemblyDefinitionInfo assemblyDef
                , bool useGuid
                , IReadOnlyList<AssemblyReferenceData> references
            )
            {
                _assetPath = assetPath;
                _assemblyDef = assemblyDef;
                _useGuid = useGuid;

                _applyAssembly.value = asset;

                _references.Clear();
                _references.AddRange(references);

                _listViewItemSource.Clear();
                _listViewItemSource.AddRange(_references);

                _listView.Rebuild();

                var hasItems = references.Count > 0;
                _emptyLabel.WithDisplay(hasItems ? DisplayStyle.None : DisplayStyle.Flex);
                _container.WithDisplay(hasItems ? DisplayStyle.Flex : DisplayStyle.None);
            }

            private void ApplyButton_OnClick()
            {
                ApplyReferences(_assetPath, _assemblyDef, _references, _useGuid);
                _onClose?.Invoke();
            }

            private void CancelButton_OnClick()
            {
                _onClose?.Invoke();
            }

            private void SearchField_OnValueChanged(ChangeEvent<string> evt)
            {
                FuzzySearchAPI.Search(_references, evt.newValue, _listViewItemSource, new SearchValidator());
                _listView?.Rebuild();
            }

            private void SelectButton_OnClick()
            {
                foreach (var item in _references)
                {
                    item.selected = true;
                }
            }

            private void DeselectButton_OnClick()
            {
                foreach (var item in _references)
                {
                    item.selected = false;
                }
            }

            private ReferenceField ListView_OnMakeItem()
                => new(string.Empty, null);

            private void ListView_OnBindItem(VisualElement element, int index)
            {
                if (element is not ReferenceField field)
                {
                    return;
                }

                var item = _listViewItemSource[index];
                field.UpdateHeader(item.Name, item, item.IsHeader);
            }

            private static void ApplyReferences(
                  string assetPath
                , AssemblyDefinitionInfo assemblyDef
                , List<AssemblyReferenceData> references
                , bool useGuid
            )
            {
                if (assemblyDef is null)
                {
                    return;
                }

                references.Sort(Compare);

                assemblyDef.references = references
                    .Where(static x => x != null)
                    .Where(static x => x.IsHeader == false)
                    .Where(static x => x.selected)
                    .Select(x => useGuid ? x.guidString : x.Name)
                    .ToArray();

                var json = EditorJsonUtility.ToJson(assemblyDef, true);

                File.WriteAllText(assetPath, json);
                AssetDatabase.Refresh();
                return;

                static int Compare(AssemblyReferenceData x, AssemblyReferenceData y)
                {
                    return StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
                }
            }

            private readonly struct SearchValidator : ISearchValidator<AssemblyReferenceData>
            {
                public string GetSearchableString(AssemblyReferenceData item)
                    => item.Name;

                public bool IsSearchable(AssemblyReferenceData item)
                    => item.IsHeader == false;
            }
        }
    }
}

#endif
