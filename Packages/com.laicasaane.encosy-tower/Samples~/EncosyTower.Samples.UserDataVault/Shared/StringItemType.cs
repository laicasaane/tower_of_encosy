using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.TypeWraps;

namespace EncosyTower.Samples.UserDataVault.Shared;

[WrapType(typeof(string), "_value", ExcludeConverter = true)]
public readonly partial struct StringItemType
{
    private StringEnum<ItemType, Converter> Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ItemType ToEnum()
    {
        return Value.ToEnum();
    }

    public readonly struct Converter : IStringEnumConverter<ItemType>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Transform(ItemType from)
            => from.ToStringFast();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemType Transform(string from)
            => ItemTypeExtensions.TryParse(from, out var result, false, true)
            ? result : ItemType.None;
    }
}
