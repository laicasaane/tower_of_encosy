#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using EncosyTower.Modules.TypeWrap;

namespace EncosyTower.Modules.AddressableKeys
{
    [WrapRecord]
    public readonly partial record struct AddressableKey(AssetKey Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(AddressableKey value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey(AssetKey.Serializable value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey.Serializable(AddressableKey value)
            => value.Value;
    }

    [WrapRecord]
    public readonly partial record struct AddressableKey<T>(AssetKey<T> Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(AddressableKey<T> value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AddressableKey value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AddressableKey(AddressableKey<T> value)
            => new((string)value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AssetKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AssetKey(AddressableKey<T> value)
            => (AssetKey)value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AssetKey.Serializable value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AssetKey.Serializable(AddressableKey<T> value)
            => (AssetKey)value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AssetKey.Serializable<T> value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey.Serializable<T>(AddressableKey<T> value)
            => value.Value;
    }
}

#endif
