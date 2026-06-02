using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Conversion;
using EncosyTower.EnumExtensions;
using EncosyTower.TypeWraps;
using EncosyTower.UnionIds;
using Unity.Collections;

namespace EncosyTower.Samples.UserDataVault.Shared;

[WrapRecord(ExcludeConverter = true)]
[TypeAsEnumMemberForTemplate(typeof(ItemType_EnumTemplate), 300)]
[KindForUnionId(typeof(ItemId), order: 300, "Weapon", "Weapon"
    , toStringMethods: ToStringMethods.All
    , tryParseSpan: TryParseMethodType.Instance
)]
public readonly partial record struct WeaponId(ushort Id)
    : IToFixedString<FixedString32Bytes>
    , IToDisplayFixedString<FixedString32Bytes>
    , IToDisplayString
    , ITryParseSpan<WeaponId>
    , ITryParse<WeaponId>
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

    public bool TryParse(ReadOnlySpan<char> str, out WeaponId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
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
    public bool TryParse(string str, out WeaponId result, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
}
