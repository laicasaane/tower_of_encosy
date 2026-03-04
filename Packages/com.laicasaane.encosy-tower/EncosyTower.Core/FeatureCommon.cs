#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Common
{
    [Feature("0. EncosyTower: Common")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.burst", "1.8.28")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.collections", "2.6.5")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.mathematics", "1.3.3")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.nuget.newtonsoft-json", "3.2.1")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.serialization", "3.1.5")]

    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.unity-codegen", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.bgtools.playerprefseditor", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.browar.editor-toolbox", isOptional: true)]

    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.needle.console", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.xarbrough.renamer", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.yasirkula.inspectplus", isOptional: true)]
    internal readonly struct FeatureCommon { }
}

#endif
