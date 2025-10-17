using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Common;

namespace EncosyTower.StringIds
{
#if UNITY_COLLECTIONS
    using FixedString = Unity.Collections.FixedString128Bytes;
#endif

    /// <summary>
    /// A struct that represents a string in an unmanaged way.
    /// </summary>
    /// <remarks>
    /// If the package <c>com.unity.collections</c> is installed, this struct wraps a <see cref="FixedString"/>,
    /// otherwise it wraps a <see cref="HashValue64"/> of the managed <see cref="string"/>.
    /// </remarks>
    /// <seealso cref="FixedString"/>
    /// <seealso cref="HashValue64"/>
    [StructLayout(LayoutKind.Sequential, Size = 128)]
    public readonly struct UnmanagedString
        : IEquatable<UnmanagedString>
        , IComparable<UnmanagedString>
    {
#if UNITY_COLLECTIONS
        public readonly FixedString Value;
#else
        public readonly ulong Value;
#endif

        private UnmanagedString(
#if UNITY_COLLECTIONS
            in FixedString value
#else
            ulong value
#endif
        )
        {
            Value = value;
        }

        public ulong ToHashCode()
#if UNITY_COLLECTIONS
            => HashValue64.FNV1a(Value);
#else
            => Value;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => Value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnmanagedString other)
            => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is UnmanagedString other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(UnmanagedString other)
            => Value.CompareTo(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in UnmanagedString a, in UnmanagedString b)
            => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in UnmanagedString a, in UnmanagedString b)
            => !a.Equals(b);

#if UNITY_COLLECTIONS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnmanagedString(in FixedString value)
            => new(value);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnmanagedString(string value)
#if UNITY_COLLECTIONS
            => new(value);
#else
            => new(HashValue64.FNV1a(value));
#endif
    }
}
