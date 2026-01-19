#if UNITY_EDITOR

using EncosyTower.EnumExtensions;
using UnityEditor;

namespace EncosyTower.Editor
{
    [EnumExtensionsFor(typeof(BuildTarget))]
    public static partial class BuildTargetExtensions { }
}

#endif
