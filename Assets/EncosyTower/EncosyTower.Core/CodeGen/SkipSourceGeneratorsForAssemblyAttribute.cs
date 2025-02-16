using System;

namespace EncosyTower.CodeGen
{
    /// <summary>
    /// Skips all EncosyTower source generators for the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGeneratorsForAssemblyAttribute : Attribute { }
}
