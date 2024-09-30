using System.Runtime.CompilerServices;
using Module.Core.Unions.Converters;

namespace Module.Core.Unions
{
    public readonly struct Union<T> : IUnion<T>
    {
        public static readonly TypeId TypeId = TypeId.Get<T>();
        private static readonly CachedUnionConverter<T> s_cachedUnionConverter = new();

        public readonly Union Value;

        public Union(in Union union)
        {
            Value = new Union(union.Base, union.TypeKind, TypeId);
        }

        public IUnionConverter<T> Converter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_cachedUnionConverter.Converter;
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
            => new Union<T>(union);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Union(in Union<T> union)
            => union.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IUnionConverter<T> GetConverter()
            => s_cachedUnionConverter.Converter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Union ToUnion(T value)
            => GetConverter().ToUnion(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Union<T> ToUnionT(T value)
            => GetConverter().ToUnionT(value);
    }
}
