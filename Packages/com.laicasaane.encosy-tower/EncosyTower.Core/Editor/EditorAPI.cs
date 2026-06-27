#if UNITY_EDITOR

using System.IO;
using EncosyTower.Core;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    public static class EditorAPI
    {
        [ApiForEditor]
        public static string ProjectPath => Path.Combine(Application.dataPath, "..");

        [ApiForEditor]
        public static bool IsDark => EditorGUIUtility.isProSkin;

        [ApiForEditor]
        public static Color GetColor(in Color dark, in Color light)
        {
            return IsDark ? dark : light;
        }

        [ApiForEditor]
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

        [ApiForEditor]
        public static GUIContent GetIcon(string dark, string light)
        {
            return IsDark
                ? EditorGUIUtility.IconContent(dark)
                : EditorGUIUtility.IconContent(light);

        }
    }
}

#endif
