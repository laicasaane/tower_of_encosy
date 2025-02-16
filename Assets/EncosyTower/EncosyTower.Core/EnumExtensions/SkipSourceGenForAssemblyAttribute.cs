using System;

namespace EncosyTower.EnumExtensions
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGenForAssemblyAttribute : Attribute { }
}
