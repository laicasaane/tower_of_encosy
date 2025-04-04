using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class EncosyDateTimeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeId ToId(this DateTime self)
            => new(self);
    }
}
