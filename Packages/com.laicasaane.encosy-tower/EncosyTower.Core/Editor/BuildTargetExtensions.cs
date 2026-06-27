#if UNITY_EDITOR

using EncosyTower.Core;
using EncosyTower.EnumExtensions;
using UnityEditor;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    [EnumExtensionsFor(typeof(BuildTarget))]
    public static partial class BuildTargetExtensions { }
}

#endif
