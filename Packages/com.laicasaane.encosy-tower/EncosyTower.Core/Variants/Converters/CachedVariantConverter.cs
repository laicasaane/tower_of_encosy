using System.Runtime.CompilerServices;

namespace EncosyTower.Variants.Converters
{
    public sealed class CachedVariantConverter<T> : IVariantConverter<T>
    {
        public static readonly CachedVariantConverter<T> Default = new();

        private IVariantConverter<T> _converter;

        private CachedVariantConverter() { }

        public IVariantConverter<T> Converter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _converter ??= VariantConverter.GetConverter<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Variant variant)
        {
            if (((Variant<T>)variant).TryGetValue(out var value))
            {
                return value.ToString();
            }

            return Converter.ToString(variant);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant ToVariant(T value)
            => Converter.ToVariant(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant<T> ToVariantT(T value)
            => Converter.ToVariantT(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(in Variant variant)
            => Converter.GetValue(variant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Variant variant, out T result)
            => Converter.TryGetValue(variant, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Variant variant, ref T dest)
            => Converter.TrySetValueTo(variant, ref dest);
    }
}
