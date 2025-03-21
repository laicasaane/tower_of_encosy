using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class EncosyTowerDateTimeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeId ToId(this DateTime self)
            => new(self);
    }
}
