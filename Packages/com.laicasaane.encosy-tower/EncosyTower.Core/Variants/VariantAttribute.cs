using System;

namespace EncosyTower.Variants
{
    /// <summary>
    /// Any struct with this attribute is eligible
    /// to be stored in <see cref="Variant"/>.
    /// <br/>
    /// Necessary facility to enable the implicit compatibility
    /// between this struct and <see cref="Variant"/>
    /// will be provided by a source generator.
    /// </summary>
    /// <remarks>
    /// By default, the native size of the struct must be
    /// within 8 bytes. To support larger structs, follow
    /// the instruction in <see cref="VariantData"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class VariantAttribute : Attribute
    {
        public VariantAttribute(Type variantType)
        {
            Type = variantType;
        }

        public Type Type { get; }
    }
}
