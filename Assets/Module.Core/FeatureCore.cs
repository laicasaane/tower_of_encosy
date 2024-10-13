#if UNITY_EDITOR

using Module.Core.Editor.ProjectSetup;

namespace Module.Core
{
    [Feature("0. Core")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.burst", "1.8.17")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.collections", "2.4.3")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.logging", "1.2.4")]
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

    [RequiresPackage(PackageRegistry.GitUrl, "https://github.com/baba-s/Kogane.AssemblyDefinitionAssetHeaderGUI.git", isOptional: true)]
    [RequiresPackage(PackageRegistry.GitUrl, "https://github.com/baba-s/Kogane.CheckBoxWindow.git", isOptional: true)]
    [RequiresPackage(PackageRegistry.GitUrl, "https://github.com/baba-s/Kogane.JsonAssemblyDefinition.git", isOptional: true)]
    internal readonly struct FeatureCore { }
}

#endif
