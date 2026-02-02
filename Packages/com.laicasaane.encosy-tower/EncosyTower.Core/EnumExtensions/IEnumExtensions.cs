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
}

