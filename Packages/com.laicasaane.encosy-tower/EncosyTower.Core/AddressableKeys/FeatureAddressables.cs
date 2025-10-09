#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.AddressableKeys
{
    [Feature("1. EncosyTower: Addressables")]

#if UNITY_6000_0_OR_NEWER
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "2.7.4")]
#else
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.3")]
#endif

    [RequiresPackage(PackageRegistry.OpenUpm, "com.littlebigfun.addressable-importer", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "jp.co.cyberagent.smartaddresser", isOptional: true)]
    internal readonly struct FeatureAddressables { }
}

#endif
