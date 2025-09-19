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
        , ISpanFormattable
        , IToUnderlyingValue<TUnderlyingValue>
        , IIsDefined
        , IIsNameDefined
        , IFindIndex
        where TEnum : struct, Enum
        where TUnderlyingValue : unmanaged
    { }
}

