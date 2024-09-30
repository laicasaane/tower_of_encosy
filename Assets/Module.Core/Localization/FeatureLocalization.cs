#if UNITY_EDITOR

using Module.Core.Editor.ProjectSetup;

namespace Module.Core.Localization.Editor
{
    [Feature("2. Localization")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.2")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.localization", "1.5.2")]
    internal readonly struct FeatureLocalization { }
}

#endif
