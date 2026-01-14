using System.Runtime.CompilerServices;
using System.Text;

namespace EncosyTower.IO
{
    public static class PathAPI
    {
        /// <summary>
        /// Converts the string to a valid file name by replacing certain characters.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="sb">A <see cref="StringBuilder"/> instance to use for the conversion.</param>
        /// <returns>A valid file name.</returns>
        /// <remarks>
        /// The characters will be replaced:
        /// <list type="bullet">
        /// <item> <c>\</c> to <c>-</c> </item>
        /// <item> <c>/</c> to <c>-</c> </item>
        /// <item> <c>:</c> to <c>-</c> </item>
        /// <item> <c>*</c> to <c>-</c> </item>
        /// <item> <c>?</c> to <c>-</c> </item>
        /// <item> <c>|</c> to <c>-</c> </item>
        /// <item> <c>"</c> to <c>'</c> </item>
        /// <item> <c>&lt;</c> to <c>ᐸ</c> </item>
        /// <item> <c>&gt;</c> to <c>ᐳ</c> </item>
        /// </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFileName(string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
                .Replace('\\', '-')
                .Replace('/', '-')
                .Replace(':', '-')
                .Replace('*', '-')
                .Replace('?', '-')
                .Replace('|', '-')
                .Replace('\"', '\'')
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                ;

            return sb.ToString();
        }
    }
}
