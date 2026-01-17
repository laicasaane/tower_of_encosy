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
        /// Indicates whether the specified string is null or empty.
        /// </summary>
        /// <param name="self">The string to check for null or empty.</param>
        /// <returns><c>true</c> if the string is null or empty; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this string self)
            => string.IsNullOrEmpty(self);

        /// <summary>
        /// Indicates whether the specified string is null or empty or consists only of white-space characters.
        /// </summary>
        /// <param name="self">The string to check for null or empty.</param>
        /// <returns>
        /// <c>true</c> if the string is null or empty or consists only of white-space characters;
        /// otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmptyOrWhiteSpace(this string self)
            => string.IsNullOrWhiteSpace(self);

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
        /// Returns <paramref name="self"/> if it is not null or white-space; otherwise, returns the specified default value.
        /// </summary>
        /// <param name="self">The string to check for null or white-space.</param>
        /// <param name="defaultValue">The value to return if the original string is null or white-space. Can be null.</param>
        /// <returns>The original string if it is not null or white-space; otherwise, the specified default value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NotEmptyWhiteSpaceOr(this string self, string defaultValue)
            => string.IsNullOrWhiteSpace(self) ? defaultValue : self;
    }
}
