using System;

namespace EncosyTower.Modules
{
    /// <summary>
    /// Skips all EncosyTower source generators for the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGenForAssemblyAttribute : Attribute { }

    /// <summary>
    /// Allows specific EncosyTower source generators for the assembly.
    /// </summary>
    /// <remarks>
    /// This attribute must be used in conjunction with <see cref="SkipSourceGenForAssemblyAttribute"/>
    /// to allow only specific generators while skipping all others.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AllowSourceGeneratorsForAssemblyAttribute : Attribute
    {
        /// <summary>
        /// Namespaces of the allowed source generators.
        /// </summary>
        public string[] Namespaces { get; }

        /// <summary>
        /// Specifies the namespaces of EncosyTower source generators that are allowed for the assembly.
        /// </summary>
        /// <param name="namespaces">An array of namespaces of the allowed source generators.</param>
        /// <remarks>
        /// This attribute must be used in conjunction with <see cref="SkipSourceGenForAssemblyAttribute"/>
        /// to allow only specific generators while skipping all others.
        /// </remarks>
        public AllowSourceGeneratorsForAssemblyAttribute(params string[] namespaces)
        {
            Namespaces = namespaces;
        }
    }
}
