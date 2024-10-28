using System.Runtime.CompilerServices;
using EncosyTower.Modules.TypeWrap;

namespace EncosyTower.Modules
{
    [WrapRecord]
    public readonly partial record struct ResourceKey(AssetKey Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey(AssetKey.Serializable value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey.Serializable(ResourceKey value)
            => value.Value;
    }
}