#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Module.Core.Collections;
using Module.Core.Editor;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ComponentModel.SourceGen;
using Module.Core.Mvvm.Input;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Mvvm.ViewBinding.SourceGen;
using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    using TypeCache = UnityEditor.TypeCache;

    [CustomEditor(typeof(MonoView), editorForChildClasses: true)]
    internal sealed partial class MonoViewInspector : UnityEditor.Editor
    {
        /// <summary>
        /// Map direction: [Destination Type => [Source Type => Adapter Type HashSet]]
        /// </summary>
        /// <remarks>
        /// Does not include <see cref="Module.Core.Extended.Mvvm.ViewBinding.Adapters.Unity.ScriptableAdapter"/>
        /// and <see cref="Module.Core.Mvvm.ViewBinding.Adapters.CompositeAdapter"/>
        /// </remarks>
        private readonly static Dictionary<DestType, Dictionary<SourceType, HashSet<Type>>> s_adapterMap = new(128);
        private readonly static GenericMenuPopup s_binderMenu = new(new MenuItemNode(), "Binders");
        private readonly static GenericMenuPopup s_contextMenu = new(new MenuItemNode(), "Contexts");
        private readonly static Dictionary<Type, Type> s_binderToTargetTypeMap = new(128);
        private readonly static Dictionary<Type, Type> s_bindingToTargetTypeMap = new(128);
        private readonly static Dictionary<Type, MemberMap> s_bindingPropertyMap = new(128);
        private readonly static Dictionary<Type, MemberMap> s_bindingCommandMap = new(128);
        private readonly static Dictionary<Type, GenericMenuPopup> s_targetTypeToBindingMenuMap = new(128);
        private readonly static Dictionary<Type, Type> s_contextToInspectorMap = new(4);
        private readonly static PropertyRef s_contextPropRef = new();
        private readonly static PropertyRef s_bindersPropRef = new();
        private readonly static PropertyRef s_binderPropRef = new();
        private static SerializedObject s_copiedObject;

        private const int TAB_INDEX_BINDINGS = 0;
        private const int TAB_INDEX_TARGETS = 1;

        private const string NO_BINDING = "Has no binding!";
        private const string NO_TARGET = "Has no target!";
        private const string NO_BINDING_TARGET = "Has no binding and target!";

        private const string PROP_PRESET_BINDERS = "_presetBinders";
        private const string PROP_PRESET_BINDINGS = "_presetBindings";
        private const string PROP_PRESET_TARGETS = "_presetTargets";
        private const string PROP_SUBTITLE = "_subtitle";

        private const string PROP_TARGET_PROPERTY_NAME = "<TargetPropertyName>k__BackingField";
        private const string PROP_TARGET_COMMAND_NAME = "<TargetCommandName>k__BackingField";
        private const string PROP_ADAPTER = "<Adapter>k__BackingField";

        private MonoView _view;
        private string _subtitleControlName = "";
        private string _binderSubtitle = "";
        private SerializedProperty _selectedSubtitleProp;
        private int _selectedDetailsTabIndex;
        private int? _selectedSubtitleIndex;
        private SerializedProperty _contextProp;
        private Type _contextType;
        private SerializedArrayProperty _presetBindersProp;
        private SerializedArrayProperty _presetBindingsProp;
        private SerializedArrayProperty _presetTargetsProp;
        private ObservableContextInspector _contextInspector;

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

            _contextProp = serializedObject.FindProperty("_context");

            _presetBindersProp = new(
                  nameof(MonoBinder)
                , static (src, dest) => dest.managedReferenceValue = src.managedReferenceValue
                , OnValidatePasteAllBinders
                , OnValidatePasteSingleBinder
                , $"{instanceId}_selected_binder_index"
                , OnSetSelectedBinderIndex
            );

            _presetBindingsProp = new(
                  nameof(MonoBinding)
                , static (src, dest) => dest.managedReferenceValue = src.managedReferenceValue
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

            EditorUtility.DisplayProgressBar("Initializing", "Please wait...", 0f);
            EditorApplication.update += InitInspector;
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
            EditorApplication.update -= InitInspector;
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

            _presetBindersProp.RefreshSelectedIndex();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            {
                DrawContext();
                UpdateContextMaps();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                DrawBindersPanel(eventData);
                GUILayout.Space(4f);
                DrawDetailsPanel(eventData);
            }
            EditorGUILayout.EndHorizontal();

            if (ValidateSelectedSubtitleIndex())
            {
                EditorGUI.FocusTextInControl(_subtitleControlName);
            }
        }

        private void DrawContext()
        {
            if (_contextInspector == null
                && _contextProp.managedReferenceValue is ObservableContext context
                && s_contextToInspectorMap.TryGetValue(context.GetType(), out var inspectorType)
            )
            {
                _contextInspector = Activator.CreateInstance(inspectorType) as ObservableContextInspector;
                _contextInspector.ContextType = context.GetType();
                _contextInspector.OnEnable(_view, serializedObject, _contextProp);
            }

            EditorGUILayout.BeginVertical(s_rootTabViewStyle);
            {
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
                {
                    EditorGUILayout.Space(22);
                    DrawContextHeader(rect);
                }
                EditorGUILayout.EndHorizontal();

                if (_contextInspector != null)
                {
                    EditorGUILayout.Space(6f);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(6f);
                        EditorGUILayout.BeginVertical();
                        {
                            _contextInspector.OnInspectorGUI();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(4f);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(6f);
                }
                else
                {
                    GUILayout.Label("No observable context is chosen.", s_noBinderStyle, GUILayout.Height(30));
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawContextHeader(in Rect rect)
        {
            var labelStyle = s_rootTabLabelStyle;
            var buttonSize = 80f;

            // Draw background
            {
                var backRect = rect;
                backRect.x += 1f;
                backRect.y += 1f;
                backRect.width -= 3f;
                backRect.height += 3f;

                var tex = Texture2D.whiteTexture;
                var mode = ScaleMode.StretchToFill;
                var borders = Vector4.zero;
                var radius = new Vector4(3f, 3f, 0f, 0f);

                GUI.DrawTexture(backRect, tex, mode, false, 0f, s_headerColor, borders, radius);
            }

            // Draw label
            {
                var labelRect = rect;
                labelRect.y += 1f;
                labelRect.width -= buttonSize + 2f;

                _contextLabel.text = _contextInspector == null
                    ? "<Invalid Observable Context>"
                    : ObjectNames.NicifyVariableName(_contextInspector.ContextType.Name);

                GUI.Label(labelRect, _contextLabel, labelStyle);
            }

            {
                var btnRect = rect;
                btnRect.x += rect.width - buttonSize - 3.5f;
                btnRect.y += 2f;
                btnRect.width = buttonSize;

                if (GUI.Button(btnRect, s_chooseLabel, s_chooseContextButtonStyle))
                {
                    s_contextPropRef.Prop = _contextProp;
                    s_contextPropRef.Inspector = this;

                    var menu = s_contextMenu;
                    menu.width = 250;
                    menu.height = 350;
                    menu.maxHeight = 600;
                    menu.showSearch = true;
                    menu.Show(Event.current.mousePosition);
                }
            }
        }

        private static void ContextMenu_SetContext(object userData)
        {
            if (userData is not MenuItemContext menuItem)
            {
                return;
            }

            var (contextType, inspectorType, propRef) = menuItem;
            var contextInspector = Activator.CreateInstance(inspectorType) as ObservableContextInspector;
            contextInspector.ContextType = contextType;
            propRef.Inspector._contextInspector = contextInspector;

            var property = propRef.Inspector._contextProp;
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;

            Undo.RecordObject(target, "Set Observable Context");

            property.managedReferenceValue = Activator.CreateInstance(contextType);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
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

            if (_contextProp.managedReferenceValue is not ObservableContext context
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

        private static void InitContextToInspector()
        {
            if (s_contextToInspectorMap.Count > 0)
            {
                return;
            }

            var contextTypes = TypeCache.GetTypesDerivedFrom<ObservableContextInspector>()
                .Where(static x => x.IsAbstract == false && x.GetConstructor(Type.EmptyTypes) != null)
                .Select(static x => (x, x.GetCustomAttribute<ObservableContextInspectorAttribute>()))
                .Where(static x => x.Item2 != null && x.Item2.ContextType != null);

            var typeMap = s_contextToInspectorMap;
            var rootNode = s_contextMenu.rootNode;

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

            EditorUtility.DisplayProgressBar("Initializing", "Please wait...", 0.25f);

            InitAdapterMap();

            EditorUtility.DisplayProgressBar("Initializing", "Please wait...", 0.5f);

            InitBinderMenu();

            EditorUtility.DisplayProgressBar("Initializing", "Please wait...", 0.75f);

            InitBindingMenuMap();

            EditorUtility.ClearProgressBar();
            EditorApplication.update -= InitInspector;
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
            var rootNode = s_binderMenu.rootNode;

            if (rootNode.Nodes.Count > 0)
            {
                return;
            }

            var defs = TypeCache.GetTypesDerivedFrom<MonoBinder>()
                .Where(ValidateType)
                .Select(ToDefinition<MonoBinder>)
                .Where(static x => x.IsValid);

            var typeMap = s_binderToTargetTypeMap;

            foreach (var (binderType, targetType, label, directory) in defs)
            {
                if (typeMap.TryAdd(binderType, targetType) == false)
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

                var currentNode = menu.rootNode;
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

            foreach (var (destType, map) in s_adapterMap)
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

            return destParent != null
                && destParent.propertyType == SerializedPropertyType.ManagedReference
                && destParent.managedReferenceValue is MonoBinder destBinder
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

            return srcObject && destParent != null
                && destParent.propertyType == SerializedPropertyType.ManagedReference
                && destParent.managedReferenceValue is MonoBinder destBinder
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
