namespace EncosyTower.Types
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Ids;

    public readonly partial struct TypeId<T> : IEquatable<TypeId<T>>, IEquatable<TypeId>
        , IComparable<TypeId<T>>, IComparable<TypeId>
    {
        internal readonly uint _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TypeId(uint value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TypeId<T> other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TypeId other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj switch {
                TypeId<T> otherT => _value == otherT._value,
                TypeId other => _value == other._value,
                _ => false
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(TypeId<T> other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(TypeId other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id<T>(in TypeId<T> id)
            => id._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator TypeId(in TypeId<T> id)
            => new(id._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator TypeId<T>(in TypeId id)
            => new(id._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value == rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value != rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in TypeId<T> lhs, in TypeId rhs)
            => lhs._value == rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in TypeId<T> lhs, in TypeId rhs)
            => lhs._value != rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in TypeId lhs, in TypeId<T> rhs)
            => lhs._value == rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in TypeId lhs, in TypeId<T> rhs)
            => lhs._value != rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value < rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value > rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in TypeId<T> lhs, in TypeId rhs)
            => lhs._value < rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in TypeId<T> lhs, in TypeId rhs)
            => lhs._value > rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in TypeId lhs, in TypeId<T> rhs)
            => lhs._value < rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in TypeId lhs, in TypeId<T> rhs)
            => lhs._value > rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value <= rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value >= rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in TypeId<T> lhs, in TypeId rhs)
            => lhs._value <= rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in TypeId<T> lhs, in TypeId rhs)
            => lhs._value >= rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in TypeId lhs, in TypeId<T> rhs)
            => lhs._value <= rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in TypeId lhs, in TypeId<T> rhs)
            => lhs._value >= rhs._value;
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Types
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct TypeId<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_value);
            return fs;
        }
    }
}

#endif
