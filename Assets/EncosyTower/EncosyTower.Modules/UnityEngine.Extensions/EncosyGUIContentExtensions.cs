using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules
{
    public static class EncosyGUIContentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GUIContent WithText(this GUIContent self, string text)
        {
            self.text = text;
            return self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GUIContent WithTooltip(this GUIContent self, string tooltip)
        {
            self.tooltip = tooltip;
            return self;
        }
    }
}
