using System;

namespace Module.Core.EnumExtensions
{
    public interface IEnumExtensions<TEnum, TUnderlyingValue>
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

