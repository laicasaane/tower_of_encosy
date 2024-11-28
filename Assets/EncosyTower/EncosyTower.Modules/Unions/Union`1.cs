using System.Runtime.CompilerServices;
using EncosyTower.Modules.Unions.Converters;

namespace EncosyTower.Modules.Unions
{
    public readonly struct Union<T> : IUnion<T>
    {
        public readonly Union Value;

        public Union(in Union union)
        {
            Value = new Union(union.Base, union.TypeKind, TypeId);
        }

        public static TypeId TypeId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (TypeId)TypeId<T>.Value;
        }

        public IUnionConverter<T> Converter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CachedUnionConverter<T>.Default.Converter;
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
        public static implicit operator Union<T>(in Union union)
            => new(union);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Union(in Union<T> union)
            => union.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IUnionConverter<T> GetConverter()
            => CachedUnionConverter<T>.Default.Converter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Union ToUnion(T value)
            => GetConverter().ToUnion(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Union<T> ToUnionT(T value)
            => GetConverter().ToUnionT(value);
    }
}
