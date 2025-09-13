using System.Runtime.CompilerServices;

namespace EncosyTower.Variants
{
    public static class VariantAPI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant AsVariant<T>(this T value)
            => Variant<T>.ToVariant(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant<T> AsVariantT<T>(this T value)
            => Variant<T>.ToVariantT(value);
    }
}
