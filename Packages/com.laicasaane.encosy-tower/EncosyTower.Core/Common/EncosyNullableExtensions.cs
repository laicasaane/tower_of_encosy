using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class EncosyNullableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(this T? self, T? other)
            where T : struct, IEquatable<T>
        {
            return self.HasValue == other.HasValue && self.HasValue && self.Value.Equals(other.Value);
        }
    }
}
