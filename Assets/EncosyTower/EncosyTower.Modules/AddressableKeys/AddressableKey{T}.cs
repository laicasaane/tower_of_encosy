#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using EncosyTower.Modules.TypeWrap;

namespace EncosyTower.Modules.AddressableKeys
{
    [WrapRecord]
    public readonly partial record struct AddressableKey<T>(AssetKey<T> Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AddressableKey value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AddressableKey(AddressableKey<T> value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AssetKey.Serializable<T> value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey.Serializable<T>(AddressableKey<T> value)
            => value.Value;
    }
}

#endif
