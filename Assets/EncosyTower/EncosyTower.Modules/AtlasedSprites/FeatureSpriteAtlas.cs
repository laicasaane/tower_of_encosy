#if UNITY_EDITOR

using EncosyTower.Modules.Editor.ProjectSetup;

namespace EncosyTower.Modules.Editor.AtlasedSprites
{
    [Feature("3. EncosyTower: SpriteAtlas")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.2", isOptional: true)]
    internal readonly struct FeatureSpriteAtlas { }
}

#endif
