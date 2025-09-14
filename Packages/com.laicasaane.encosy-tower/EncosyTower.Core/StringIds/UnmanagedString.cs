using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    /// otherwise it wraps a hash code of the managed <see cref="string"/>.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Size = 128)]
    public readonly struct UnmanagedString
        : IEquatable<UnmanagedString>
        , IComparable<UnmanagedString>
    {
#if UNITY_COLLECTIONS
        public readonly FixedString Value;
#else
        public readonly int Value;
#endif

        private UnmanagedString(
#if UNITY_COLLECTIONS
            in FixedString value
#else
            int value
#endif
        )
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
#if UNITY_COLLECTIONS
            => Value.GetHashCode();
#else
            => Value;
#endif

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
            => new(value.GetHashCode());
#endif
    }
}
