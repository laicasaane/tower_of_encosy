#if UNITY_EDITOR

using Module.Core.Editor.ProjectSetup;

namespace Module.Core.Entities.Editor
{
    [Feature("4. Entities")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.entities", "1.2.4")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.entities.graphics", "1.2.4", isOptional: true)]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.latios.latiosframework", "0.10.7", isOptional: true)]
    internal readonly struct FeatureEntities { }
}

#endif
