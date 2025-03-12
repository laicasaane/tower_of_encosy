#if UNITY_COLLECTIONS
#define __ENCOSY_SHARED_STRING_VAULT__
#endif

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EncosyTower.StringIds
{
#if __ENCOSY_SHARED_STRING_VAULT__
    using FixedString = Unity.Collections.FixedString128Bytes;
#endif

    [StructLayout(LayoutKind.Sequential, Size = 128)]
    public readonly struct UnmanagedString
        : IEquatable<UnmanagedString>
        , IComparable<UnmanagedString>
    {
#if __ENCOSY_SHARED_STRING_VAULT__
        public readonly FixedString Value;
#else
        public readonly int Value;
#endif

        private UnmanagedString(
#if __ENCOSY_SHARED_STRING_VAULT__
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
#if __ENCOSY_SHARED_STRING_VAULT__
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

#if __ENCOSY_SHARED_STRING_VAULT__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnmanagedString(in FixedString value)
            => new(value);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnmanagedString(string value)
#if __ENCOSY_SHARED_STRING_VAULT__
            => new(value);
#else
            => new(value.GetHashCode());
#endif
    }
}
