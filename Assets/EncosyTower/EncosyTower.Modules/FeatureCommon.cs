#if UNITY_EDITOR

using EncosyTower.Modules.Editor.ProjectSetup;

namespace EncosyTower.Modules
{
    [Feature("0. EncosyTower: Common")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.burst", "1.8.18")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.collections", "2.5.1")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.logging", "1.3.4")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.mathematics", "1.3.2")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.nuget.newtonsoft-json", "3.2.1")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.serialization", "3.1.2")]

#if UNITY_2023_1_OR_NEWER
#else
    [RequiresPackage(PackageRegistry.Unity, "com.unity.textmeshpro", "3.2.0-pre.10")]
#endif

    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.unity-codegen")]

#if !UNITY_6000_0_OR_NEWER
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask")]
#else
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
#endif

    [RequiresPackage(PackageRegistry.OpenUpm, "com.draconware-dev.span-extensions.net.unity")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.gilzoide.easy-project-settings")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.needle.console", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.alchemy", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.debug-ui", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.lit-motion", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.annulusgames.tween-playables", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.browar.editor-toolbox", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.xarbrough.renamer", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.xarbrough.scriptable-object-creator", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.yasirkula.inspectplus", isOptional: true)]
    internal readonly struct FeatureCommon { }
}

#endif
