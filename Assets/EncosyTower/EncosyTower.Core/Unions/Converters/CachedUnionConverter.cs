using System.Runtime.CompilerServices;

namespace EncosyTower.Unions.Converters
{
    public sealed class CachedUnionConverter<T> : IUnionConverter<T>
    {
        public static readonly CachedUnionConverter<T> Default = new();

        private IUnionConverter<T> _converter;

        private CachedUnionConverter() { }

        public IUnionConverter<T> Converter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _converter ??= UnionConverter.GetConverter<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union)
        {
            if (((Union<T>)union).TryGetValue(out var value))
            {
                return value.ToString();
            }

            return Converter.ToString(union);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(T value)
            => Converter.ToUnion(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<T> ToUnionT(T value)
            => Converter.ToUnionT(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(in Union union)
            => Converter.GetValue(union);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out T result)
            => Converter.TryGetValue(union, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref T dest)
            => Converter.TrySetValueTo(union, ref dest);
    }
}
