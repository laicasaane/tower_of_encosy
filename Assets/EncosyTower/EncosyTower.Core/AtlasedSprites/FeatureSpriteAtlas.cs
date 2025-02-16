#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.AtlasedSprites
{
    [Feature("3. EncosyTower: SpriteAtlas")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "2.3.16", isOptional: true)]
    internal readonly struct FeatureSpriteAtlas { }
}

#endif
