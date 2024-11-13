namespace EncosyTower.Modules
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct Id2
        : IEquatable<Id2>
        , IComparable<Id2>
    {
        [FieldOffset(0)]
        private readonly ulong _value;

        [FieldOffset(0)]
        public readonly Id Y;

        [FieldOffset(4)]
        public readonly Id X;

        // ReSharper disable once UnusedMember.Local
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Id2(ulong value) : this()
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id2(Id x, Id y) : this()
        {
            Y = y;
            X = x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Id x, out Id y)
        {
            x = this.X;
            y = this.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Id2 other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Id2 other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Id2 other)
            => _value.CompareTo(other._value);

#if !UNITY_COLLECTIONS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"({X}, {Y})";
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool False(out int value)
            {
                value = 0;
                return false;
            }

            if (destination.Length < 1)
            {
                return False(out charsWritten);
            }

            var openQuoteChars = 0;
            destination[openQuoteChars++] = '(';
            destination = destination[openQuoteChars..];

            if (X.TryFormat(destination, out var xChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[xChars..];

            if (destination.Length < 2)
            {
                return False(out charsWritten);
            }

            var delimiterChars = 0;
            destination[delimiterChars++] = ',';
            destination[delimiterChars++] = ' ';
            destination = destination[delimiterChars..];

            if (Y.TryFormat(destination, out var yChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[yChars..];

            if (destination.Length < 1)
            {
                return False(out charsWritten);
            }

            var closeQuoteChars = 0;
            destination[closeQuoteChars++] = ')';

            charsWritten = openQuoteChars + xChars + delimiterChars + yChars + closeQuoteChars;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ulong(in Id2 id)
            => id._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id2(in (Id x, Id y) tuple)
            => new(tuple.x, tuple.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Id2 left, in Id2 right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Id2 left, in Id2 right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in Id2 left, in Id2 right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in Id2 left, in Id2 right)
            => left._value >= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in Id2 left, in Id2 right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in Id2 left, in Id2 right)
            => left._value <= right._value;

        [Serializable]
        public partial struct Serializable : ITryConvert<Id2>
            , IEquatable<Serializable>
            , IComparable<Serializable>
        {
            [SerializeField]
            private Id.Serializable _x;

            [SerializeField]
            private Id.Serializable _y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(Id.Serializable x, Id.Serializable y)
            {
                _x = x;
                _y = y;
            }

            public bool TryConvert(out Id2 result)
            {
                if (_x.TryConvert(out var x) && _y.TryConvert(out var y))
                {
                    result = new(x, y);
                    return true;
                }

                result = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => _x == other._x && _y == other._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable other && _x == other._x && _y == other._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => ((Id2)this).GetHashCode();

#if !UNITY_COLLECTIONS
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => $"({_x}, {_y})";
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(Serializable other)
                => ((Id2)this).CompareTo(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Id2(Serializable value)
                => new(value._x, value._y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(Id2 value)
                => new(value.X, value.Y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left._x == right._x && left._y == right._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => left._x != right._x || left._y != right._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >(in Serializable left, in Serializable right)
                => (Id2)left > (Id2)right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >=(in Serializable left, in Serializable right)
                => (Id2)left >= (Id2)right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <(in Serializable left, in Serializable right)
                => (Id2)left < (Id2)right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <=(in Serializable left, in Serializable right)
                => (Id2)left <= (Id2)right;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Modules
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct Id2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => ToFixedString().ToString();

        public FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append('(');
            fs.Append(X);
            fs.Append(',');
            fs.Append(' ');
            fs.Append(Y);
            fs.Append(')');
            return fs;
        }

        public partial struct Serializable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => ToFixedString().ToString();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FixedString32Bytes ToFixedString()
            {
                var fs = new FixedString32Bytes();
                fs.Append('(');
                fs.Append(_x);
                fs.Append(',');
                fs.Append(' ');
                fs.Append(_y);
                fs.Append(')');
                return fs;
            }
        }
    }
}

#endif
