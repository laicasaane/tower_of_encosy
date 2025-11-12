using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.SystemExtensions;
using Unity.Collections;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// A struct that represents a string in an unmanaged way.
    /// </summary>
    /// <remarks>
    /// This struct wraps a <see cref="FixedString512Bytes"/>.
    /// </remarks>
    /// <seealso cref="FixedString512Bytes"/>
    [StructLayout(LayoutKind.Sequential, Size = 512)]
    public partial struct UnmanagedString
        : IEquatable<UnmanagedString>
        , IComparable<UnmanagedString>
        , IGetHashCode64
        , IAsSpan<byte>
        , IAsReadOnlySpan<byte>
    {
        private FixedString512Bytes _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnmanagedString(in FixedString512Bytes value)
        {
            _value = value;
        }

        public static int UTF8MaxLengthInBytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FixedString512Bytes.UTF8MaxLengthInBytes;
        }

        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsEmpty;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.Capacity;
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnmanagedString(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnmanagedString(in FixedString512Bytes value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in UnmanagedString a, in UnmanagedString b)
            => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in UnmanagedString a, in UnmanagedString b)
            => !a.Equals(b);

        internal static Option<UnmanagedString> FromBufferAt(Range range, ReadOnlySpan<byte> buffer)
        {
            if (range.TryGetOffsetAndLength(buffer.Length).TryGetValue(out var offsetLength) == false)
            {
                return Option.None;
            }

            var slice = buffer.Slice(offsetLength.Offset, offsetLength.Length);
            var result = new UnmanagedString();
            result._value.CopyFromTruncated(slice);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong GetHashCode64()
            => HashValue64.FNV1a(_value.AsReadOnlySpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString512Bytes ToFixedString()
            => _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UnmanagedString other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj)
            => obj is UnmanagedString other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(UnmanagedString other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan()
            => _value.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan()
            => _value.AsReadOnlySpan();
    }
}
