#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.AtlasedSprites
{
    [Feature("3. EncosyTower: SpriteAtlas")]

#if UNITY_6000_0_OR_NEWER
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "2.7.4", isOptional: true)]
#else
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.3", isOptional: true)]
#endif
    internal readonly struct FeatureSpriteAtlas { }
}

#endif
