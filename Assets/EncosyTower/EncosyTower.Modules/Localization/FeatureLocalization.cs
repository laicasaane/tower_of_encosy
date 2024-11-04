#if UNITY_EDITOR

using EncosyTower.Modules.Editor.ProjectSetup;

namespace EncosyTower.Modules.Editor.Localization
{
    [Feature("2. EncosyTower: Localization")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.2")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.localization", "1.5.3")]
    internal readonly struct FeatureLocalization { }
}

#endif
