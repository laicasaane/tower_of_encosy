#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor
{
    public static class EditorAPI
    {
        public static string ProjectPath => Application.dataPath.Replace("/Assets", string.Empty);

        public static bool IsDark => EditorGUIUtility.isProSkin;

        public static Color GetColor(in Color dark, in Color light)
        {
            return IsDark ? dark : light;
        }

        public static Color GetColor(string hexDark, string hexLight)
        {
            if (IsDark)
            {
                ColorUtility.TryParseHtmlString(hexDark, out var color);
                return color;
            }
            else
            {
                ColorUtility.TryParseHtmlString(hexLight, out var color);
                return color;
            }
        }

        public static GUIContent GetIcon(string dark, string light)
        {
            return IsDark
                ? EditorGUIUtility.IconContent(dark)
                : EditorGUIUtility.IconContent(light);

        }
    }
}

#endif
