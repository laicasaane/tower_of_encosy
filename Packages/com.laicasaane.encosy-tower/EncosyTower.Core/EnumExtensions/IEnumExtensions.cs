using System;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Conversion;

namespace EncosyTower.EnumExtensions
{
    public interface IEnumUnderlying<TUnderlying>
        where TUnderlying : unmanaged, IComparable, IComparable<TUnderlying>
            , IConvertible, IEquatable<TUnderlying>, IFormattable
    {
        TUnderlying UnderlyingValue { get; }
    }

    public interface IEnumExtensions<TEnum, TUnderlying> : IEnumUnderlying<TUnderlying>
        , IHasLength
        , IToDisplayString
        , IToStringFast
        , IToDisplayStringFast
        , IToIndex
        , ISpanFormattable
        , IIsDefined
        , IIsNameDefined
        where TEnum : unmanaged, Enum
        where TUnderlying : unmanaged, IComparable, IComparable<TUnderlying>
            , IConvertible, IEquatable<TUnderlying>, IFormattable
    {
        TEnum Value { get; }
    }

    public interface IEnumExtensions<TEnumEx, TEnum, TUnderlying> : IEnumExtensions<TEnum, TUnderlying>
        where TEnumEx : IEnumExtensions<TEnum, TUnderlying>
        where TEnum : unmanaged, Enum
        where TUnderlying : unmanaged, IComparable, IComparable<TUnderlying>
            , IConvertible, IEquatable<TUnderlying>, IFormattable
    {
        TEnumEx Create(TEnum value);

        TEnumEx CreateFromUnderlyingValue(TUnderlying value);

        bool TryParse(string name, out TEnumEx value);

        bool TryParse(string name, out TEnumEx value, bool ignoreCase);

        bool TryParse(string name, out TEnumEx value, bool ignoreCase, bool allowMatchingMetadataAttribute);

        bool TryParse(ReadOnlySpan<char> name, out TEnumEx value);

        bool TryParse(ReadOnlySpan<char> name, out TEnumEx value, bool ignoreCase);

        bool TryParse(ReadOnlySpan<char> name, out TEnumEx value, bool ignoreCase, bool allowMatchingMetadataAttribute);
    }
}

