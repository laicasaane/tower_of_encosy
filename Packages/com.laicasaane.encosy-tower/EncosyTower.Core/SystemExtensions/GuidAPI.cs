namespace EncosyTower.SystemExtensions
{
    using System;
    using System.Runtime.CompilerServices;

    public static partial class GuidAPI
    {
        /// <summary>
        /// Creates a new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.
        /// </summary>
        /// <returns>
        /// A new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.
        /// </returns>
        /// <remarks>
        ///     <para>This uses <see cref="DateTimeOffset.UtcNow" /> to determine the Unix Epoch timestamp source.</para>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid CreateVersion7()
            => CreateVersion7(DateTimeOffset.UtcNow);

        /// <summary>
        /// Creates a new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.
        /// </summary>
        /// <param name="timestamp">The date time offset used to determine the Unix Epoch timestamp.</param>
        /// <returns>
        /// A new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timestamp" />
        /// represents an offset prior to <see cref="DateTimeOffset.UnixEpoch" />.
        /// </exception>
        /// <remarks>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid CreateVersion7(DateTimeOffset timestamp)
            => Guid.NewGuid().ToVersion7(timestamp);

        /// <summary>
        /// Creates a new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.
        /// </summary>
        /// <param name="unixTimeMilliseconds">The Unix Epoch timestamp in milliseconds.</param>
        /// <returns>
        /// A new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.
        /// </returns>
        /// <remarks>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid CreateVersion7(ulong unixTimeMilliseconds)
            => Guid.NewGuid().ToVersion7(unixTimeMilliseconds);
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.SystemExtensions
{
    using System;
    using Unity.Collections;

    partial class GuidAPI
    {
        public static bool TryParse(in FixedString128Bytes guidString, out Guid result)
        {
            Span<char> utf16Chars = stackalloc char[68];

            var length = Math.Min(guidString.Length, utf16Chars.Length);

            for (var i = 0; i < length; i++)
            {
                utf16Chars[i] = (char)guidString[i];
            }

            return Guid.TryParse(utf16Chars[..length], out result);
        }
    }
}

#endif
