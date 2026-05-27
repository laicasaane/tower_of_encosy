using System;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Conversion;

namespace EncosyTower.EnumExtensions
{
    public interface IEnumUnderlying<out TUnderlyingValue> : IToUnderlyingValue<TUnderlyingValue>
        where TUnderlyingValue : unmanaged
    {
    }

    public interface IEnumExtensions<TEnum, out TUnderlyingValue> : IEnumUnderlying<TUnderlyingValue>
        , IHasLength
        , IToDisplayString
        , IToStringFast
        , IToDisplayStringFast
        , IToIndex
        , ISpanFormattable
        , IIsDefined
        , IIsNameDefined
        where TEnum : struct, Enum
        where TUnderlyingValue : unmanaged
    {
    }

    public interface IEnumExtensions<TEnumEx, TEnum, out TUnderlyingValue> : IEnumExtensions<TEnum, TUnderlyingValue>
        where TEnumEx : IEnumExtensions<TEnum, TUnderlyingValue>
        where TEnum : struct, Enum
        where TUnderlyingValue : unmanaged
    {
        TEnumEx Create(TEnum value);

        bool TryParse(string name, out TEnumEx value);

        bool TryParse(string name, out TEnumEx value, bool ignoreCase);

        bool TryParse(string name, out TEnumEx value, bool ignoreCase, bool allowMatchingMetadataAttribute);

        bool TryParse(ReadOnlySpan<char> name, out TEnumEx value);

        bool TryParse(ReadOnlySpan<char> name, out TEnumEx value, bool ignoreCase);

        bool TryParse(ReadOnlySpan<char> name, out TEnumEx value, bool ignoreCase, bool allowMatchingMetadataAttribute);
    }
}

