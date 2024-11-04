#if UNITY_EDITOR

using EncosyTower.Modules.Editor.ProjectSetup;

namespace EncosyTower.Modules.Editor.Data
{
    [Feature("5. EncosyTower: Data")]
    [RequiresPackage(PackageRegistry.Unity, "com.cathei.bakingsheet", "4.1.3")]
    internal readonly struct FeatureData { }
}

#endif
