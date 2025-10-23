namespace EncosyTower.SystemExtensions
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Common;

    public static partial class EncosyGuidExtensions
    {
        /// <summary>
        /// Creates a new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
        /// </summary>
        /// <returns>
        /// A new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
        /// </returns>
        /// <remarks>
        ///     <para>This uses <see cref="DateTimeOffset.UtcNow" /> to determine the Unix Epoch timestamp source.</para>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ToVersion7(in this Guid self)
            => ToVersion7(self, DateTimeOffset.UtcNow);

        /// <summary>
        /// Creates a new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
        /// </summary>
        /// <param name="timestamp">The date time offset used to determine the Unix Epoch timestamp.</param>
        /// <returns>
        /// A new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timestamp" />
        /// represents an offset prior to <see cref="DateTimeOffset.UnixEpoch" />.
        /// </exception>
        /// <remarks>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ToVersion7(in this Guid self, DateTimeOffset timestamp)
            => ToSystem(ToBurstable(self).ToVersion7(timestamp));

        /// <summary>
        /// Creates a new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
        /// </summary>
        /// <param name="unixTimeMilliseconds">The Unix Epoch timestamp in milliseconds.</param>
        /// <returns>
        /// A new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
        /// </returns>
        /// <remarks>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ToVersion7(in this Guid self, ulong unixTimeMilliseconds)
            => ToSystem(ToBurstable(self).ToVersion7(unixTimeMilliseconds));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid AsGuid(in this SerializableGuid self)
            => new Union(self).SystemGuid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializableGuid AsSerializable(in this Guid self)
            => new Union(self).SerializableGuid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsReadOnlySpan(in this Guid self)
            => self.AsSerializable().AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(
              in this Guid self
            , out int a
            , out short b
            , out short c
            , out byte d
            , out byte e
            , out byte f
            , out byte g
            , out byte h
            , out byte i
            , out byte j
            , out byte k
        )
        {
            (a, b, c, d, e, f, g, h, i, j, k) = ToBurstable(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BurstableGuid ToBurstable(in Guid guid)
            => new Union(guid).BurstableGuid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Guid ToSystem(in BurstableGuid guid)
            => new Union(guid).SystemGuid;
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.SystemExtensions
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Collections;
    using Unity.Collections;

    partial class EncosyGuidExtensions
    {
        /// <summary>
        /// Converts a <see cref="Guid"/> to its equivalent <see cref="FixedString128Bytes"/> representation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString128Bytes ToFixedString(in this Guid self)
            => ToFixedString(self, stackalloc char[1] { 'D' });

        /// <summary>
        /// Converts a <see cref="Guid"/> to its equivalent <see cref="FixedString128Bytes"/> representation.
        /// </summary>
        /// <param name="format">
        /// A read-only span containing the character representing one of the following specifiers
        /// that indicates the exact format to use when interpreting the current GUID instance:
        /// "N", "D", "B", "P", or "X".
        /// </param>
        /// <example>
        /// <code>
        /// Guid.NewGuid().ToFixedString(stackalloc char[1] { 'D' });
        /// </code>
        /// </example>
        public static FixedString128Bytes ToFixedString(in this Guid self, ReadOnlySpan<char> format)
        {
            var fs = new FixedString128Bytes();
            var guid = new Union(self).BurstableGuid;

            Span<char> utf16Chars = stackalloc char[68];
            guid.TryFormat(utf16Chars, out var utf16CharsWritten, format);
            fs.Append(utf16Chars[..utf16CharsWritten]);

            return fs;
        }
    }
}

#endif

namespace EncosyTower.SystemExtensions
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using EncosyTower.Common;
    using EncosyTower.Debugging;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;

    partial class EncosyGuidExtensions
    {
        [StructLayout(LayoutKind.Explicit)]
        private readonly struct Union
        {
            [FieldOffset(0)] public readonly Guid SystemGuid;
            [FieldOffset(0)] public readonly BurstableGuid BurstableGuid;
            [FieldOffset(0)] public readonly SerializableGuid SerializableGuid;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Union(in Guid guid) : this()
            {
                SystemGuid = guid;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Union(in BurstableGuid guid) : this()
            {
                BurstableGuid = guid;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Union(in SerializableGuid guid) : this()
            {
                SerializableGuid = guid;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct BurstableGuid
        {
            // https://github.com/dotnet/runtime/blob/release/8.0/src/libraries/System.Private.CoreLib/src/System/Guid.cs

            // TryFormatCore accepts an `int flags` composed of:
            // - Lowest byte: required length
            // - Second byte: opening brace char, or 0 if no braces
            // - Third byte: closing brace char, or 0 if no braces
            // - Highest bit: 1 if use dashes, else 0
            private const int TRY_FORMAT_FLAGS_USE_DASHES = unchecked((int)0x80000000);
            private const int TRY_FORMAT_FLAGS_CURLY_BRACES = ('}' << 16) | ('{' << 8);
            private const int TRY_FORMAT_FLAGS_PARENS = (')' << 16) | ('(' << 8);

            private const byte VARIANT_10XX_MASK = 0xC0;
            private const byte VARIANT_10XX_VALUE = 0x80;

            private const ushort VERSION_MASK = 0xF000;
            private const ushort VERSION4_VALUE = 0x4000;
            private const ushort VERSION7_VALUE = 0x7000;

            private readonly int _a;
            private readonly short _b;
            private readonly short _c;
            private readonly byte _d;
            private readonly byte _e;
            private readonly byte _f;
            private readonly byte _g;
            private readonly byte _h;
            private readonly byte _i;
            private readonly byte _j;
            private readonly byte _k;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Deconstruct(
                  out int a
                , out short b
                , out short c
                , out byte d
                , out byte e
                , out byte f
                , out byte g
                , out byte h
                , out byte i
                , out byte j
                , out byte k
            )
            {
                a = _a;
                b = _b;
                c = _c;
                d = _d;
                e = _e;
                f = _f;
                g = _g;
                h = _h;
                i = _i;
                j = _j;
                k = _k;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="timestamp"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="timestamp"/> represents an offset prior to <see cref="DateTime.UnixEpoch"/>.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BurstableGuid ToVersion7(DateTimeOffset timestamp)
            {
                var unixTimeMilliseconds = timestamp.ToUnixTimeMilliseconds();

                // 2^48 is roughly 8925.5 years, which from the Unix Epoch means we won't
                // overflow until around July of 10,895. So there isn't any need to handle
                // it given that DateTimeOffset.MaxValue is December 31, 9999. However, we
                // can't represent timestamps prior to the Unix Epoch since UUIDv7 explicitly
                // stores a 48-bit unsigned value, so we do need to throw if one is passed in.
                ThrowIfNegative(unixTimeMilliseconds, nameof(timestamp));

                return ToVersion7Core((ulong)unixTimeMilliseconds);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BurstableGuid ToVersion7(ulong unixTimeMilliseconds)
            {
                return ToVersion7Core(unixTimeMilliseconds);
            }

            private BurstableGuid ToVersion7Core(ulong unixTimeMilliseconds)
            {
                BurstableGuid result = this;

                UnsafeUtilityExtensions.AsRef(in result._a) = (int)(unixTimeMilliseconds >> 16);
                UnsafeUtilityExtensions.AsRef(in result._b) = (short)(unixTimeMilliseconds);

                UnsafeUtilityExtensions.AsRef(in result._c) = (short)((result._c & ~VERSION_MASK) | VERSION7_VALUE);
                UnsafeUtilityExtensions.AsRef(in result._d) = (byte)((result._d & ~VARIANT_10XX_MASK) | VARIANT_10XX_VALUE);

                return result;
            }

            public unsafe bool TryFormat(
                  Span<char> destination
                , out int charsWritten
                , ReadOnlySpan<char> format
            )
            {
                int flags;

                if (format.Length == 0)
                {
                    flags = 36 + TRY_FORMAT_FLAGS_USE_DASHES;
                }
                else
                {
                    if (format.Length != 1)
                    {
                        ThrowBadGuidFormatSpecification();
                    }

                    switch (format[0] | 0x20)
                    {
                        case 'd':
                            flags = 36 + TRY_FORMAT_FLAGS_USE_DASHES;
                            break;

                        case 'p':
                            flags = 38 + TRY_FORMAT_FLAGS_USE_DASHES + TRY_FORMAT_FLAGS_PARENS;
                            break;

                        case 'b':
                            flags = 38 + TRY_FORMAT_FLAGS_USE_DASHES + TRY_FORMAT_FLAGS_CURLY_BRACES;
                            break;

                        case 'n':
                            flags = 32;
                            break;

                        case 'x':
                            return TryFormatX(destination, out charsWritten);

                        default:
                            flags = 0;
                            ThrowBadGuidFormatSpecification();
                            break;
                    }
                }

                return TryFormatCore(destination, out charsWritten, flags);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] // only used from two callers
            private unsafe bool TryFormatCore(
                  Span<char> destination
                , out int charsWritten
                , int flags
            )
            {
                // The low byte of flags contains the required length.
                if ((byte)flags > destination.Length)
                {
                    charsWritten = 0;
                    return false;
                }

                charsWritten = (byte)flags;
                flags >>= 8;

                fixed (char* guidChars = &MemoryMarshal.GetReference(destination))
                {
                    char* p = guidChars;

                    // The low byte of flags now contains the opening brace char (if any)
                    if ((byte)flags != 0)
                    {
                        *p++ = (char)(byte)flags;
                    }

                    flags >>= 8;

                    // Non-vectorized fallback for D, N, P and B formats:
                    // [{|(]dddddddd[-]dddd[-]dddd[-]dddd[-]dddddddddddd[}|)]
                    p += HexsToChars(p, _a >> 24, _a >> 16);
                    p += HexsToChars(p, _a >> 8, _a);

                    if (flags < 0 /* dash */)
                    {
                        *p++ = '-';
                    }

                    p += HexsToChars(p, _b >> 8, _b);

                    if (flags < 0 /* dash */)
                    {
                        *p++ = '-';
                    }

                    p += HexsToChars(p, _c >> 8, _c);

                    if (flags < 0 /* dash */)
                    {
                        *p++ = '-';
                    }

                    p += HexsToChars(p, _d, _e);

                    if (flags < 0 /* dash */)
                    {
                        *p++ = '-';
                    }

                    p += HexsToChars(p, _f, _g);
                    p += HexsToChars(p, _h, _i);
                    p += HexsToChars(p, _j, _k);

                    // The low byte of flags now contains the closing brace char (if any)
                    if ((byte)flags != 0)
                    {
                        *p = (char)(byte)flags;
                    }

                    Checks.IsTrue(p == guidChars + charsWritten - ((byte)flags != 0 ? 1 : 0));
                }

                return true;
            }

            private unsafe bool TryFormatX(Span<char> destination, out int charsWritten)
            {
                if (destination.Length < 68)
                {
                    charsWritten = 0;
                    return false;
                }

                charsWritten = 68;

                fixed (char* guidChars = &MemoryMarshal.GetReference(destination))
                {
                    char* p = guidChars;

                    // {0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
                    *p++ = '{';
                    *p++ = '0';
                    *p++ = 'x';

                    p += HexsToChars(p, _a >> 24, _a >> 16);
                    p += HexsToChars(p, _a >> 8, _a);

                    *p++ = ',';
                    *p++ = '0';
                    *p++ = 'x';

                    p += HexsToChars(p, _b >> 8, _b);

                    *p++ = ',';
                    *p++ = '0';
                    *p++ = 'x';

                    p += HexsToChars(p, _c >> 8, _c);

                    *p++ = ',';
                    *p++ = '{';

                    p += HexsToCharsHexOutput(p, _d, _e);

                    *p++ = ',';

                    p += HexsToCharsHexOutput(p, _f, _g);

                    *p++ = ',';

                    p += HexsToCharsHexOutput(p, _h, _i);

                    *p++ = ',';

                    p += HexsToCharsHexOutput(p, _j, _k);

                    *p++ = '}';
                    *p = '}';

                    Checks.IsTrue(p == guidChars + charsWritten - 1);
                }

                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static char HexToChar(int a)
            {
                a &= 0xF;
                return (char)((a > 9) ? (a - 10 + 97) : (a + 48));
            }

            private unsafe static int HexsToChars(char* guidChars, int a, int b)
            {
                *guidChars = HexToChar(a >> 4);
                guidChars[1] = HexToChar(a);
                guidChars[2] = HexToChar(b >> 4);
                guidChars[3] = HexToChar(b);
                return 4;
            }

            private unsafe static int HexsToCharsHexOutput(char* guidChars, int a, int b)
            {
                *guidChars = '0';
                guidChars[1] = 'x';
                guidChars[2] = HexToChar(a >> 4);
                guidChars[3] = HexToChar(a);
                guidChars[4] = ',';
                guidChars[5] = '0';
                guidChars[6] = 'x';
                guidChars[7] = HexToChar(b >> 4);
                guidChars[8] = HexToChar(b);
                return 9;
            }

            [HideInCallstack, StackTraceHidden, DoesNotReturn]
            private static void ThrowBadGuidFormatSpecification()
                => throw new FormatException(
                    "Format string can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\"."
                );

            [HideInCallstack, StackTraceHidden, DoesNotReturn]
            private static void ThrowIfNegative(long value, string paramName)
            {
                if (value >= 0)
                {
                    return;
                }

                throw new ArgumentOutOfRangeException(
                      paramName
                    , value
                    , $"{paramName} ('{value}') must be a non-negative value."
                );
            }
        }
    }
}
