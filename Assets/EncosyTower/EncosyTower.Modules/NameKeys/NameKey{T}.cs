using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.NameKeys
{
    public readonly struct NameKey<T> : IEquatable<NameKey<T>>
    {
        public readonly Id<T> Id;
        public readonly Bool<T> IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NameKey(Id id, Bool<T> isFixed)
        {
            Id = id;
            IsFixed = isFixed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NameKey(Id<T> id, Bool<T> isFixed)
        {
            Id = id;
            IsFixed = isFixed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(NameKey<T> other)
            => Id == other.Id && IsFixed == other.IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is NameKey<T> other && Id == other.Id && IsFixed == other.IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Id.GetHashCode(), IsFixed.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in NameKey<T> left, in NameKey<T> right)
            => left.Id == right.Id && left.IsFixed == right.IsFixed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in NameKey<T> left, in NameKey<T> right)
            => left.Id != right.Id || left.IsFixed != right.IsFixed;
    }
}