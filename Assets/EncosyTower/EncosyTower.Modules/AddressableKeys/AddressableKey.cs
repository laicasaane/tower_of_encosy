#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using EncosyTower.Modules.TypeWrap;

namespace EncosyTower.Modules.AddressableKeys
{
    [WrapRecord]
    public readonly partial record struct AddressableKey(AssetKey Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey(AssetKey.Serializable value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey.Serializable(AddressableKey value)
            => value.Value;

    }
}

#endif
