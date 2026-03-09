using System;
using System.Collections.Generic;
using EncosyTower.Collections;

namespace EncosyTower.EnumExtensions
{
    public interface IEnumBitField<T> : IContains<T>, IAny<T>, IUnsettable<T>, ISettable<T>, IEnumerable<T>
        where T : unmanaged, Enum
    {
    }
}
