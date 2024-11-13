namespace EncosyTower.Modules
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a boolean value as a byte.
    /// </summary>
    public readonly partial struct ByteBool : IEquatable<bool>, IEquatable<ByteBool>
        , IComparable, IComparable<bool>, IComparable<ByteBool>
        , ITryParse<ByteBool>, ITryParseSpan<ByteBool>
    {
        private const byte FALSE = byte.MinValue;
        private const byte TRUE = byte.MaxValue;

        public readonly static ByteBool False = new(FALSE);

        public readonly static ByteBool True = new(TRUE);

#if !UNITY_COLLECTIONS
        public static string FalseString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bool.FalseString;
        }

        public static string TrueString
        {
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bool.TrueString;
        }
#endif

        private readonly byte _raw;

        public ByteBool(bool value)
        {
            _raw = value ? TRUE : FALSE;
        }

        private ByteBool(byte value)
        {
            _raw = value > 0 ? TRUE : FALSE;
        }

        private ByteBool(int value)
        {
            _raw = value > 0 ? TRUE : FALSE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(bool other)
            => Equals(new ByteBool(other));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ByteBool other)
            => _raw == other._raw;

        public override bool Equals(object obj)
        {
            if (obj is ByteBool other)
            {
                return _raw == other._raw;
            }

            if (obj is bool otherBool)
            {
                return Equals(otherBool);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => ((bool)this).GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => this ? bool.TrueString : bool.FalseString;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(bool other)
            => ((bool)this).CompareTo(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ByteBool other)
            => ((bool)this).CompareTo(other);

        public int CompareTo(object obj)
        {
            return obj switch {
                ByteBool other => CompareTo(other),
                bool otherBool => ((bool)this).CompareTo(otherBool),
                _ => ((bool)this).CompareTo(obj)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParse(string str, out ByteBool result)
            => TryParse(str.AsSpan(), out result);

        public bool TryParse(ReadOnlySpan<char> str, out ByteBool result)
        {
            var parseResult = bool.TryParse(str, out var value);
            result = value;
            return parseResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(ByteBool value)
            => value._raw > FALSE;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ByteBool(bool value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator byte(ByteBool value)
            => value._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ByteBool(byte value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBool operator !(ByteBool value)
            => new((byte)(~value._raw));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ByteBool lhs, ByteBool rhs)
            => lhs._raw == rhs._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ByteBool lhs, ByteBool rhs)
            => lhs._raw != rhs._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ByteBool lhs, bool rhs)
            => lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ByteBool lhs, bool rhs)
            => !lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(bool lhs, ByteBool rhs)
            => rhs.Equals(lhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(bool lhs, ByteBool rhs)
            => !rhs.Equals(lhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ByteBool left, ByteBool right)
            => left._raw < right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ByteBool left, ByteBool right)
            => left._raw <= right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ByteBool left, ByteBool right)
            => left._raw > right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ByteBool left, ByteBool right)
            => left._raw >= right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBool operator &(ByteBool left, ByteBool right)
            => new(left._raw & right._raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBool operator |(ByteBool left, ByteBool right)
            => new(left._raw | right._raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(ByteBool value)
            => value._raw == TRUE;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(ByteBool value)
            => value._raw == FALSE;
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Modules
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct ByteBool
    {
        public static FixedString32Bytes FalseString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var fs = new FixedString32Bytes();
                fs.Append((FixedString32Bytes)bool.FalseString);
                return fs;
            }
        }

        public static FixedString32Bytes TrueString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var fs = new FixedString32Bytes();
                fs.Append((FixedString32Bytes)bool.TrueString);
                return fs;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString32Bytes ToFixedString()
            => this ? TrueString : FalseString;
    }
}

#endif
