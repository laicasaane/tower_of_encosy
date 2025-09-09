#if UNITY_EDITOR

namespace EncosyTower.Editor.UIElements
{
    public static class EditorStyleSheetPaths
    {
        public const string ROOT = "Packages/com.laicasaane.encosy-tower";

#if UNITY_6000_0_OR_NEWER
        public const string PROJECT_SETTINGS_STYLE_SHEET = "StyleSheets/ProjectSettings/ProjectSettingsCommon.uss";
#else
        private const string CORE_ROOT = $"{ROOT}/EncosyTower.Core/Editor.UIElements";
        public const string PROJECT_SETTINGS_STYLE_SHEET = $"{CORE_ROOT}/Common_2022_3.uss";
#endif

    }
}

#endif
