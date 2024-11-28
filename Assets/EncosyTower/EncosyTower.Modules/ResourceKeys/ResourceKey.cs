using System.Runtime.CompilerServices;
using EncosyTower.Modules.TypeWrap;

namespace EncosyTower.Modules
{
    [WrapRecord]
    public readonly partial record struct ResourceKey(AssetKey Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(ResourceKey value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey(AssetKey.Serializable value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey.Serializable(ResourceKey value)
            => value.Value;
    }

    [WrapRecord]
    public readonly partial record struct ResourceKey<T>(AssetKey<T> Value)
    where T : UnityEngine.Object
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(ResourceKey<T> value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(ResourceKey value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ResourceKey(ResourceKey<T> value)
            => new((string)value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(AssetKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AssetKey(ResourceKey<T> value)
            => (AssetKey)value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(AssetKey.Serializable value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AssetKey.Serializable(ResourceKey<T> value)
            => (AssetKey)value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(AssetKey.Serializable<T> value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey.Serializable<T>(ResourceKey<T> value)
            => value.Value;
    }
}
