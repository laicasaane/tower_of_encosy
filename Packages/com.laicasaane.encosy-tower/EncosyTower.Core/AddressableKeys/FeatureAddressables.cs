#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.AddressableKeys
{
    [Feature("1. EncosyTower: Addressables")]

    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "2.8.1")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.littlebigfun.addressable-importer", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "jp.co.cyberagent.smartaddresser", isOptional: true)]
    internal readonly struct FeatureAddressables { }
}

#endif
