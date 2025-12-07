using System;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Conversion;

namespace EncosyTower.EnumExtensions
{
    public interface IEnumExtensions<TEnum, out TUnderlyingValue>
        : IHasLength
        , IToStringFast
        , IToDisplayStringFast
        , IToIndex
        , ISpanFormattable
        , IToUnderlyingValue<TUnderlyingValue>
        , IIsDefined
        , IIsNameDefined
        where TEnum : struct, Enum
        where TUnderlyingValue : unmanaged
    {
    }

    public interface IToIndex
    {
        int ToIndex();
    }
}

