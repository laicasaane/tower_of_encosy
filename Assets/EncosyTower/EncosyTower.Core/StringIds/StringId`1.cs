using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Ids;

namespace EncosyTower.StringIds
{
    public readonly struct StringId<T> : IEquatable<StringId<T>>
    {
        public readonly Id<T> Id;
        public readonly Bool<T> IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringId(Id id, Bool<T> isFixed)
        {
            Id = id;
            IsFixed = isFixed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringId(Id<T> id, Bool<T> isFixed)
        {
            Id = id;
            IsFixed = isFixed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(StringId<T> other)
            => Id == other.Id && IsFixed == other.IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is StringId<T> other && Id == other.Id && IsFixed == other.IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Id.GetHashCode(), IsFixed.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in StringId<T> left, in StringId<T> right)
            => left.Id == right.Id && left.IsFixed == right.IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in StringId<T> left, in StringId<T> right)
            => left.Id != right.Id || left.IsFixed != right.IsFixed;
    }
}
