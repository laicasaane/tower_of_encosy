
namespace EncosyTower.Modules
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public readonly partial struct LongId
        : IEquatable<LongId>
        , IComparable<LongId>
    {
        private readonly ulong _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongId(ulong value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(LongId other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(LongId other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is LongId other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
            => _value.TryFormat(destination, out charsWritten, format, provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator LongId(ulong value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(in LongId value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in LongId left, in LongId right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in LongId left, in LongId right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in LongId left, in LongId right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in LongId left, in LongId right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in LongId left, in LongId right)
            => left._value <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in LongId left, in LongId right)
            => left._value >= right._value;

        [Serializable]
        public partial struct Serializable : ITryConvert<LongId>
            , IEquatable<Serializable>
            , IComparable<Serializable>
        {
            [SerializeField]
            private ulong _value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(ulong value)
            {
                _value = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out LongId result)
            {
                result = new(_value);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable other && _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _value.GetHashCode();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => _value.ToString();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(Serializable other)
                => _value.CompareTo(other._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator LongId(Serializable value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(LongId value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left._value == right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => left._value != right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <(Serializable left, Serializable right)
                => left._value < right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <=(Serializable left, Serializable right)
                => left._value <= right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >(Serializable left, Serializable right)
                => left._value > right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >=(Serializable left, Serializable right)
                => left._value >= right._value;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Modules
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct LongId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_value);
            return fs;
        }

        public partial struct Serializable
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
}

#endif
