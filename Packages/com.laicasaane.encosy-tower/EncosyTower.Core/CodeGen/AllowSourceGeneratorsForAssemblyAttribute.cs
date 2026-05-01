using System;

namespace EncosyTower.CodeGen
{
    /// <summary>
    /// Allows specific EncosyTower source generators for the assembly.
    /// </summary>
    /// <remarks>
    /// This attribute must be used in conjunction with <see cref="SkipSourceGeneratorsForAssemblyAttribute"/>
    /// to allow only specific generators while skipping all others.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AllowSourceGeneratorsForAssemblyAttribute : Attribute
    {
        /// <summary>
        /// Namespaces of the allowed source generators.
        /// </summary>
        public string[] Namespaces { get; }

        /// <inheritdoc cref="AllowSourceGeneratorsForAssemblyAttribute" />
        public AllowSourceGeneratorsForAssemblyAttribute(params string[] namespaces)
        {
            Namespaces = namespaces;
        }
    }
}
