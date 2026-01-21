namespace EncosyTower.Common
{
    public static partial class EncosyStringBuilderExtensions
    {
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Common
{
    using System.Runtime.CompilerServices;
    using System.Text;
    using EncosyTower.Collections;
    using Unity.Collections;

    partial class EncosyStringBuilderExtensions
    {
        /// <summary>
        /// Appends the content of a fixed string to the StringBuilder.
        /// </summary>
        /// <typeparam name="TFixedString">The type of the fixed string.</typeparam>
        /// <param name="self">The StringBuilder instance.</param>
        /// <param name="fs">The fixed string to append.</param>
        /// <returns>The updated StringBuilder instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append<TFixedString>(this StringBuilder self, in TFixedString fs)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            if (self is not null)
            {
                fs.AppendTo(self, out _);
            }

            return self;
        }

        /// <summary>
        /// Appends the content of a fixed string followed by a newline to the StringBuilder.
        /// </summary>
        /// <typeparam name="TFixedString">The type of the fixed string.</typeparam>
        /// <param name="self">The StringBuilder instance.</param>
        /// <param name="fs">The fixed string to append.</param>
        /// <returns>The updated StringBuilder instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine<TFixedString>(this StringBuilder self, in TFixedString fs)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            if (self is not null)
            {
                fs.AppendTo(self, out _);
                self.AppendLine();
            }

            return self;
        }
    }
}

#endif

