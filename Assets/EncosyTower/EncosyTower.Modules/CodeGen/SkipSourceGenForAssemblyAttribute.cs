using System;

namespace EncosyTower.Modules
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGenForAssemblyAttribute : Attribute { }
}
