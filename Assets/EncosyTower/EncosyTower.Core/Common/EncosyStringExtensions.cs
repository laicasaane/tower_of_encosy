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
    }
}
