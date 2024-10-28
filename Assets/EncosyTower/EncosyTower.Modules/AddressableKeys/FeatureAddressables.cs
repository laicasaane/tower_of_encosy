#if UNITY_EDITOR

using EncosyTower.Modules.Editor.ProjectSetup;

namespace EncosyTower.Modules.AddressableKeys.Editor
{
    [Feature("1. Addressables")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.addressables", "1.22.2")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.littlebigfun.addressable-importer", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "jp.co.cyberagent.smartaddresser", isOptional: true)]
    internal readonly struct FeatureAddressables { }
}

#endif
