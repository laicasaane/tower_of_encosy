#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.Search
{
    [Feature("6. EncosyTower: Search")]
    [RequiresPackage(PackageRegistry.OpenUpm, "org.nuget.raffinert.fuzzysharp")]
    internal readonly struct FeatureSearch { }
}

#endif
