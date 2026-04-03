using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.TypeWraps;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UserDataVault.Shared;

[UnionId(Size = UnionIdSize.ULong, KindSettings = UnionIdKindSettings.PreserveOrder)]
[UnionIdKind(typeof(NoneType), 000, nameof(NoneType.None))]
public readonly partial struct ItemId
{
    public ItemId(ItemType resourceType) : this()
    {
        if (resourceType.TryConvert(out var order, out var value))
        {
            Kind = (IdKind)order;
            IdUnsigned = (byte)value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ItemType(ItemId id)
    {
        var value = (ushort)id.IdUnsigned;
        var order = (ushort)id.Kind;
        return (ItemType)(value + order);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ItemTypeExtended(ItemId id)
        => (ItemType)id;
}

[WrapType(typeof(string), "_value", ExcludeConverter = true)]
public readonly partial struct StringItemIdKind
{
    private StringEnum<ItemId.IdKind, Converter> Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ItemId.IdKind ToEnum()
    {
        return Value.ToEnum();
    }

    public readonly struct Converter : IStringEnumConverter<ItemId.IdKind>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Transform(ItemId.IdKind from)
            => from.ToStringFast();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemId.IdKind Transform(string from)
            => ItemId_IdKindExtensions.TryParse(from, out var result, false, true)
            ? result : ItemId.IdKind.None;
    }
}
