#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Databases.Authoring
{
    [Feature("5. EncosyTower: Databases Authoring")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.editorcoroutines")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.laicasaane.bakingsheet")]

#if !UNITY_6000_0_OR_NEWER
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask")]
#else
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
#endif
    internal readonly struct FeatureData { }
}

#endif
