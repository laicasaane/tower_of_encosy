using System;
using EncosyTower.Collections;

namespace EncosyTower.EnumExtensions
{
    public interface IEnumBitField<T> : IContains<T>, IAny<T>
        where T : unmanaged, Enum
    {
        T Unset(T flag);
    }
}
