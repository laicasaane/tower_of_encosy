namespace EncosyTower.Variants
{
    /// <summary>
    /// Any struct implements this interface is eligible
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
    /// <typeparam name="T"></typeparam>
    public interface IVariant<T> { }
}
