#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Module.Core.Collections;
using Module.Core.Editor;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.TypeWrap;
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
        private readonly static Dictionary<Type, Type> s_binderToTargetTypeMap = new(128);
        private readonly static Dictionary<Type, GenericMenuPopup> s_bindingMenuMap = new(128);
        private readonly static BindersPropRef s_bindersPropRef = new();
        private readonly static BinderPropRef s_binderPropRef = new();

        private const int TAB_INDEX_BINDINGS = 0;
        private const int TAB_INDEX_TARGETS = 1;

        private const string NO_BINDING = "Has no binding!";
        private const string NO_TARGET = "Has no target!";
        private const string NO_BINDING_TARGET = "Has no binding and target!";

        private MonoView _view;
        private string _selectedBinderIndexKey;
        private SerializedProperty _presetBindersProp;
        private int? _selectedBinderIndex;
        private int _selectedDetailsTabIndex;
        private int? _selectedSubtitleIndex;
        private string _subtitleControlName = "";
        private string _binderSubtitle = "";
        private SerializedProperty _selectedSubtitleProp;

        private void OnEnable()
        {
            if (target is not MonoView view)
            {
                return;
            }

            _view = view;
            _presetBindersProp = serializedObject.FindProperty("_presetBinders");

            var instanceId = _view.GetInstanceID();
            _selectedBinderIndexKey = $"{instanceId}_selected_binder_index";

            LoadSelectedBinderIndex();
            SetSelectedBinderIndex(_selectedBinderIndex);

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
            EditorApplication.update -= InitInspector;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_view == false)
            {
                return;
            }

            var ev = Event.current;
            var pressedKey = ev.keyCode;
            var eventType = ev.type;

            if (ConsumeKeyPress(eventType, pressedKey))
            {
                return;
            }

            InitStyles();
            RefreshSelectedBinderIndex();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                DrawBindersPanel(pressedKey);
                GUILayout.Space(4f);
                DrawDetailsPanel();
            }
            EditorGUILayout.EndHorizontal();

            if (ValidateSelectedSubtitleIndex())
            {
                EditorGUI.FocusTextInControl(_subtitleControlName);
            }
        }

        private bool ConsumeKeyPress(EventType eventType, KeyCode pressedKey)
        {
            var isKeyUp = eventType == EventType.KeyUp;

            switch (pressedKey)
            {
                case KeyCode.F2:
                {
                    if (ValidateSelectedBinderIndex())
                    {
                        SetSelectedSubtitleIndex(_selectedBinderIndex);
                    }
                    return false;
                }

                case KeyCode.UpArrow:
                {
                    if (isKeyUp && ValidateSelectedBinderIndex())
                    {
                        var prevIndex = _selectedBinderIndex.Value - 1;
                        SetSelectedBinderIndex(Mathf.Max(prevIndex, 0));
                        Event.current.Use();
                        return true;
                    }

                    return false;
                }

                case KeyCode.DownArrow:
                {
                    if (isKeyUp && ValidateSelectedBinderIndex())
                    {
                        var nextIndex = _selectedBinderIndex.Value + 1;
                        var lastIndex = _presetBindersProp.arraySize - 1;
                        SetSelectedBinderIndex(Mathf.Min(nextIndex, lastIndex));
                        Event.current.Use();
                        return true;
                    }

                    return false;
                }

                case KeyCode.Return:
                {
                    if (isKeyUp && ValidateSelectedSubtitleIndex())
                    {
                        ApplyBinderSubtitle();
                        Event.current.Use();
                        return true;
                    }

                    return false;
                }

                case KeyCode.Escape:
                {
                    if (isKeyUp)
                    {
                        SetSelectedSubtitleIndex(null);
                        Event.current.Use();
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        private void LoadSelectedBinderIndex()
        {
            if (_presetBindersProp.arraySize < 1)
            {
                _selectedBinderIndex = null;
                return;
            }

            var str = EditorUserSettings.GetConfigValue(_selectedBinderIndexKey);

            if (int.TryParse(str, out var value))
            {
                _selectedBinderIndex = value;
            }
            else if (_presetBindersProp.arraySize > 0)
            {
                _selectedBinderIndex = 0;
            }
            else
            {
                _selectedBinderIndex = null;
            }
        }

        private void SetSelectedBinderIndex(int? value)
        {
            _selectedBinderIndex = value;

            if (value.HasValue)
            {
                EditorUserSettings.SetConfigValue(_selectedBinderIndexKey, value.Value.ToString());
            }
            else
            {
                EditorUserSettings.SetConfigValue(_selectedBinderIndexKey, string.Empty);
            }
        }

        private void SetSelectedSubtitleIndex(int? value)
        {
            if (value.HasValue && (uint)value.Value < (uint)_presetBindersProp.arraySize)
            {
                _selectedSubtitleIndex = value;
                _subtitleControlName = $"sub_name_{value.Value}";
                _selectedSubtitleProp = _presetBindersProp.GetArrayElementAtIndex(value.Value).FindPropertyRelative("_subtitle");
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

        private bool ValidateSelectedBinderIndex()
        {
            return _selectedBinderIndex.HasValue
                && (uint)_selectedBinderIndex.Value < (uint)_presetBindersProp.arraySize;
        }

        private bool ValidateSelectedSubtitleIndex()
        {
            return _selectedSubtitleIndex.HasValue
                && (uint)_selectedSubtitleIndex.Value < (uint)_presetBindersProp.arraySize;
        }

        private void RefreshSelectedBinderIndex()
        {
            if (_presetBindersProp.arraySize > 0 && _selectedBinderIndex.HasValue == false)
            {
                _selectedBinderIndex = 0;
            }
            else if (_presetBindersProp.arraySize < 1 && _selectedBinderIndex.HasValue)
            {
                _selectedBinderIndex = null;
            }
        }

        private static void InitInspector()
        {
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
                var currentNode = rootNode;
                var hasDirectory = string.IsNullOrWhiteSpace(directory) == false;

                currentNode = hasDirectory ? currentNode.GetOrCreateNode(directory) : currentNode;
                currentNode = currentNode.CreateNode(label);

                currentNode.content = hasDirectory ? new($"{directory}/{label}") : new(label);
                currentNode.func2 = BinderMenu_AddBinder;
                currentNode.userData = new BinderMenuItem(binderType, targetType, s_bindersPropRef);
                currentNode.on = false;

                typeMap.TryAdd(binderType, targetType);
            }
        }

        private static void InitBindingMenuMap()
        {
            var map = s_bindingMenuMap;

            if (map.Count > 0)
            {
                return;
            }

            var defs = TypeCache.GetTypesDerivedFrom<MonoBinding>()
                .Where(ValidateType)
                .Select(ToDefinition<MonoBinding>)
                .Where(static x => x.IsValid);

            foreach (var (bindingType, targetType, label, _) in defs)
            {
                if (map.TryGetValue(targetType, out var menu) == false)
                {
                    map[targetType] = menu = new(new MenuItemNode(), "Bindings");
                }

                var currentNode = menu.rootNode;
                currentNode = currentNode.CreateNode(label);

                currentNode.content = new(label);
                currentNode.func2 = BindingMenu_AddBinding;
                currentNode.userData = new BindingMenuItem(bindingType, targetType, s_binderPropRef);
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
            var label = attrib?.Label ?? "";
            var directory = attrib?.Directory ?? "";

            return new(type, targetType, label, directory);
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

        [WrapRecord]
        private readonly partial record struct DestType(Type _);

        [WrapRecord]
        private readonly partial record struct SourceType(Type _);

        private readonly partial record struct BinderDefinition(
              Type BinderType
            , Type TargetType
            , string Label
            , string Directory
        )
        {
            public readonly bool IsValid => BinderType != null && TargetType != null;
        }

        private sealed class BindersPropRef
        {
            public SerializedProperty Prop { get; set; }

            public MonoViewInspector Inspector { get; set; }

            public void Reset()
            {
                Prop = null;
                Inspector = null;
            }
        }

        private sealed class BinderPropRef
        {
            public SerializedProperty Prop { get; set; }

            public MonoViewInspector Inspector { get; set; }

            public void Reset()
            {
                Prop = null;
                Inspector = null;
            }
        }

        private sealed partial record class BinderMenuItem(
              Type BinderType
            , Type TargetType
            , BindersPropRef Instance
        );

        private sealed partial record class BindingMenuItem(
              Type BindingType
            , Type TargetType
            , BinderPropRef Instance
        );
    }
}

#endif
