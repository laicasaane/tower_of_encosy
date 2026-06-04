#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Serialization.NewtonsoftJson
{
    [Feature("10. EncosyTower: Newtonsoft.Json")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.nuget.newtonsoft-json", "3.2.1")]
    internal readonly struct FeatureNewtonsoftJson { }
}

#endif
