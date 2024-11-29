#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EncosyTower.Modules.Mvvm.ComponentModel.SourceGen;
using EncosyTower.Modules.Mvvm.Input;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Mvvm.ViewBinding.SourceGen;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
{
    using TypeCache = UnityEditor.TypeCache;

    [CustomEditor(typeof(MonoView), editorForChildClasses: true)]
    internal sealed partial class MonoViewInspector : UnityEditor.Editor
    {
        /// <summary>
        /// Map direction: [Destination Type => [Source Type => Adapter Type HashSet]]
        /// </summary>
        /// <remarks>
        /// Does not include <see cref="EncosyTower.Modules.Mvvm.ViewBinding.Adapters.Unity.ScriptableAdapter"/>
        /// and <see cref="EncosyTower.Modules.Mvvm.ViewBinding.Adapters.CompositeAdapter"/>
        /// </remarks>
        private readonly static Dictionary<DestType, Dictionary<SourceType, HashSet<Type>>> s_adapterMap = new(128);
        private readonly static GenericMenuPopup s_binderMenu = new(new MenuItemNode(), "Binders");
        private readonly static GenericMenuPopup s_contextMenu = new(new MenuItemNode(), "Contexts");
        private readonly static Dictionary<Type, Type> s_binderToTargetTypeMap = new(128);
        private readonly static Dictionary<Type, Type> s_targetTypeToBinderMap = new(128);
        private readonly static Dictionary<Type, Type> s_bindingToTargetTypeMap = new(128);
        private readonly static Dictionary<Type, MemberMap> s_bindingPropertyMap = new(128);
        private readonly static Dictionary<Type, MemberMap> s_bindingCommandMap = new(128);
        private readonly static Dictionary<Type, GenericMenuPopup> s_targetTypeToBindingMenuMap = new(128);
        private readonly static Dictionary<Type, Type> s_contextToInspectorMap = new(4);
        private readonly static PropertyRef s_contextPropRef = new();
        private readonly static PropertyRef s_bindersPropRef = new();
        private readonly static PropertyRef s_binderPropRef = new();
        private static SerializedObject s_copiedObject;

        private static readonly string[] s_settingFieldNames = typeof(MonoViewSettings)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(static x => x.IsPublic || x.GetCustomAttribute<SerializeField>() != null)
            .Select(static x => x.Name)
            .ToArray();

        private const string NO_BINDING = "Has no binding!";
        private const string NO_TARGET = "Has no target!";
        private const string NO_BINDING_TARGET = "Has no binding and target!";

        private const string PROP_PRESET_BINDERS = "_presetBinders";
        private const string PROP_PRESET_BINDINGS = "_presetBindings";
        private const string PROP_PRESET_TARGETS = "_presetTargets";
        private const string PROP_SUBTITLE = "_subtitle";

        private MonoView _view;
        private string _subtitleControlName = "";
        private string _binderSubtitle = "";
        private SerializedProperty _selectedSubtitleProp;
        private int? _selectedSubtitleIndex;
        private SerializedProperty _settingsProp;
        private SerializedProperty _contextProp;
        private Type _contextType;
        private SerializedArrayProperty _presetBindersProp;
        private SerializedArrayProperty _presetBindingsProp;
        private SerializedArrayProperty _presetTargetsProp;
        private BindingContextInspector _contextInspector;

        private readonly GUIContent _settingsLabel = new("Settings");
        private readonly GUIContent _contextLabel = new();
        private readonly Dictionary<string, Type> _contextPropertyMap = new();
        private readonly Dictionary<string, Type> _contextCommandMap = new();

        private void OnEnable()
        {
            if (target is not MonoView view)
            {
                return;
            }

            _view = view;

            var instanceId = _view.GetInstanceID();

            _settingsProp = serializedObject.FindProperty("_settings");
            _contextProp = serializedObject.FindProperty("_context");

            _presetBindersProp = new(
                  nameof(MonoBinder)
                , CloneMonoBinder
                , OnValidatePasteAllBinders
                , OnValidatePasteSingleBinder
                , $"{instanceId}_selected_binder_index"
                , OnSetSelectedBinderIndex
            );

            _presetBindingsProp = new(
                  nameof(MonoBinding)
                , CloneManagedReference
                , OnValidatePasteAllBindings
                , OnValidatePasteSingleBinding
            );

            _presetTargetsProp = new(
                  "MonoBindingTarget"
                , static (src, dest) => dest.objectReferenceValue = src.objectReferenceValue
                , OnValidatePasteAllTargets
                , OnValidatePasteSingleTarget
            );

            _presetBindersProp.Intialize(serializedObject.FindProperty(PROP_PRESET_BINDERS));
            _presetBindersProp.LoadSelectedIndex();
            _presetBindersProp.SetSelectedIndex(_presetBindersProp.SelectedIndex);

            InitInspector();
        }

        private static void CloneManagedReference(SerializedProperty src, SerializedProperty dest)
        {
            var type = src.managedReferenceValue.GetType();
            var cloned = Activator.CreateInstance(type);
            var json = EditorJsonUtility.ToJson(src.managedReferenceValue, false);
            EditorJsonUtility.FromJsonOverwrite(json, cloned);
            dest.managedReferenceValue = cloned;
        }

        private static void CloneMonoBinder(SerializedProperty src, SerializedProperty dest)
        {
            CloneManagedReference(src, dest);

            var srcTargets = src.FindPropertyRelative(PROP_PRESET_TARGETS);
            var destTargets = dest.FindPropertyRelative(PROP_PRESET_TARGETS);
            var targetsLength = srcTargets.arraySize;

            destTargets.arraySize = targetsLength;

            for (var i = 0; i < targetsLength; i++)
            {
                var srcTarget = srcTargets.GetArrayElementAtIndex(i);
                var destTarget = destTargets.GetArrayElementAtIndex(i);
                destTarget.objectReferenceValue = srcTarget.objectReferenceValue;
            }
        }

        private void OnDisable()
        {
            OnDestroy();
        }

        private void OnDestroy()
        {
            s_bindersPropRef?.Reset();
            s_binderPropRef?.Reset();
            _contextInspector?.OnDestroy();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_view == false)
            {
                return;
            }

            InitStyles();

            EventData eventData = Event.current;

            if (ConsumeEvent(eventData))
            {
                return;
            }

            _presetBindersProp.RefreshSelectedIndex();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            {
                DrawSettingsPanel();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            {
                DrawContextPanel();
                UpdateContextMaps();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                DrawBindersPanel(eventData);
                GUILayout.Space(4f);
                PreventSerializedPropertyHasDisappearedError();
                DrawDetailsPanels(eventData);
            }
            EditorGUILayout.EndHorizontal();

            if (ValidateSelectedSubtitleIndex())
            {
                EditorGUI.FocusTextInControl(_subtitleControlName);
            }
        }

        private bool ConsumeEvent(in EventData eventData)
        {
            var isKeyUp = eventData.Type == EventType.KeyUp;
            var key = eventData.Key;

            switch (key)
            {
                case KeyCode.Return:
                {
                    if (isKeyUp && ValidateSelectedSubtitleIndex())
                    {
                        ApplyBinderSubtitle();
                        Event.current.Use();
                        return true;
                    }
                    break;
                }

                case KeyCode.Escape:
                {
                    if (isKeyUp && ValidateSelectedSubtitleIndex())
                    {
                        SetSelectedSubtitleIndex(null);
                        Event.current.Use();
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        private void DrawDragDropArea(
              in Rect dropRect
            , in EventData eventData
            , Action<Memory<UnityEngine.Object>> onDrop = null
        )
        {
            if (eventData.Type is not (EventType.DragUpdated or EventType.DragPerform))
            {
                return;
            }

            if (dropRect.Contains(eventData.MousePos) == false)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (eventData.Type != EventType.DragPerform)
            {
                return;
            }

            DragAndDrop.AcceptDrag();
            onDrop?.Invoke(DragAndDrop.objectReferences);
        }

        private static void ContextMenu_SetContext(object userData)
        {
            if (userData is not MenuItemContext menuItem)
            {
                return;
            }

            var (contextType, inspectorType, propRef) = menuItem;
            var contextInspector = Activator.CreateInstance(inspectorType) as BindingContextInspector;
            contextInspector.ContextType = contextType;
            propRef.Inspector._contextInspector = contextInspector;

            var property = propRef.Inspector._contextProp;
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, "Set Binding Context");

            property.managedReferenceValue = Activator.CreateInstance(contextType);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void DrawPanelHeaderLabel(
              GUIContent label
            , GUILayoutOption guiWidth
            , in Rect rect
            , in Rect offset = default
            , GUIContent icon = null
        )
        {
            var labelStyle = s_rootTabLabelStyle;

            // Draw background
            {
                var backRect = rect;
                backRect.x += 1f + offset.x;
                backRect.y += 1f + offset.y;
                backRect.width -= 3f - offset.width;
                backRect.height -= 1f - offset.height;

                var tex = Texture2D.whiteTexture;
                var mode = ScaleMode.StretchToFill;
                var borders = Vector4.zero;
                var radius = new Vector4(3f, 3f, 0f, 0f);

                GUI.DrawTexture(backRect, tex, mode, false, 0f, s_headerColor, borders, radius);
            }

            // Draw icon
            if (icon != null)
            {
                labelStyle.CalcMinMaxWidth(label, out var minWidth, out _);
                var minHeight = labelStyle.CalcHeight(label, minWidth);

                var iconRect = rect;
                iconRect.x += 5f;
                iconRect.y += (rect.height - minHeight) / 2f - 3f;
                iconRect.width = 20f;
                iconRect.height = 20f;

                var tex = icon.image;
                var mode = ScaleMode.ScaleToFit;
                var borders = Vector4.zero;
                var radius = Vector4.zero;

                GUI.DrawTexture(iconRect, tex, mode, true, 1f, Color.white, borders, radius);
            }

            EditorGUILayout.LabelField(label, labelStyle, guiWidth, GUILayout.Height(26));
        }

        private static void DrawPanelHeaderFoldout(in Rect rect, SerializedProperty property, GUIContent label, in Rect offset = default)
        {
            var isExpanded = property.isExpanded;

            // Draw background
            {
                var backRect = rect;
                backRect.x += 1f + offset.x;
                backRect.y += 1f + offset.y;
                backRect.width -= 3f + offset.width;
                backRect.height -= isExpanded ? (3f + offset.height) : 2f;

                var tex = Texture2D.whiteTexture;
                var mode = ScaleMode.StretchToFill;
                var borders = Vector4.zero;
                var radius = isExpanded ? new Vector4(3f, 3f, 0f, 0f) : new Vector4(3f, 3f, 3f, 3f);

                GUI.DrawTexture(backRect, tex, mode, false, 0f, s_headerColor, borders, radius);
            }

            // Draw label
            {
                var labelRect = rect;
                labelRect.y += 1f;

                if (GUI.Button(rect, label, s_rootTabLabelStyle))
                {
                    isExpanded = property.isExpanded = !isExpanded;
                }
            }

            // Draw arrow
            {
                var labelRect = rect;
                labelRect.width = 24f;
                labelRect.y += isExpanded ? 1f : 0f;

                var icon = isExpanded ? s_foldoutExpandedIconLabel : s_foldoutCollapsedIconLabel;
                GUI.Label(labelRect, icon);
            }
        }

        /// <remarks>
        /// 'SerializedProperty has disappeared!' happens when a binder is removed
        /// or the recent removal is reverted via keyboard shortcut.
        /// This error causes the Inspector to be corrupted.
        /// </remarks>
        private void PreventSerializedPropertyHasDisappearedError()
        {
            if (_presetBindersProp.TryGetAtSelectedIndex(out var binderProperty))
            {
                _presetBindingsProp.Intialize(binderProperty.FindPropertyRelative(PROP_PRESET_BINDINGS));
                _presetTargetsProp.Intialize(binderProperty.FindPropertyRelative(PROP_PRESET_TARGETS));
            }
            else
            {
                _presetBindingsProp.SetSelectedIndex(null);
                _presetTargetsProp.SetSelectedIndex(null);
                _presetBindingsProp.Intialize(null);
                _presetTargetsProp.Intialize(null);
            }
        }

        private void OnSetSelectedBinderIndex(SerializedArrayProperty property)
        {
            _presetBindingsProp.SetSelectedIndex(null);
            _presetTargetsProp.SetSelectedIndex(null);

            if (property.TryGetAtSelectedIndex(out var binderProperty))
            {
                _presetBindingsProp.Intialize(binderProperty.FindPropertyRelative(PROP_PRESET_BINDINGS));
                _presetTargetsProp.Intialize(binderProperty.FindPropertyRelative(PROP_PRESET_TARGETS));
            }
            else
            {
                _presetBindingsProp.Intialize(null);
                _presetTargetsProp.Intialize(null);
            }
        }

        private void SetSelectedSubtitleIndex(int? value)
        {
            if (_presetBindersProp.ValidateIndex(value))
            {
                _selectedSubtitleIndex = value;
                _subtitleControlName = $"sub_name_{value.Value}";
                _selectedSubtitleProp = _presetBindersProp.GetElementAt(value.Value)
                    .FindPropertyRelative(PROP_SUBTITLE);

                _binderSubtitle = _selectedSubtitleProp.stringValue;
            }
            else
            {
                _selectedSubtitleIndex = null;
                _subtitleControlName = string.Empty;
                _selectedSubtitleProp = null;
                _binderSubtitle = string.Empty;
            }
        }

        private bool ValidateSelectedSubtitleIndex()
            => _presetBindersProp.ValidateIndex(_selectedSubtitleIndex);

        private void UpdateContextMaps()
        {
            var propMap = _contextPropertyMap;
            var cmdMap = _contextCommandMap;

            propMap.Clear();
            cmdMap.Clear();

            if (_contextProp.managedReferenceValue is not IBindingContext context
                || context.TryGetContextType(out var contextType) == false
            )
            {
                return;
            }

            _contextType = contextType;

            var propertyAttribs = contextType.GetCustomAttributes<NotifyPropertyChangedInfoAttribute>();

            foreach (var attrib in propertyAttribs)
            {
                if (propMap.ContainsKey(attrib.PropertyName) == false)
                {
                    propMap[attrib.PropertyName] = attrib.PropertyType;
                }
            }

            var commandAttribs = contextType.GetCustomAttributes<RelayCommandInfoAttribute>();

            foreach (var attrib in commandAttribs)
            {
                if (cmdMap.ContainsKey(attrib.CommandName) == false)
                {
                    cmdMap[attrib.CommandName] = attrib.ParameterType;
                }
            }
        }

        private static void BuildRightClickTextMenu(GenericMenu menu, SerializedArrayProperty property)
        {
            menu.AddItem(s_copyItemLabel, false, Menu_OnCopySelected, property);

            if (property.ValidatePasteSingle())
            {
                menu.AddItem(s_pasteItemLabel, false, Menu_OnPasteSingle, property);
            }
            else
            {
                menu.AddDisabledItem(s_pasteItemLabel);
            }

            menu.AddItem(s_deleteItemLabel, false, Menu_OnDeleteSelected, property);

            menu.AddSeparator(string.Empty);

            var selectedIndex = property.SelectedIndex;

            if (selectedIndex is > 0)
            {
                menu.AddItem(s_moveUpLabel, false, Menu_OnMoveUpSelected, property);
            }
            else
            {
                menu.AddDisabledItem(s_moveUpLabel);
            }

            if (selectedIndex.HasValue && selectedIndex.Value < property.ArraySize - 1)
            {
                menu.AddItem(s_moveDownLabel, false, Menu_OnMoveDownSelected, property);
            }
            else
            {
                menu.AddDisabledItem(s_moveDownLabel);
            }
        }

        private static void Menu_OnCopyAll(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.CopyAll();
        }

        private static void Menu_OnPasteAll(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.PasteAll();
        }

        private static void Menu_OnClearAll(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.ClearAll();
        }

        private static void Menu_OnCopySelected(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.CopySelected();
        }

        private static void Menu_OnPasteSingle(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.PasteSingle();
        }

        private static void Menu_OnDeleteSelected(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.DeleteSelected();
        }

        private static void Menu_OnMoveUpSelected(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.MoveSelectedItemUp();

        }

        private static void Menu_OnMoveDownSelected(object userData)
        {
            if (userData is not SerializedArrayProperty property)
            {
                return;
            }

            property.MoveSelectedItemDown();
        }

        private static void InitContextToInspector()
        {
            if (s_contextToInspectorMap.Count > 0)
            {
                return;
            }

            var contextTypes = TypeCache.GetTypesDerivedFrom<BindingContextInspector>()
                .Where(static x => x.IsAbstract == false && x.GetConstructor(Type.EmptyTypes) != null)
                .Select(static x => (x, x.GetCustomAttribute<BindingContextInspectorAttribute>()))
                .Where(static x => x.Item2 is { ContextType: not null });

            var typeMap = s_contextToInspectorMap;
            var rootNode = s_contextMenu.RootNode;

            foreach (var (inspectorType, inspectorAttrib) in contextTypes)
            {
                var contextType = inspectorAttrib.ContextType;

                if (contextType == null)
                {
                    continue;
                }

                if (typeMap.TryAdd(contextType, inspectorType) == false)
                {
                    continue;
                }

                var labelAttrib = contextType.GetCustomAttribute<LabelAttribute>();
                string label, directory;

                if (labelAttrib != null)
                {
                    label = labelAttrib.Label ?? contextType.Name;
                    directory = labelAttrib.Directory ?? string.Empty;
                }
                else
                {
                    label = contextType.Name;
                    directory = contextType.Namespace;
                }

                var currentNode = rootNode;
                var hasDirectory = string.IsNullOrWhiteSpace(directory) == false;

                currentNode = hasDirectory ? currentNode.GetOrCreateNode(directory) : currentNode;
                currentNode = currentNode.CreateNode(label);

                currentNode.content = hasDirectory ? new($"{directory}/{label}") : new(label);
                currentNode.func2 = ContextMenu_SetContext;
                currentNode.userData = new MenuItemContext(contextType, inspectorType, s_contextPropRef);
                currentNode.on = false;
            }
        }

        private static void InitInspector()
        {
            InitContextToInspector();
            InitAdapterMap();
            InitBinderMenu();
            InitBindingMenuMap();
        }

        private static void InitAdapterMap()
        {
            if (s_adapterMap.Count > 0)
            {
                return;
            }

            var adapterTypes = TypeCache.GetTypesDerivedFrom<IAdapter>()
                .Where(static x => x.IsAbstract == false && x.IsSubclassOf(typeof(UnityEngine.Object)) == false)
                .Select(static x => (x, x.GetCustomAttribute<AdapterAttribute>()))
                .Where(static x => x.Item2 != null);

            foreach (var (adapterType, attrib) in adapterTypes)
            {
                if (s_adapterMap.TryGetValue(attrib.DestinationType, out var map) == false)
                {
                    s_adapterMap[attrib.DestinationType] = map = new(128);
                }

                if (map.TryGetValue(attrib.SourceType, out var types) == false)
                {
                    map[attrib.SourceType] = types = new(128);
                }

                types.Add(adapterType);
            }
        }

        private static void InitBinderMenu()
        {
            var rootNode = s_binderMenu.RootNode;

            if (rootNode.Nodes.Count > 0)
            {
                return;
            }

            var defs = TypeCache.GetTypesDerivedFrom<MonoBinder>()
                .Where(ValidateType)
                .Select(ToDefinition<MonoBinder>)
                .Where(static x => x.IsValid);

            var binderMap = s_targetTypeToBinderMap;
            var targetTypeMap = s_binderToTargetTypeMap;

            foreach (var (binderType, targetType, label, directory) in defs)
            {
                if (binderMap.TryAdd(targetType, binderType) == false
                    || targetTypeMap.TryAdd(binderType, targetType) == false
                )
                {
                    continue;
                }

                var currentNode = rootNode;
                var hasDirectory = string.IsNullOrWhiteSpace(directory) == false;

                currentNode = hasDirectory ? currentNode.GetOrCreateNode(directory) : currentNode;
                currentNode = currentNode.CreateNode(label);

                currentNode.content = hasDirectory ? new($"{directory}/{label}") : new(label);
                currentNode.func2 = BinderMenu_AddBinder;
                currentNode.userData = new MenuItemBinder(binderType, targetType, s_bindersPropRef);
                currentNode.on = false;
            }
        }

        private static void InitBindingMenuMap()
        {
            var menuMap = s_targetTypeToBindingMenuMap;

            if (menuMap.Count > 0)
            {
                return;
            }

            var bindingMap = s_bindingToTargetTypeMap;
            var bindingPropertyMap = s_bindingPropertyMap;
            var bindingCommandMap = s_bindingCommandMap;

            var defs = TypeCache.GetTypesDerivedFrom<MonoBinding>()
                .Where(ValidateType)
                .Select(ToDefinition<MonoBinding>)
                .Where(static x => x.IsValid);

            foreach (var (bindingType, targetType, label, _) in defs)
            {
                if (menuMap.TryGetValue(targetType, out var menu) == false)
                {
                    menuMap[targetType] = menu = new(new MenuItemNode(), "Bindings");
                }

                bindingMap[bindingType] = targetType;

                if (bindingPropertyMap.TryGetValue(bindingType, out var propertyMap) == false)
                {
                    bindingPropertyMap[bindingType] = propertyMap = new();
                }

                if (bindingCommandMap.TryGetValue(bindingType, out var commandMap) == false)
                {
                    bindingCommandMap[bindingType] = commandMap = new();
                }

                var propertyAttribs = bindingType.GetCustomAttributes<BindingPropertyMethodInfoAttribute>();

                foreach (var attrib in propertyAttribs)
                {
                    propertyMap.TryAdd(attrib.MethodName, attrib.ParameterType);
                }

                var commandAttribs = bindingType.GetCustomAttributes<BindingCommandMethodInfoAttribute>();

                foreach (var attrib in commandAttribs)
                {
                    commandMap.TryAdd(attrib.MethodName, attrib.ParameterType);
                }

                var currentNode = menu.RootNode;
                currentNode = currentNode.CreateNode(label);

                currentNode.content = new(label);
                currentNode.func2 = BindingMenu_AddBinding;
                currentNode.userData = new MenuItemBinding(bindingType, targetType, s_binderPropRef);
                currentNode.on = false;
            }
        }

        private static bool ValidateType(Type type)
        {
            return type.IsAbstract == false
                && type.GetCustomAttribute<SerializableAttribute>() != null
                && type.GetConstructor(Type.EmptyTypes) != null
                ;
        }

        private static BinderDefinition ToDefinition<TBase>(Type type)
        {
            if (FindFirstTypeArg(typeof(TBase), type, out var targetType) == false)
            {
                return default;
            }

            var attrib = type.GetCustomAttribute<LabelAttribute>();

            if (attrib != null)
            {
                return new(type, targetType, attrib.Label ?? type.Name, attrib.Directory ?? "");
            }
            else
            {
                return new(type, targetType, type.Name, type.Namespace);
            }
        }

        private static bool FindFirstTypeArg(Type baseType, Type type, out Type typeArg)
        {
            var tempType = type;

            while (tempType != null)
            {
                var args = tempType.GetGenericArguments().AsSpan();

                if (args.Length > 0 && tempType.Name.StartsWith(baseType.Name, StringComparison.Ordinal))
                {
                    typeArg = args[0];
                    return true;
                }

                tempType = tempType.BaseType;

                if (tempType == baseType)
                {
                    break;
                }
            }

            typeArg = default;
            return false;
        }

        private static List<Type> GetOtherAdapterTypesExcludeSourceType(SourceType sourceTypeToExclude)
        {
            var result = new List<Type>();

            foreach (var (_, map) in s_adapterMap)
            {
                foreach (var (sourceType, types) in map)
                {
                    if (sourceType.Equals(sourceTypeToExclude))
                    {
                        continue;
                    }

                    var orderedTypes = types.OrderBy(static x => {
                        var attrib = x.GetCustomAttribute<AdapterAttribute>();
                        return attrib?.Order ?? AdapterAttribute.DEFAULT_ORDER;
                    });

                    result.AddRange(orderedTypes);
                }
            }

            return result;
        }

        private static bool OnValidatePasteAllBinders(SerializedProperty src, SerializedArrayProperty dest)
        {
            return src.isArray
                && src.propertyPath.EndsWith(PROP_PRESET_BINDERS, StringComparison.Ordinal)
                && dest.Property.propertyPath.Equals(PROP_PRESET_BINDERS, StringComparison.Ordinal);
        }

        private static bool OnValidatePasteSingleBinder(SerializedProperty src, SerializedArrayProperty dest)
        {
            return src.isArray == false
                && src.propertyType == SerializedPropertyType.ManagedReference
                && src.managedReferenceValue is MonoBinder
                && dest.Property.propertyPath.Equals(PROP_PRESET_BINDERS, StringComparison.Ordinal);
        }

        private static bool OnValidatePasteAllBindings(SerializedProperty src, SerializedArrayProperty dest)
            => ValidatePasteAll(src, dest, PROP_PRESET_BINDINGS);

        private static bool OnValidatePasteSingleBinding(SerializedProperty src, SerializedArrayProperty dest)
        {
            if (src.isArray
                || src.propertyType != SerializedPropertyType.ManagedReference
                || src.managedReferenceValue is not MonoBinding srcBinding
                || dest.Property.propertyPath.EndsWith(PROP_PRESET_BINDINGS, StringComparison.Ordinal) == false
            )
            {
                return false;
            }

            var destParent = dest.Property.FindParentProperty();

            return destParent is { propertyType: SerializedPropertyType.ManagedReference, managedReferenceValue: MonoBinder destBinder }
                   && s_binderToTargetTypeMap.TryGetValue(destBinder.GetType(), out var destTargetType)
                   && s_bindingToTargetTypeMap.TryGetValue(srcBinding.GetType(), out var srcTargetType)
                   && srcTargetType == destTargetType;
        }

        private static bool OnValidatePasteAllTargets(SerializedProperty src, SerializedArrayProperty dest)
            => ValidatePasteAll(src, dest, PROP_PRESET_TARGETS);

        private static bool OnValidatePasteSingleTarget(SerializedProperty src, SerializedArrayProperty dest)
        {
            if (src.isArray
                || src.propertyType != SerializedPropertyType.ObjectReference
                || dest.Property.propertyPath.EndsWith(PROP_PRESET_TARGETS, StringComparison.Ordinal) == false
            )
            {
                return false;
            }

            var srcObject = src.objectReferenceValue;
            var destParent = dest.Property.FindParentProperty();

            return srcObject
                && destParent is { propertyType: SerializedPropertyType.ManagedReference, managedReferenceValue: MonoBinder destBinder }
                && s_binderToTargetTypeMap.TryGetValue(destBinder.GetType(), out var destTargetType)
                && destTargetType.IsAssignableFrom(srcObject.GetType());
        }

        private static bool ValidatePasteAll(
              SerializedProperty src
            , SerializedArrayProperty dest
            , string endWithPropName
        )
        {
            if (src.isArray == false
                || src.propertyPath.EndsWith(endWithPropName, StringComparison.Ordinal) == false
                || dest.Property.propertyPath.EndsWith(endWithPropName, StringComparison.Ordinal) == false
            )
            {
                return false;
            }

            var srcParent = src.FindParentProperty();
            var destParent = dest.Property.FindParentProperty();

            return srcParent != null && destParent != null
                && srcParent.propertyType == SerializedPropertyType.ManagedReference
                && destParent.propertyType == SerializedPropertyType.ManagedReference
                && srcParent.managedReferenceValue is MonoBinder srcBinder
                && destParent.managedReferenceValue is MonoBinder destBinder
                && s_binderToTargetTypeMap.TryGetValue(srcBinder.GetType(), out var srcTargetType)
                && s_binderToTargetTypeMap.TryGetValue(destBinder.GetType(), out var destTargetType)
                && srcTargetType == destTargetType;
        }
    }
}

#endif
