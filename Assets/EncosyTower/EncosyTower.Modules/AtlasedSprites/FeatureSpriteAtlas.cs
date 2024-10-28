#if UNITY_EDITOR

using EncosyTower.Modules.Editor.ProjectSetup;

namespace EncosyTower.Modules.AtlasedSprites.Editor
{
    [Feature("3. SpriteAtlas")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.2", isOptional: true)]
    internal readonly struct FeatureSpriteAtlas { }
}

#endif
