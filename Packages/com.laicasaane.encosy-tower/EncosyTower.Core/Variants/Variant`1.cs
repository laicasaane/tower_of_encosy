using System.Runtime.CompilerServices;
using EncosyTower.Types;
using EncosyTower.Variants.Converters;

namespace EncosyTower.Variants
{
    public readonly struct Variant<T> : IVariant<T>
    {
        public readonly Variant Value;

        public Variant(in Variant variant)
        {
            Value = new Variant(variant.Base, variant.TypeKind, TypeId);
        }

        public static TypeId TypeId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (TypeId)Type<T>.Id;
        }

        public IVariantConverter<T> Converter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CachedVariantConverter<T>.Default.Converter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue() => Converter.GetValue(Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out T result) => Converter.TryGetValue(Value, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(ref T dest) => Converter.TrySetValueTo(Value, ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Converter.ToString(Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Variant<T>(in Variant variant)
            => new(variant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Variant(in Variant<T> variant)
            => variant.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IVariantConverter<T> GetConverter()
            => CachedVariantConverter<T>.Default.Converter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant ToVariant(T value)
            => GetConverter().ToVariant(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant<T> ToVariantT(T value)
            => GetConverter().ToVariantT(value);
    }
}
