using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.UnionIds.Types
{
    [Serializable]
    public struct UnionId_ULong2 : IEquatable<UnionId_ULong2>, IComparable<UnionId_ULong2>
    {
        public ulong x;
        public ulong y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in UnionId_ULong2 a, in UnionId_ULong2 b)
            => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in UnionId_ULong2 a, in UnionId_ULong2 b)
            => !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in UnionId_ULong2 a, in UnionId_ULong2 b)
            => a.CompareTo(b) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in UnionId_ULong2 a, in UnionId_ULong2 b)
            => a.CompareTo(b) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in UnionId_ULong2 a, in UnionId_ULong2 b)
            => a.CompareTo(b) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in UnionId_ULong2 a, in UnionId_ULong2 b)
            => a.CompareTo(b) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UnionId_ULong2 other)
            => x == other.x && y == other.y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is UnionId_ULong2 other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashValue.Combine(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(UnionId_ULong2 other)
        {
            int result = x.CompareTo(other.x);
            return result != 0 ? result : y.CompareTo(other.y);
        }
    }
}
