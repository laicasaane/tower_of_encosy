using System;

namespace EncosyTower.Common
{
    public interface IStringEnumConverter<TEnum>
        where TEnum : unmanaged, Enum
    {
        string Transform(TEnum from);

        TEnum Transform(string from);
    }
}
