using System;

namespace EncosyTower.Entities
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGenForAssemblyAttribute : Attribute { }
}
