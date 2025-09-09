#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Common
{
    [Feature("0. EncosyTower: Common")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.burst", "1.8.23")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.collections", "2.5.7")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.mathematics", "1.3.2")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.nuget.newtonsoft-json", "3.2.1")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.serialization", "3.1.2")]

#if UNITY_2023_1_OR_NEWER
#else
    [RequiresPackage(PackageRegistry.Unity, "com.unity.textmeshpro", "3.2.0-pre.10")]
#endif

    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.unity-codegen")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.bgtools.playerprefseditor", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.browar.editor-toolbox", isOptional: true)]

#if UNITY_6000_0_OR_NEWER
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
#else
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask")]
#endif

    [RequiresPackage(PackageRegistry.OpenUpm, "com.needle.console", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.xarbrough.renamer", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.yasirkula.inspectplus", isOptional: true)]
    internal readonly struct FeatureCommon { }
}

#endif
