using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class EncosyStringExtensions
    {
        /// <summary>
        /// Indicates whether the specified string is not null or empty.
        /// </summary>
        /// <param name="self">The string to check for null or empty.</param>
        /// <returns><c>true</c> if the string is not null or empty; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotEmpty(this string self)
            => string.IsNullOrEmpty(self) == false;

        /// <summary>
        /// Returns <paramref name="self"/> if it is not null or empty; otherwise, returns the specified default value.
        /// </summary>
        /// <param name="self">The string to check for null or empty.</param>
        /// <param name="defaultValue">The value to return if the original string is null or empty. Can be null.</param>
        /// <returns>The original string if it is not null or empty; otherwise, the specified default value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NotEmptyOr(this string self, string defaultValue)
            => string.IsNullOrEmpty(self) ? defaultValue : self;

        /// <summary>
        /// Indicates whether the specified <paramref name="self"/> is empty or consists only of white-space characters.
        /// </summary>
        /// <param name="self">The <c>ReadOnlySpan&lt;char&gt;</c> to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="self"/> is empty or consists only of white-space characters;
        /// otherwise, <c>false</c>.
        /// </returns>
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
