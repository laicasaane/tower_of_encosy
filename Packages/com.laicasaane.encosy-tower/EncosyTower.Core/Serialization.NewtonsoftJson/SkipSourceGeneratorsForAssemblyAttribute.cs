#if UNITY_NEWTONSOFT_JSON

using System;

namespace EncosyTower.Serialization.NewtonsoftJson
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGeneratorsForAssemblyAttribute : Attribute { }
}

#endif
