#if UNITY_EDITOR

using EncosyTower.Core;
using EncosyTower.EnumExtensions;
using UnityEditor;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    [EnumExtensionsFor(typeof(BuildTargetGroup))]
    public static partial class BuildTargetGroupExtensions { }
}

#endif
