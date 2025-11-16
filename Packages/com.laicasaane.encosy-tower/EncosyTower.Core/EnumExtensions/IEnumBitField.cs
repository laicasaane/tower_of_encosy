using System;
using EncosyTower.Collections;

namespace EncosyTower.EnumExtensions
{
    public interface IEnumBitField<T> : IContains<T>, IAny<T>, IUnset<T>
        where T : unmanaged, Enum
    {
    }
}
