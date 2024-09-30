#if UNITY_EDITOR

using Module.Core.Editor.ProjectSetup;

namespace Module.Core.AtlasedSprites.Editor
{
    [Feature("3. SpriteAtlas")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.2", isOptional: true)]
    internal readonly struct FeatureSpriteAtlas { }
}

#endif
