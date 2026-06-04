#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.Search
{
    [Feature("6. EncosyTower: Search")]
    [RequiresPackage(PackageRegistry.OpenUpm, "org.nuget.raffinert.fuzzysharp", "5.0.2")]
    [RequiresPackage(PackageRegistry.OpenUpm, "org.nuget.microsoft.bcl.memory", "9.0.14")]
    [RequiresPackage(PackageRegistry.OpenUpm, "org.nuget.indexrange", "1.1.1")]
    internal readonly struct FeatureSearch { }
}

#endif
