#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Data.Authoring
{
    [Feature("5. EncosyTower: Data Authoring")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.editorcoroutines")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.nuget.newtonsoft-json", "3.2.1")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cathei.bakingsheet", "4.1.3")]

#if !UNITY_6000_0_OR_NEWER
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask")]
#else
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
#endif
    internal readonly struct FeatureData { }
}

#endif
