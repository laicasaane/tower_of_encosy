#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.Localization
{
    [Feature("2. EncosyTower: Localization")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "2.3.16")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.localization", "1.5.4")]
    internal readonly struct FeatureLocalization { }
}

#endif
