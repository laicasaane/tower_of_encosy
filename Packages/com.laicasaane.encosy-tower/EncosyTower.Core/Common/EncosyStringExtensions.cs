using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class EncosyStringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotEmpty(this string self)
            => string.IsNullOrEmpty(self) == false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NotEmptyOr(this string self, string defaultValue)
            => string.IsNullOrEmpty(self) ? defaultValue : self;

        public static bool IsEmptyOrWhiteSpace(in this ReadOnlySpan<char> self)
        {
            for (int i = 0; i < self.Length; i++)
            {
                if (char.IsWhiteSpace(self[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
