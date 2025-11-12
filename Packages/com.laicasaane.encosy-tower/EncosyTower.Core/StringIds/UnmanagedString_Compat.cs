#if !UNITY_COLLECTIONS

#pragma warning disable IDE1006 // Naming Styles

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.StringIds
{
    partial struct UnmanagedString
    {
        public struct FixedString512Bytes : IEquatable<FixedString512Bytes>, IComparable<FixedString512Bytes>
        {
            private ushort utf8LengthInBytes;
            private FixedBytes510 bytes;

            public FixedString512Bytes(string other) : this()
            {
                Initialize(ref this, other.AsSpan());
            }

            public static int UTF8MaxLengthInBytes => 509;

            public int Length
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get
                {
                    return utf8LengthInBytes;
                }

                set
                {
                    utf8LengthInBytes = (ushort)value;

                    unsafe
                    {
                        GetUnsafePtr()[utf8LengthInBytes] = 0;
                    }
                }
            }

            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => UTF8MaxLengthInBytes;
            }

            public readonly bool IsEmpty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => utf8LengthInBytes == 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator FixedString512Bytes(string other)
                => new(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(in FixedString512Bytes a, in FixedString512Bytes b)
                => a.Equals(b);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(in FixedString512Bytes a, in FixedString512Bytes b)
                => !a.Equals(b);

            private static unsafe void Initialize(ref FixedString512Bytes fs, ReadOnlySpan<char> chars)
            {
                int worstCaseCapacity = chars.Length * 4;
                Span<byte> buffer = stackalloc byte[worstCaseCapacity];
                var length = Encoding.UTF8.GetBytes(chars, buffer);
                length = Mathf.Min(length, fs.Capacity);

                fs.Length = length;
                buffer[..length].CopyTo(fs.AsSpan());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly unsafe byte* GetUnsafePtr()
            {
                fixed (void* b = &bytes)
                {
                    return (byte*)b;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe Span<byte> AsSpan()
            {
                return new(GetUnsafePtr(), Length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly unsafe ReadOnlySpan<byte> AsReadOnlySpan()
            {
                return new(GetUnsafePtr(), Length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(FixedString512Bytes other)
            {
                return AsReadOnlySpan().SequenceEqual(other.AsReadOnlySpan());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override bool Equals(object obj)
            {
                return obj switch {
                    string other => Equals(other),
                    FixedString512Bytes other => Equals(other),
                    _ => false
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override int GetHashCode()
            {
                return HashValue.FNV1a(AsReadOnlySpan());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override string ToString()
            {
                return Encoding.UTF8.GetString(AsReadOnlySpan());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int CompareTo(FixedString512Bytes other)
            {
                return AsReadOnlySpan().SequenceCompareTo(other.AsReadOnlySpan());
            }

            [StructLayout(LayoutKind.Explicit, Size = 16)]
            private struct FixedBytes16
            {
                [FieldOffset(0)] public byte byte0000;
                [FieldOffset(1)] public byte byte0001;
                [FieldOffset(2)] public byte byte0002;
                [FieldOffset(3)] public byte byte0003;
                [FieldOffset(4)] public byte byte0004;
                [FieldOffset(5)] public byte byte0005;
                [FieldOffset(6)] public byte byte0006;
                [FieldOffset(7)] public byte byte0007;
                [FieldOffset(8)] public byte byte0008;
                [FieldOffset(9)] public byte byte0009;
                [FieldOffset(10)] public byte byte0010;
                [FieldOffset(11)] public byte byte0011;
                [FieldOffset(12)] public byte byte0012;
                [FieldOffset(13)] public byte byte0013;
                [FieldOffset(14)] public byte byte0014;
                [FieldOffset(15)] public byte byte0015;
            }

            [StructLayout(LayoutKind.Explicit, Size = 510)]
            private struct FixedBytes510
            {
                [FieldOffset(0)] public FixedBytes16 offset0000;
                [FieldOffset(16)] public FixedBytes16 offset0016;
                [FieldOffset(32)] public FixedBytes16 offset0032;
                [FieldOffset(48)] public FixedBytes16 offset0048;
                [FieldOffset(64)] public FixedBytes16 offset0064;
                [FieldOffset(80)] public FixedBytes16 offset0080;
                [FieldOffset(96)] public FixedBytes16 offset0096;
                [FieldOffset(112)] public FixedBytes16 offset0112;
                [FieldOffset(128)] public FixedBytes16 offset0128;
                [FieldOffset(144)] public FixedBytes16 offset0144;
                [FieldOffset(160)] public FixedBytes16 offset0160;
                [FieldOffset(176)] public FixedBytes16 offset0176;
                [FieldOffset(192)] public FixedBytes16 offset0192;
                [FieldOffset(208)] public FixedBytes16 offset0208;
                [FieldOffset(224)] public FixedBytes16 offset0224;
                [FieldOffset(240)] public FixedBytes16 offset0240;
                [FieldOffset(256)] public FixedBytes16 offset0256;
                [FieldOffset(272)] public FixedBytes16 offset0272;
                [FieldOffset(288)] public FixedBytes16 offset0288;
                [FieldOffset(304)] public FixedBytes16 offset0304;
                [FieldOffset(320)] public FixedBytes16 offset0320;
                [FieldOffset(336)] public FixedBytes16 offset0336;
                [FieldOffset(352)] public FixedBytes16 offset0352;
                [FieldOffset(368)] public FixedBytes16 offset0368;
                [FieldOffset(384)] public FixedBytes16 offset0384;
                [FieldOffset(400)] public FixedBytes16 offset0400;
                [FieldOffset(416)] public FixedBytes16 offset0416;
                [FieldOffset(432)] public FixedBytes16 offset0432;
                [FieldOffset(448)] public FixedBytes16 offset0448;
                [FieldOffset(464)] public FixedBytes16 offset0464;
                [FieldOffset(480)] public FixedBytes16 offset0480;
                [FieldOffset(496)] public byte byte0496;
                [FieldOffset(497)] public byte byte0497;
                [FieldOffset(498)] public byte byte0498;
                [FieldOffset(499)] public byte byte0499;
                [FieldOffset(500)] public byte byte0500;
                [FieldOffset(501)] public byte byte0501;
                [FieldOffset(502)] public byte byte0502;
                [FieldOffset(503)] public byte byte0503;
                [FieldOffset(504)] public byte byte0504;
                [FieldOffset(505)] public byte byte0505;
                [FieldOffset(506)] public byte byte0506;
                [FieldOffset(507)] public byte byte0507;
                [FieldOffset(508)] public byte byte0508;
                [FieldOffset(509)] public byte byte0509;
            }
        }
    }

    internal static class UnmanagedStringFixedStringExtensions
    {
        public static unsafe void CopyFromTruncated(this ref UnmanagedString.FixedString512Bytes fs, ReadOnlySpan<byte> utf8Chars)
        {
            var length = Mathf.Min(utf8Chars.Length, fs.Capacity);
            fs.Length = length;
            utf8Chars[..length].CopyTo(fs.AsSpan());
        }
    }
}

#endif
