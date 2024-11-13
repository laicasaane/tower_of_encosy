using System;

namespace EncosyTower.Modules.EnumExtensions
{
    public interface IEnumExtensions<TEnum, out TUnderlyingValue>
        : IHasLength
        , IToStringFast
        , IToDisplayStringFast
        , IToUnderlyingValue<TUnderlyingValue>
        , ITryFormat
        , IIsDefined
        , IIsDefinedIn
        , IFindIndex
        where TEnum : struct, Enum
        where TUnderlyingValue : unmanaged
    { }
}

