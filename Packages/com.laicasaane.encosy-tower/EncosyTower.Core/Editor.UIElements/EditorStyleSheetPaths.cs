#if UNITY_EDITOR

using EncosyTower.Core;

namespace EncosyTower.Editor.UIElements
{
    [ApiForEditor]
    public static class EditorStyleSheetPaths
    {
        [ApiForEditor]
        public const string ROOT = "Packages/com.laicasaane.encosy-tower";

        [ApiForEditor]
        public const string PROJECT_SETTINGS_STYLE_SHEET = "StyleSheets/ProjectSettings/ProjectSettingsCommon.uss";
    }
}

#endif
