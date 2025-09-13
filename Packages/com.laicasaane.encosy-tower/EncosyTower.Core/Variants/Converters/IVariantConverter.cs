namespace EncosyTower.Variants.Converters
{
    public interface IVariantConverter
    {
        string ToString(in Variant variant);
    }

    public interface IVariantConverter<T> : IVariantConverter
    {
        Variant ToVariant(T value);

        Variant<T> ToVariantT(T value);

        T GetValue(in Variant variant);

        bool TryGetValue(in Variant variant, out T result);

        bool TrySetValueTo(in Variant variant, ref T dest);
    }
}
