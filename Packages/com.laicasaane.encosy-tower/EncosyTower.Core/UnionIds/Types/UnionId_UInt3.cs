using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.UnionIds.Types
{
    [Serializable]
    public struct UnionId_UInt3 : IEquatable<UnionId_UInt3>, IComparable<UnionId_UInt3>
    {
        public uint x;
        public uint y;
        public uint z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in UnionId_UInt3 a, in UnionId_UInt3 b)
            => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in UnionId_UInt3 a, in UnionId_UInt3 b)
            => !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in UnionId_UInt3 a, in UnionId_UInt3 b)
            => a.CompareTo(b) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in UnionId_UInt3 a, in UnionId_UInt3 b)
            => a.CompareTo(b) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in UnionId_UInt3 a, in UnionId_UInt3 b)
            => a.CompareTo(b) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in UnionId_UInt3 a, in UnionId_UInt3 b)
            => a.CompareTo(b) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UnionId_UInt3 other)
            => x == other.x && y == other.y && z == other.z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is UnionId_UInt3 other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashValue.Combine(x, y, z);

        public readonly int CompareTo(UnionId_UInt3 other)
        {
            int result = x.CompareTo(other.x);

            if (result != 0) return result;

            result = y.CompareTo(other.y);
            return result != 0 ? result : z.CompareTo(other.z);
        }
    }
}
