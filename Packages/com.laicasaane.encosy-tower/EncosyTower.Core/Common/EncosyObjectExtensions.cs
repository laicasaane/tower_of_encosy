using System;
using System.Globalization;

namespace EncosyTower.Common
{
    public static class EncosyObjectExtensions
    {
        private static readonly string s_nullString = "Null";

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <param name="self">The object to convert to a string.</param>
        /// <returns>
        /// A string representation of <paramref name="self"/>.
        /// If <paramref name="self"/> is <c>null</c>, returns <c>"Null"</c>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="self"/> implements <see cref="IFormattable"/>,
        /// its <see cref="IFormattable.ToString(string, IFormatProvider)"/> method is called.
        /// Otherwise, its <see cref="object.ToString"/> method is called.
        /// <br/>
        /// This method uses <see cref="CultureInfo.CurrentCulture"/> to format the string representation of
        /// <paramref name="self"/>.
        /// </remarks>
        public static string GetString(this object self)
            => GetString(self, CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a string that represents the current object using the specified format provider.
        /// </summary>
        /// <param name="self">The object to convert to a string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A string representation of <paramref name="self"/>.
        /// If <paramref name="self"/> is <c>null</c>, returns <c>"Null"</c>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="self"/> implements <see cref="IFormattable"/>,
        /// its <see cref="IFormattable.ToString(string, IFormatProvider)"/> method is called.
        /// Otherwise, its <see cref="object.ToString"/> method is called.
        /// </remarks>
        public static string GetString(this object self, IFormatProvider formatProvider = null)
        {
            if (self == null)
            {
                return s_nullString;
            }

            if (self is IFormattable formattable)
            {
                return formattable.ToString(null, formatProvider);
            }

            return self.ToString();
        }
    }
}
