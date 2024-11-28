using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Unions.Converters
{
    public sealed class CachedUnionConverter<T> : IUnionConverter<T>
    {
        public static readonly CachedUnionConverter<T> Default = new();

        private IUnionConverter<T> _converter;

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

            return this.Converter.ToString(union);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(T value)
            => this.Converter.ToUnion(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<T> ToUnionT(T value)
            => this.Converter.ToUnionT(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(in Union union)
            => this.Converter.GetValue(union);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out T result)
            => this.Converter.TryGetValue(union, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref T dest)
            => this.Converter.TrySetValueTo(union, ref dest);
    }
}
