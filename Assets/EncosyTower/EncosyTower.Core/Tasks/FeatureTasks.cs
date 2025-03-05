#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.Tasks
{
    [Feature("6. EncosyTower: Page Flows")]

#if UNITY_6000_0_OR_NEWER
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
#else
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask")]
#endif

    internal readonly struct FeatureTasks { }
}

#endif
