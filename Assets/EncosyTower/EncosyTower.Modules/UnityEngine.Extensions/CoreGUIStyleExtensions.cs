using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace EncosyTower.Modules
{
    public static class CoreGUIStyleExtensions
    {
        public static GUIStyle WithNormalBackground([NotNull] this GUIStyle style, Color color)
        {
            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, color);
            background.Apply();

            style.normal.background = background;

            return style;
        }

    }
}
