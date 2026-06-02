#if UNITY_COLLECTIONS

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using EncosyTower.Common;
using Unity.Collections;

namespace EncosyTower.Collections
{
    using static EncosyTower.Common.GenericT;

    public static class EncosyFixedStringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this bool value)
        {
            return (FixedString32Bytes)(value ? bool.TrueString : bool.FalseString);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this Index value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value.Value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this Range value)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDest CastTo<TDest>(in this FixedString32Bytes fs)
            where TDest : unmanaged, INativeList<byte>, IUTF8Bytes
            => fs.CastTo(T<TDest>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDest CastTo<TDest>(in this FixedString64Bytes fs)
            where TDest : unmanaged, INativeList<byte>, IUTF8Bytes
            => fs.CastTo(T<TDest>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDest CastTo<TDest>(in this FixedString128Bytes fs)
            where TDest : unmanaged, INativeList<byte>, IUTF8Bytes
            => fs.CastTo(T<TDest>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDest CastTo<TDest>(in this FixedString512Bytes fs)
            where TDest : unmanaged, INativeList<byte>, IUTF8Bytes
            => fs.CastTo(T<TDest>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDest CastTo<TDest>(in this FixedString4096Bytes fs)
            where TDest : unmanaged, INativeList<byte>, IUTF8Bytes
            => fs.CastTo(T<TDest>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDest Cast<TSource, TDest>(this TSource fs)
            where TSource : unmanaged, INativeList<byte>, IUTF8Bytes
            where TDest : unmanaged, INativeList<byte>, IUTF8Bytes
            => fs.CastTo(T<TDest>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDest CastTo<TSource, TDest>(this TSource fs, T<TDest> _)
            where TSource : unmanaged, INativeList<byte>, IUTF8Bytes
            where TDest : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            TDest result = default;
            result.Append(fs);
            return result;
        }

        /// <summary>
        /// Copies a span of UTF-16 chars to this string (making the two strings equal).
        /// Replaces any existing content of the FixedString.
        /// </summary>
        /// <remarks>
        /// When the method returns an error, the destination string is not modified.
        /// </remarks>
        /// <typeparam name="TFixedString">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf16Chars">The span of UTF-16 chars to be copied.</param>
        /// <returns>
        /// CopyError.None if successful.
        /// Returns CopyError.Truncation if the source string is too large to fit in the destination.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CopyError CopyFrom<TFixedString>(this ref TFixedString fs, ReadOnlySpan<char> utf16Chars)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fs.Length = 0;

            if (Append(ref fs, utf16Chars) != FormatError.None)
            {
                return CopyError.Truncation;
            }

            return CopyError.None;
        }

        /// <summary>
        /// Copies a span of UTF-16 chars to this string.
        /// If the string exceeds the capacity it will be truncated.
        /// Replaces any existing content of the FixedString.
        /// </summary>
        /// <typeparam name="T">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf16Chars">The span of UTF-16 chars to be copied.</param>
        /// <returns>
        ///  CopyError.None if successful.
        ///  Returns CopyError.Truncation if the source span is too large to fit in the destination.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CopyError CopyFromTruncated<T>(this ref T fs, ReadOnlySpan<char> utf16Chars)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            unsafe
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
        }

        /// <summary>
        /// Appends a span of UTF-16 chars to this string.
        /// </summary>
        /// <remarks>
        /// When the method returns an error, the destination string is not modified.
        /// </remarks>
        /// <typeparam name="TFixedString">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf16Chars">The span of UTF-16 chars to append.</param>
        /// <returns>
        /// <see cref="FormatError.None"/> if successful.
        /// Returns <see cref="FormatError.Overflow"/> if the capacity of the destination string is exceeded.
        /// </returns>
        public static FormatError Append<TFixedString>(this ref TFixedString fs, ReadOnlySpan<char> utf16Chars)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            unsafe
            {
                // we don't know how big the expansion from UTF-16 to UTF8 will be, so we account for worst case.
                int utf16Length = utf16Chars.Length;
                int worstCaseCapacity = utf16Length * 4;
                byte* utf8Bytes = stackalloc byte[worstCaseCapacity];
                int utf8Len;

                fixed (char* chars = utf16Chars)
                {
                    CopyError err = UTF8ArrayUnsafeUtility.Copy(
                          utf8Bytes
                        , out utf8Len
                        , worstCaseCapacity
                        , chars
                        , utf16Length
                    );

                    if (err != CopyError.None)
                    {
                        return FormatError.Overflow;
                    }
                }

                return fs.Append(utf8Bytes, utf8Len);
            }
        }

        /// <summary>
        /// Copies a span of UTF8 chars to this string (making the two strings equal).
        /// Replaces any existing content of the FixedString.
        /// </summary>
        /// <remarks>
        /// When the method returns an error, the destination string is not modified.
        /// </remarks>
        /// <typeparam name="TFixedString">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf8Chars">The span of UTF8 chars to be copied.</param>
        /// <returns>
        /// CopyError.None if successful.
        /// Returns CopyError.Truncation if the source string is too large to fit in the destination.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CopyError CopyFrom<TFixedString>(this ref TFixedString fs, ReadOnlySpan<byte> utf8Chars)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fs.Length = 0;

            if (Append(ref fs, utf8Chars) != FormatError.None)
            {
                return CopyError.Truncation;
            }

            return CopyError.None;
        }

        /// <summary>
        /// Copies a span of UTF8 chars to this string.
        /// If the string exceeds the capacity it will be truncated.
        /// Replaces any existing content of the FixedString.
        /// </summary>
        /// <typeparam name="TFixedString">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf8Chars">The span of UTF8 chars to be copied.</param>
        /// <returns>
        ///  CopyError.None if successful.
        ///  Returns CopyError.Truncation if the source span is too large to fit in the destination.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CopyError CopyFromTruncated<TFixedString>(this ref TFixedString fs, ReadOnlySpan<byte> utf8Chars)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            unsafe
            {
                fixed (byte* chars = utf8Chars)
                {
                    CopyError result = UTF8ArrayUnsafeUtility.Copy(
                          fs.GetUnsafePtr()
                        , out var destLength
                        , fs.Capacity
                        , chars
                        , utf8Chars.Length
                    );

                    fs.Length = destLength;
                    return result;
                }
            }
        }

        /// <summary>
        /// Copies a FixedString to a span of characters.
        /// If the string exceeds the capacity it will be truncated.
        /// </summary>
        /// <typeparam name="TFixedString">A string type.</typeparam>
        /// <param name="fs">A string to copy.</param>
        /// <param name="dest">The destination string.</param>
        /// <param name="destLength">Outputs the number of chars written to the destination.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CopyError CopyTo<TFixedString>(this TFixedString fs, Span<char> dest, out int destLength)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            unsafe
            {
                fixed (char* chars = dest)
                {
                    ConversionError result = Unicode.Utf8ToUtf16(
                          fs.GetUnsafePtr()
                        , fs.Length
                        , chars
                        , out destLength
                        , dest.Length
                    );

                    return result == ConversionError.None ? CopyError.None : CopyError.Truncation;
                }
            }
        }

        /// <summary>
        /// Appends this FixedString to a StringBuilder.
        /// </summary>
        /// <typeparam name="TFixedString">A string type.</typeparam>
        /// <param name="fs">A string to append.</param>
        /// <param name="stringBuilder">The StringBuilder to append to.</param>
        /// <param name="destLength">The number of characters written to the StringBuilder.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendTo<TFixedString>(
              this TFixedString fs
            , [NotNull] StringBuilder stringBuilder
            , out int destLength
        )
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            unsafe
            {
                Span<char> utf16Chars = stackalloc char[fs.Length];
                fs.CopyTo(utf16Chars, out destLength);
                stringBuilder.Append(utf16Chars[..destLength]);
            }
        }

        /// <summary>
        /// Appends a span of UTF8 chars to this string.
        /// </summary>
        /// <remarks>
        /// When the method returns an error, the destination string is not modified.
        /// </remarks>
        /// <typeparam name="TFixedString">The type of the destination string.</typeparam>
        /// <param name="fs">The destination string.</param>
        /// <param name="utf8Chars">The span of UTF8 chars to append.</param>
        /// <returns>
        /// <see cref="FormatError.None"/> if successful.
        /// Returns <see cref="FormatError.Overflow"/> if the capacity of the destination string is exceeded.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FormatError Append<TFixedString>(this ref TFixedString fs, ReadOnlySpan<byte> utf8Chars)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            unsafe
            {
                fixed (byte* chars = utf8Chars)
                {
                    return fs.Append(chars, utf8Chars.Length);
                }
            }
        }

        /// <summary>
        /// Returns a span covering the entire length of <paramref name="fs"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan<TFixedString>(this ref TFixedString fs)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
            => new(fs.GetUnsafePtr(), fs.Length);

        /// <summary>
        /// Returns a readonly span covering the entire length of <paramref name="fs"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsReadOnlySpan<TFixedString>(this TFixedString fs)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            unsafe
            {
                return new(fs.GetUnsafePtr(), fs.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeText ToNativeText<TFixedString>(this TFixedString fs, Allocator allocator)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            var result = new NativeText(fs.Length, allocator);
            result.CopyFromTruncated(fs.AsReadOnlySpan());

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeText ToNativeText<TFixedString>(this TFixedString fs, AllocatorManager.AllocatorHandle allocator)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            var result = new NativeText(fs.Length, allocator);
            result.CopyFromTruncated(fs.AsReadOnlySpan());

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFormat<TFixedString>(
              this TFixedString fs
            , Span<char> destination
            , out int charsWritten
        )
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            var result = fs.CopyTo(destination, out var charCount) == CopyError.None;
            charsWritten = result ? charCount : 0;

            return result;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFormat<TFixedString>(
              this TFixedString fs
            , Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format
            , IFormatProvider provider = null
        )
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            return TryFormat(fs, destination, out charsWritten);
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}

#endif
