#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.Entities
{
    [Feature("4. EncosyTower: Entities")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.entities", "1.3.9")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.entities.graphics", "1.4.6", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.latios.latiosframework", "0.12.0", isOptional: true)]
    internal readonly struct FeatureEntities { }
}

#endif
