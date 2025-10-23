#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class EncosyFixedStringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this bool value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value ? bool.TrueString : bool.FalseString);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this byte value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this sbyte value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this short value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this ushort value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this int value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this uint value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this long value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this ulong value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this float value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        public static FixedString32Bytes ToFixedString(this System.Index value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value.Value);
            return fs;
        }

        public static FixedString32Bytes ToFixedString(this System.Range value)
        {
            var fs = new FixedString32Bytes();
            fs.Append('[');
            fs.Append(value.Start.ToFixedString());
            fs.Append(',');
            fs.Append(' ');
            fs.Append(value.End.ToFixedString());
            fs.Append(']');
            return fs;
        }

        /// <summary>
        /// Copies a span of UTF16 chars to this string (making the two strings equal).
        /// Replaces any existing content of the FixedString.
        /// </summary>
        /// <remarks>
        /// When the method returns an error, the destination string is not modified.
        /// </remarks>
        /// <typeparam name="T">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf16Chars">The span of UTF16 chars to be copied.</param>
        /// <returns>
        /// CopyError.None if successful.
        /// Returns CopyError.Truncation if the source string is too large to fit in the destination.
        /// </returns>
        [ExcludeFromBurstCompatTesting("Takes managed string")]
        public static CopyError CopyFrom<T>(this ref T fs, ReadOnlySpan<char> utf16Chars)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fs.Length = 0;

            if (Append(ref fs, utf16Chars) != 0)
            {
                return CopyError.Truncation;
            }

            return CopyError.None;
        }

        /// <summary>
        /// Copies a span of UTF16 chars to this string.
        /// If the string exceeds the capacity it will be truncated.
        /// Replaces any existing content of the FixedString.
        /// </summary>
        /// <typeparam name="T">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf16Chars">The span of UTF16 chars to be copied.</param>
        /// <returns>
        ///  CopyError.None if successful.
        ///  Returns CopyError.Truncation if the source span is too large to fit in the destination.
        /// </returns>
        public unsafe static CopyError CopyFromTruncated<T>(this ref T fs, ReadOnlySpan<char> utf16Chars)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fixed (char* chars = utf16Chars)
            {
                CopyError result = UTF8ArrayUnsafeUtility.Copy(
                      fs.GetUnsafePtr()
                    , out var destLength
                    , fs.Capacity
                    , chars
                    , utf16Chars.Length
                );

                fs.Length = destLength;
                return result;
            }
        }

        /// <summary>
        /// Appends a span of UTF16 chars to this string.
        /// </summary>
        /// <remarks>
        /// When the method returns an error, the destination string is not modified.
        /// </remarks>
        /// <typeparam name="T">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf16Chars">The span of UTF16 chars to append.</param>
        /// <returns>
        /// <see cref="FormatError.None"/> if successful.
        /// Returns <see cref="FormatError.Overflow"/> if the capacity of the destination string is exceeded.
        /// </returns>
        public unsafe static FormatError Append<T>(this ref T fs, ReadOnlySpan<char> utf16Chars)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            // we don't know how big the expansion from UTF16 to UTF8 will be, so we account for worst case.
            int utf16Length = utf16Chars.Length;
            int worstCaseCapacity = utf16Length * 4;
            byte* utf8Bytes = stackalloc byte[worstCaseCapacity];
            int utf8Len;

            fixed (char* chars = utf16Chars)
            {
                var err = UTF8ArrayUnsafeUtility.Copy(utf8Bytes, out utf8Len, worstCaseCapacity, chars, utf16Length);

                if (err != CopyError.None)
                {
                    return FormatError.Overflow;
                }
            }

            return fs.Append(utf8Bytes, utf8Len);
        }

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString32Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 29 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString32Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString32Bytes self)
            => new(self.GetUnsafePtr(), FixedString32Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString64Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 61 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString64Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString64Bytes self)
            => new(self.GetUnsafePtr(), FixedString64Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString128Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 125 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString128Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString128Bytes self)
            => new(self.GetUnsafePtr(), FixedString128Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString512Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 509 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString512Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString512Bytes self)
            => new(self.GetUnsafePtr(), FixedString512Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString4096Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 4093 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString4096Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString4096Bytes self)
            => new(self.GetUnsafePtr(), FixedString4096Bytes.UTF8MaxLengthInBytes);
    }
}

#endif
