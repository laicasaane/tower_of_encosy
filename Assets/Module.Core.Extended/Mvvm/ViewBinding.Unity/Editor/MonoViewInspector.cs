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
        /// Does not include <see cref="Module.Core.Extended.Mvvm.Unity.Adapters.ScriptableAdapter"/>
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

        private GUIStyle _rootTabViewStyle;
        private GUIStyle _rootTabLabelStyle;
        private GUIStyle _toolbarLeftButtonStyle;
        private GUIStyle _toolbarMidButtonStyle;
        private GUIStyle _toolbarRightButtonStyle;
        private GUIStyle _toolbarMenuButtonStyle;
        private GUIStyle _noBinderStyle;
        private GUIStyle _binderButtonStyle;
        private GUIStyle _binderIndexLabelStyle;
        private GUIStyle _binderSelectedButtonStyle;
        private GUIStyle _bindersHeaderStyle;
        private GUIStyle _detailsHeaderStyle;
        private GUIStyle _removeButtonStyle;
        private GUIStyle _indexLabelStyle;
        private GUIStyle _headerLabelStyle;
        private GUIStyle _itemLabelStyle;
        private GUIContent _addLabel;
        private GUIContent _removeLabel;
        private GUIContent _menuLabel;
        private GUIContent _clearLabel;
        private GUIContent _iconWarning;
        private GUIContent _iconBinding;
        private GUIContent _bindersLabel;
        private GUIContent _propertyBindingLabel;
        private GUIContent _commandBindingLabel;
        private GUIContent[] _detailsTabLabels;
        private Color _headerColor;
        private Color _contentColor;
        private Color _selectedColor;
        private Color _menuColor;
        private Color _altContentColor;

        private void InitStyles()
        {
            if (_rootTabViewStyle != null)
            {
                return;
            }

            _rootTabViewStyle = new(EditorStyles.helpBox) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
            };

            _rootTabLabelStyle = new(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
            };

            _toolbarLeftButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(2, 0, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            _toolbarMidButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            _toolbarRightButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 1, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            _toolbarMenuButtonStyle = new(_toolbarRightButtonStyle) {
                padding = new(2, 2, 2, 2),
            };

            _noBinderStyle = new(EditorStyles.miniLabel) {
                padding = new(0, 0, 0, 0),
                margin = new(1, 0, 0, 0),
                fixedHeight = 0,
                fixedWidth = 0,
                stretchWidth = true,
                stretchHeight = true,
                fontSize = EditorStyles.miniLabel.fontSize,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
            };

            _binderButtonStyle = new(EditorStyles.toolbarButton) {
                padding = new(0, 0, 0, 0),
                margin = new(1, 0, 0, 0),
                fixedHeight = 0,
                fontSize = GUI.skin.button.fontSize,
            };

            _binderSelectedButtonStyle = new(_binderButtonStyle) {
                fontStyle = FontStyle.Bold,
                margin = new(1, 2, 0, 0),
            };

            _binderIndexLabelStyle = new(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleLeft,
            };

            {
                var style = _binderSelectedButtonStyle;

                style.normal.scaledBackgrounds
                    = style.onNormal.scaledBackgrounds
                    = style.active.scaledBackgrounds
                    = style.onActive.scaledBackgrounds
                    = new Texture2D[] { Texture2D.whiteTexture };

                style.normal.background
                    = style.onNormal.background
                    = style.active.background
                    = style.onActive.background
                    = Texture2D.whiteTexture;
            }

            _bindersHeaderStyle = new() {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
            };

            _detailsHeaderStyle = new() {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
            };

            _removeButtonStyle = new(EditorStyles.iconButton);

            _indexLabelStyle = new(EditorStyles.miniLabel) {
                padding = new(3, 0, 0, 0),
            };

            _headerLabelStyle = new(EditorStyles.boldLabel) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
            };

            _itemLabelStyle = new(EditorStyles.label) {
                stretchWidth = false,
            };

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_Toolbar Plus More")
                    : EditorGUIUtility.IconContent("Toolbar Plus More");

                _addLabel = new(icon.image, "Add");
            }

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_Toolbar Minus")
                    : EditorGUIUtility.IconContent("Toolbar Minus");

                _removeLabel = new(icon.image, "Remove Selected Item");
            }

            {
                var icon = EditorGUIUtility.IconContent("pane options@2x");
                _menuLabel = new(icon.image, "Menu");
                _menuColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            }

            _clearLabel = new("Clear");

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_console.warnicon")
                    : EditorGUIUtility.IconContent("console.warnicon");

                _iconWarning = new GUIContent(icon.image);
            }

            _iconBinding = EditorGUIUtility.isProSkin
                ? EditorGUIUtility.IconContent("d_BlendTree Icon")
                : EditorGUIUtility.IconContent("BlendTree Icon");

            {
                ColorUtility.TryParseHtmlString("#2C5D87", out var darkColor);
                ColorUtility.TryParseHtmlString("#3A72B0", out var lightColor);
                _selectedColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#474747", out var darkColor);
                ColorUtility.TryParseHtmlString("#D6D6D6", out var lightColor);
                _headerColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#383838", out var darkColor);
                ColorUtility.TryParseHtmlString("#C8C8C8", out var lightColor);
                _contentColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#404040", out var darkColor);
                ColorUtility.TryParseHtmlString("#ABABAB", out var lightColor);
                _altContentColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            _bindersLabel = new("Binders");
            _detailsTabLabels = new GUIContent[] { new("Bindings"), new("Targets"), };
            _propertyBindingLabel = new("Property");
            _commandBindingLabel = new("Command");
        }

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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_view == false)
            {
                return;
            }

            InitStyles();
            RefreshSelectedBinderIndex();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                DrawBindersPanel();
                GUILayout.Space(4f);
                DrawDetailsPanel();
            }
            EditorGUILayout.EndHorizontal();
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
