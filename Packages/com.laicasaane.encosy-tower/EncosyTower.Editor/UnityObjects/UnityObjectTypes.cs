#if UNITY_EDITOR

using EncosyTower.ConfigKeys;
using EncosyTower.Editor.ConfigKeys;
using EncosyTower.Logging;
using UnityEditor;

namespace EncosyTower.Editor.UnityObjects
{
    public static class UnityObjectTypes
    {
        private const string GROUP = nameof(UnityObjectTypes);

        private const string MENU_PATH = "Encosy Tower/Utilities/Unity Objects";
        private const string TYPE_CONFIG_MENU_PATH = $"{MENU_PATH}/Type Config";
        private const string CONTAINS_UNITY = $"{TYPE_CONFIG_MENU_PATH}/Contains 'Unity'";
        private const string CONTAINS_EDITOR = $"{TYPE_CONFIG_MENU_PATH}/Contains 'Editor'";
        private const string CONTAINS_AUTHORING = $"{TYPE_CONFIG_MENU_PATH}/Contains 'Authoring'";
        private const string CONTAINS_WINDOW = $"{TYPE_CONFIG_MENU_PATH}/Contains 'Window'";
        private const string CONTAINS_DRAWER = $"{TYPE_CONFIG_MENU_PATH}/Contains 'Drawer'";
        private const string CONTAINS_IMPORTER = $"{TYPE_CONFIG_MENU_PATH}/Contains 'Importer'";
        private const string IS_ABSTRACT = $"{TYPE_CONFIG_MENU_PATH}/Is Abstract?";
        private const string IS_UNBOUND_GENERIC = $"{TYPE_CONFIG_MENU_PATH}/Is Unbound Generic?";
        private const string IS_COMPONENT = $"{TYPE_CONFIG_MENU_PATH}/Is Component?";
        private const string IS_SCRIPTABLE_OBJECT = $"{TYPE_CONFIG_MENU_PATH}/Is ScriptableObject?";

        private static readonly ConfigKey<bool> s_containsUnity = $"{GROUP}-{nameof(ContainsUnity)}";
        private static readonly ConfigKey<bool> s_containsEditor = $"{GROUP}-{nameof(ContainsEditor)}";
        private static readonly ConfigKey<bool> s_containsAuthoring = $"{GROUP}-{nameof(ContainsAuthoring)}";
        private static readonly ConfigKey<bool> s_containsWindow = $"{GROUP}-{nameof(ContainsWindow)}";
        private static readonly ConfigKey<bool> s_containsDrawer = $"{GROUP}-{nameof(ContainsDrawer)}";
        private static readonly ConfigKey<bool> s_containsImporter = $"{GROUP}-{nameof(ContainsImporter)}";
        private static readonly ConfigKey<bool> s_isAbstract = $"{GROUP}-{nameof(IsAbstract)}";
        private static readonly ConfigKey<bool> s_isUnboundGeneric = $"{GROUP}-{nameof(IsUnboundGeneric)}";
        private static readonly ConfigKey<bool> s_isComponent = $"{GROUP}-{nameof(IsComponent)}";
        private static readonly ConfigKey<bool> s_isScriptableObject = $"{GROUP}-{nameof(IsScriptableObject)}";

        private static bool ContainsUnity
        {
            get => s_containsUnity.GetEditorUserSetting(false);
            set => s_containsUnity.SetEditorUserSetting(value);
        }

        private static bool ContainsEditor
        {
            get => s_containsEditor.GetEditorUserSetting(false);
            set => s_containsEditor.SetEditorUserSetting(value);
        }

        private static bool ContainsAuthoring
        {
            get => s_containsAuthoring.GetEditorUserSetting(false);
            set => s_containsAuthoring.SetEditorUserSetting(value);
        }

        private static bool ContainsWindow
        {
            get => s_containsWindow.GetEditorUserSetting(false);
            set => s_containsWindow.SetEditorUserSetting(value);
        }

        private static bool ContainsDrawer
        {
            get => s_containsDrawer.GetEditorUserSetting(false);
            set => s_containsDrawer.SetEditorUserSetting(value);
        }

        private static bool ContainsImporter
        {
            get => s_containsImporter.GetEditorUserSetting(false);
            set => s_containsImporter.SetEditorUserSetting(value);
        }

        private static bool IsAbstract
        {
            get => s_isAbstract.GetEditorUserSetting(false);
            set => s_isAbstract.SetEditorUserSetting(value);
        }

        private static bool IsUnboundGeneric
        {
            get => s_isUnboundGeneric.GetEditorUserSetting(false);
            set => s_isUnboundGeneric.SetEditorUserSetting(value);
        }

        private static bool IsComponent
        {
            get => s_isComponent.GetEditorUserSetting(false);
            set => s_isComponent.SetEditorUserSetting(value);
        }

