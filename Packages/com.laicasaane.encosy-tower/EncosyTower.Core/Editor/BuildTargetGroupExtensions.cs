#if UNITY_EDITOR

using EncosyTower.EnumExtensions;
using UnityEditor;

namespace EncosyTower.Editor
{
    [EnumExtensionsFor(typeof(BuildTargetGroup))]
    public static partial class BuildTargetGroupExtensions { }
}

#endif
