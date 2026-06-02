using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Conversion;
using EncosyTower.EnumExtensions;
using EncosyTower.TypeWraps;
using EncosyTower.UnionIds;
using Unity.Collections;

namespace EncosyTower.Samples.UserDataVault.Shared;

[UnionId(Size = UnionIdSize.UInt, KindSettings = UnionIdKindSettings.PreserveOrder, Separator = '+')]
[UnionIdKind(typeof(NoneType), 000, nameof(NoneType.None))]
[TypeAsEnumMemberForTemplate(typeof(ItemType_EnumTemplate), 400)]
[KindForUnionId(typeof(ItemId), order: 400, "Accessory", "Accessory"
    , toStringMethods: ToStringMethods.All
    , tryParseSpan: TryParseMethodType.Instance
)]
public readonly partial struct AccessoryId
{
    public AccessoryId(AccessoryType resourceType) : this()
    {
        if (resourceType.TryConvert(out var order, out var value))
        {
            Kind = (IdKind)order;
            IdUnsigned = (byte)value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AccessoryType(AccessoryId id)
    {
        var value = (ushort)id.IdUnsigned;
        var order = (ushort)id.Kind;
        return (AccessoryType)(value + order);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AccessoryTypeExtended(AccessoryId id)
        => (AccessoryType)id;
}

[WrapType(typeof(string), "_value", ExcludeConverter = true)]
public readonly partial struct StringAccessoryIdKind
{
    private StringEnum<AccessoryId.IdKind, Converter> Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AccessoryId.IdKind ToEnum()
    {
        return Value.ToEnum();
    }

    public readonly struct Converter : IStringEnumConverter<AccessoryId.IdKind>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Transform(AccessoryId.IdKind from)
            => from.ToStringFast();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AccessoryId.IdKind Transform(string from)
            => AccessoryId_IdKindExtensions.TryParse(from, out var result, false, true)
            ? result : AccessoryId.IdKind.None;
    }
}

[WrapRecord(ExcludeConverter = true)]
[TypeAsEnumMemberForTemplate(typeof(AccessoryType_EnumTemplate), 100)]
[KindForUnionId(typeof(AccessoryId), order: 100, "Ring", "Ring"
    , toStringMethods: ToStringMethods.All
    , tryParseSpan: TryParseMethodType.Instance
)]
public readonly partial record struct RingId(ushort Id)
    : IToFixedString<FixedString32Bytes>
    , IToDisplayFixedString<FixedString32Bytes>
    , IToDisplayString
    , ITryParseSpan<RingId>
    , ITryParse<RingId>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString()
        => Id.ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedString32Bytes ToDisplayFixedString()
        => ToFixedString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedString32Bytes ToFixedString()
        => Id.ToFixedString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ToDisplayFixedString<T>()
        where T : unmanaged, INativeList<byte>, IUTF8Bytes
    {
        T result = default;
        result.Append(ToDisplayFixedString());
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ToFixedString<T>()
        where T : unmanaged, INativeList<byte>, IUTF8Bytes
    {
        T result = default;
        result.Append(ToFixedString());
        return result;
    }

    public bool TryParse(ReadOnlySpan<char> str, out RingId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (ushort.TryParse(str, out var value))
        {
            result = value;
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string str, out RingId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
}

[WrapRecord(ExcludeConverter = true)]
[TypeAsEnumMemberForTemplate(typeof(AccessoryType_EnumTemplate), 200)]
[KindForUnionId(typeof(AccessoryId), order: 200, "Necklace", "Necklace"
    , toStringMethods: ToStringMethods.All
    , tryParseSpan: TryParseMethodType.Instance
)]
public readonly partial record struct NecklaceId(ushort Id)
    : IToFixedString<FixedString32Bytes>
    , IToDisplayFixedString<FixedString32Bytes>
    , IToDisplayString
    , ITryParseSpan<NecklaceId>
    , ITryParse<NecklaceId>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString()
        => Id.ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedString32Bytes ToDisplayFixedString()
        => ToFixedString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedString32Bytes ToFixedString()
        => Id.ToFixedString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ToDisplayFixedString<T>()
        where T : unmanaged, INativeList<byte>, IIndexable<byte>, IUTF8Bytes
                , IComparable<string>, IEquatable<string>, IComparable<T>, IEquatable<T>
    {
        T result = default;
        result.Append(ToDisplayFixedString());
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ToFixedString<T>()
        where T : unmanaged, INativeList<byte>, IIndexable<byte>, IUTF8Bytes
                , IComparable<string>, IEquatable<string>, IComparable<T>, IEquatable<T>
    {
        T result = default;
        result.Append(ToFixedString());
        return result;
    }

    public bool TryParse(ReadOnlySpan<char> str, out NecklaceId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (ushort.TryParse(str, out var value))
        {
            result = value;
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string str, out NecklaceId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
}


[WrapRecord(ExcludeConverter = true)]
[TypeAsEnumMemberForTemplate(typeof(AccessoryType_EnumTemplate), 300)]
[KindForUnionId(typeof(AccessoryId), order: 300, "Bracelet", "Bracelet"
    , toStringMethods: ToStringMethods.All
    , tryParseSpan: TryParseMethodType.Instance
)]
public readonly partial record struct BraceletId(ushort Id)
    : IToFixedString, IToFixedString<FixedString32Bytes>
    , IToDisplayFixedString, IToDisplayFixedString<FixedString32Bytes>
    , IToDisplayString
    , ITryParseSpan<BraceletId>
    , ITryParse<BraceletId>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString()
        => Id.ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedString32Bytes ToDisplayFixedString()
        => ToFixedString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedString32Bytes ToFixedString()
        => Id.ToFixedString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TFixedString ToDisplayFixedString<TFixedString>()
        where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        => ToDisplayFixedString().CastTo<TFixedString>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TFixedString ToFixedString<TFixedString>()
        where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        => ToFixedString().CastTo<TFixedString>();

    public bool TryParse(ReadOnlySpan<char> str, out BraceletId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (ushort.TryParse(str, out var value))
        {
            result = value;
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string str, out BraceletId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
}

