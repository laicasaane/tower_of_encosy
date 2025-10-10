namespace EncosyTower.StringIds
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using EncosyTower.Ids;
    using UnityEngine;

    /// <summary>
    /// Represents a lightweight handle for a string with an additional metadata.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="StringId"/> must be retrieved from <see cref="StringToId"/>.
    /// <br/>
    /// <see cref="StringId"/> constructors should not be used because they cannot
    /// guarantee the uniqueness of a string or its (assumingly) associative ID.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct MetaStringId
        : IEquatable<MetaStringId>
        , IComparable<MetaStringId>
        , ISpanFormattable
    {
        [FieldOffset(0), SerializeField, HideInInspector]
        private readonly ulong _value;

        [FieldOffset(0), NonSerialized]
        private readonly StringId _id;

        [FieldOffset(4), NonSerialized]
        private readonly Id _meta;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MetaStringId(ulong value) : this()
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MetaStringId(StringId id, Id meta) : this()
        {
            _id = id;
            _meta = meta;
        }

        public readonly StringId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _id;
        }

        public readonly Id Meta
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _meta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(out StringId id, out Id meta)
        {
            id = _id;
            meta = _meta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(MetaStringId other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is MetaStringId other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(MetaStringId other)
            => _value.CompareTo(other._value);

        public readonly bool TryFormat(
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

            if (_id.TryFormat(destination, out var idChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[idChars..];

            if (destination.Length < 2)
            {
                return False(out charsWritten);
            }

            var delimiterChars = 0;
            destination[delimiterChars++] = '-';
            destination = destination[delimiterChars..];

            if (_meta.TryFormat(destination, out var metaChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            charsWritten = idChars + delimiterChars + metaChars;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
            => $"{_id.ToString(format, formatProvider)}-{_meta.ToString(format, formatProvider)}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ulong(in MetaStringId id)
            => id._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MetaStringId(in (Id x, StringId y) tuple)
            => new(tuple.x, tuple.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in MetaStringId left, in MetaStringId right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in MetaStringId left, in MetaStringId right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in MetaStringId left, in MetaStringId right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in MetaStringId left, in MetaStringId right)
            => left._value >= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in MetaStringId left, in MetaStringId right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in MetaStringId left, in MetaStringId right)
            => left._value <= right._value;
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.StringIds
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using Unity.Collections;

    partial struct MetaStringId : IToFixedString<FixedString32Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => ToFixedString().ToString();

        public readonly FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_id.Id);
            fs.Append('-');
            fs.Append(_meta);
            return fs;
        }
    }
}

#else

namespace EncosyTower.StringIds
{
    using System.Runtime.CompilerServices;

    partial struct MetaStringId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => $"{_id}-{_meta}";
    }
}

#endif
