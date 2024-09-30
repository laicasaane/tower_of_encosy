using System.Runtime.CompilerServices;
using Module.Core.TypeWrap;

namespace Module.Core
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