        private static bool IsScriptableObject
        {
            get => s_isScriptableObject.GetEditorUserSetting(false);
            set => s_isScriptableObject.SetEditorUserSetting(value);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Menu.SetChecked(CONTAINS_UNITY, ContainsUnity);
            Menu.SetChecked(CONTAINS_EDITOR, ContainsEditor);
            Menu.SetChecked(CONTAINS_AUTHORING, ContainsAuthoring);
            Menu.SetChecked(CONTAINS_WINDOW, ContainsWindow);
            Menu.SetChecked(CONTAINS_DRAWER, ContainsDrawer);
            Menu.SetChecked(CONTAINS_IMPORTER, ContainsImporter);
            Menu.SetChecked(IS_ABSTRACT, IsAbstract);
            Menu.SetChecked(IS_UNBOUND_GENERIC, IsUnboundGeneric);
            Menu.SetChecked(IS_COMPONENT, IsComponent);
            Menu.SetChecked(IS_SCRIPTABLE_OBJECT, IsScriptableObject);
        }

        [MenuItem(CONTAINS_UNITY, priority = 85_85_78_00)]
        private static void ToggleContainsUnity()
        {
            var value = ContainsUnity = !ContainsUnity;
            Menu.SetChecked(CONTAINS_UNITY, value);
        }

        [MenuItem(CONTAINS_EDITOR, priority = 85_85_78_00)]
        private static void ToggleContainsEditor()
        {
            var value = ContainsEditor = !ContainsEditor;
            Menu.SetChecked(CONTAINS_EDITOR, value);
        }

        [MenuItem(CONTAINS_AUTHORING, priority = 85_85_78_00)]
        private static void ToggleContainsAuthoring()
        {
            var value = ContainsAuthoring = !ContainsAuthoring;
            Menu.SetChecked(CONTAINS_AUTHORING, value);
        }

        [MenuItem(CONTAINS_WINDOW, priority = 85_85_78_00)]
        private static void ToggleContainsWindow()
        {
            var value = ContainsWindow = !ContainsWindow;
            Menu.SetChecked(CONTAINS_WINDOW, value);
        }

        [MenuItem(CONTAINS_DRAWER, priority = 85_85_78_00)]
        private static void ToggleContainsDrawer()
        {
            var value = ContainsDrawer = !ContainsDrawer;
            Menu.SetChecked(CONTAINS_DRAWER, value);
        }

        [MenuItem(CONTAINS_IMPORTER, priority = 85_85_78_00)]
        private static void ToggleContainsImporter()
        {
            var value = ContainsImporter = !ContainsImporter;
            Menu.SetChecked(CONTAINS_IMPORTER, value);
        }

        [MenuItem(IS_ABSTRACT, priority = 85_85_78_00)]
        private static void ToggleAbstract()
        {
            var value = IsAbstract = !IsAbstract;
            Menu.SetChecked(IS_ABSTRACT, value);
        }

        [MenuItem(IS_UNBOUND_GENERIC, priority = 85_85_78_00)]
        private static void ToggleUnboundGeneric()
        {
            var value = IsUnboundGeneric = !IsUnboundGeneric;
            Menu.SetChecked(IS_UNBOUND_GENERIC, value);
        }

        [MenuItem(IS_COMPONENT, priority = 85_85_78_00)]
        private static void ToggleComponent()
        {
            var value = IsComponent = !IsComponent;
            Menu.SetChecked(IS_UNBOUND_GENERIC, value);
        }

        [MenuItem(IS_SCRIPTABLE_OBJECT, priority = 85_85_78_00)]
        private static void ToggleScriptableObject()
        {
            var value = IsScriptableObject = !IsScriptableObject;
            Menu.SetChecked(IS_UNBOUND_GENERIC, value);
        }

        [MenuItem($"{MENU_PATH}/Print Types", priority = 85_85_78_00)]
        private static void PrintTypes()
        {
            var types = TypeCache.GetTypesDerivedFrom<UnityEngine.Object>();
            var componentType = typeof(UnityEngine.Component);
            var scriptableType = typeof(UnityEngine.ScriptableObject);

            var containsUnity = ContainsUnity;
            var containsEditor = ContainsEditor;
            var containsAuthoring = ContainsAuthoring;
            var containsWindow = ContainsWindow;
            var containsDrawer = ContainsDrawer;
            var containsImporter = ContainsImporter;
            var isAbstract = IsAbstract;
            var isUnboundGeneric = IsUnboundGeneric;
            var isComponent = IsComponent;
            var isScriptableObject = IsScriptableObject;

            foreach (var type in types)
            {
                if ((isAbstract == false && type.IsAbstract)
                    || (isUnboundGeneric == false && (type.IsGenericType && type.ContainsGenericParameters))
                    || (isComponent == false && componentType.IsAssignableFrom(type))
                    || (isScriptableObject == false && scriptableType.IsAssignableFrom(type))
                )
                {
                    continue;
                }

                var name = type.FullName ?? string.Empty;

                if (IsIgnored(name, containsUnity, "Unity")
                    || IsIgnored(name, containsEditor, "Editor")
                    || IsIgnored(name, containsAuthoring, "Authoring")
                    || IsIgnored(name, containsWindow, "Window")
                    || IsIgnored(name, containsDrawer, "Drawer")
                    || IsIgnored(name, containsImporter, "Importer")
                )
                {
                    continue;
                }

                StaticDevLogger.LogInfoSlim(type);
            }

            static bool IsIgnored(string name, bool config, string word)
                => (config && name.Contains(word) == false)
                || (config == false && name.Contains(word));
        }
    }
}

#endif
