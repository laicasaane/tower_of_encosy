#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Common
{
    [Feature("0. EncosyTower: Common")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.burst", "1.8.29")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.collections", "2.6.7")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.mathematics", "1.3.3")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.serialization", "3.1.5")]

    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.unity-codegen", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
    internal readonly struct FeatureCommon { }
}

#endif
