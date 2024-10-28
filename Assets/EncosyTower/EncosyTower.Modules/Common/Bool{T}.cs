namespace EncosyTower.Modules
{
    using System;
    using System.Runtime.CompilerServices;

    public readonly partial struct Bool<T> : IEquatable<bool>, IEquatable<ByteBool>, IEquatable<Bool<T>>
        , IComparable, IComparable<bool>, IComparable<ByteBool>, IComparable<Bool<T>>
    {
        public static readonly Bool<T> True = new(true);

        public static readonly Bool<T> False = new(false);

#if !UNITY_COLLECTIONS
        public static string FalseString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ByteBool.FalseString;
        }

        public static string TrueString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ByteBool.TrueString;
        }
#endif

        private readonly ByteBool _value;

        private Bool(bool value)
        {
            _value = value;
        }

        private Bool(ByteBool value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(bool other)
            => _value == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ByteBool other)
            => _value == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<T> other)
            => _value == other._value;

        public override bool Equals(object obj)
        {
            if (obj is Bool<T> other)
            {
                return _value == other._value;
            }

            if (obj is ByteBool otherByteBool)
            {
                return _value == otherByteBool;
            }

            if (obj is bool otherBool)
            {
                return _value == otherBool;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(bool other)
            => _value.CompareTo(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ByteBool other)
            => _value.CompareTo(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Bool<T> other)
            => _value.CompareTo(other._value);

        public int CompareTo(object obj)
        {
            if (obj is Bool<T> other)
            {
                return _value.CompareTo(other._value);
            }

            if (obj is ByteBool otherByteBool)
            {
                return _value.CompareTo(otherByteBool);
            }

            if (obj is bool otherBool)
            {
                return _value.CompareTo(otherBool);
            }

            return _value.CompareTo(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(Bool<T> value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ByteBool(Bool<T> value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bool<T>(bool value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bool<T>(ByteBool value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bool<T> operator !(Bool<T> value)
            => new(!value._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bool<T> left, Bool<T> right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bool<T> left, Bool<T> right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bool<T> left, bool right)
            => left._value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bool<T> left, bool right)
            => left._value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(bool left, Bool<T> right)
            => left == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(bool left, Bool<T> right)
            => left != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bool<T> left, ByteBool right)
            => left._value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bool<T> left, ByteBool right)
            => left._value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ByteBool left, Bool<T> right)
            => left == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ByteBool left, Bool<T> right)
            => left != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bool<T> left, Bool<T> right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bool<T> left, Bool<T> right)
            => left._value <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bool<T> left, Bool<T> right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bool<T> left, Bool<T> right)
            => left._value >= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bool<T> left, ByteBool right)
            => left._value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bool<T> left, ByteBool right)
            => left._value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bool<T> left, ByteBool right)
            => left._value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bool<T> left, ByteBool right)
            => left._value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ByteBool left, Bool<T> right)
            => left < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ByteBool left, Bool<T> right)
            => left <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ByteBool left, Bool<T> right)
            => left > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ByteBool left, Bool<T> right)
            => left >= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bool<T> operator &(Bool<T> left, Bool<T> right)
            => new(left._value & right._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bool<T> operator |(Bool<T> left, Bool<T> right)
            => new(left._value | right._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bool<T> operator &(Bool<T> left, ByteBool right)
            => new(left._value & right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bool<T> operator |(Bool<T> left, ByteBool right)
            => new(left._value | right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bool<T> operator &(ByteBool left, Bool<T> right)
            => new(left & right._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bool<T> operator |(ByteBool left, Bool<T> right)
            => new(left | right._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(Bool<T> value)
            => value._value == True._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(Bool<T> value)
            => value._value == False._value;
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Modules
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct Bool<T>
    {
        public static FixedString32Bytes FalseString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ByteBool.FalseString;
        }

        public static FixedString32Bytes TrueString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ByteBool.TrueString;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString32Bytes ToFixedString()
            => _value.ToFixedString();
    }
}

#endif
