using System;

namespace EncosyTower.Modules.Data
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGenForAssemblyAttribute : Attribute { }
}
