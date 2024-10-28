using System;

namespace EncosyTower.Modules.NewtonsoftAot
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGenForAssemblyAttribute : Attribute { }
}
