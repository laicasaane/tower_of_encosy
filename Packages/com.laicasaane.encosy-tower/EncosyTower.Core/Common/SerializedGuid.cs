namespace EncosyTower.Common
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using EncosyTower.SystemExtensions;
    using UnityEngine;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public partial struct SerializedGuid
        : IEquatable<SerializedGuid>, IEquatable<Guid>
        , IComparable<SerializedGuid>, IComparable<Guid>, IComparable
        , ISpanFormattable
    {
        public static readonly SerializedGuid Empty = new(Guid.Empty);

        [SerializeField] private ulong _ac;
        [SerializeField] private ulong _dk;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedGuid(in Guid guid)
        {
            this = new Union(guid).SerializedGuid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedGuid(ulong ac, ulong dk)
        {
            _ac = ac;
            _dk = dk;
        }

        /// <summary>
        /// An aggregation of <see cref="Guid"/> components: a, b, c.
        /// </summary>
        public readonly ulong Ac
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ac;
        }

        /// <summary>
        /// An aggregation of <see cref="Guid"/> components: d, e, f, g, h, k.
        /// </summary>
        public readonly ulong Dk
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedGuid NewGuid()
            => Guid.NewGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedGuid CreateVersion7()
            => GuidAPI.CreateVersion7();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Guid(in SerializedGuid guid)
            => guid.ToGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SerializedGuid(in Guid guid)
            => new Union(guid).SerializedGuid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in SerializedGuid lhs, in SerializedGuid rhs)
            => lhs.ToGuid() == rhs.ToGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in SerializedGuid lhs, in SerializedGuid rhs)
            => lhs.ToGuid() == rhs.ToGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(out ulong ac, out ulong dk)
        {
            ac = _ac;
            dk = _dk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Guid ToGuid()
            => new Union(this).SystemGuid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly SerializedGuid ToVersion7()
            => ToGuid().ToVersion7();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(SerializedGuid other)
            => ToGuid() == other.ToGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Guid other)
            => ToGuid().Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(SerializedGuid other)
            => ToGuid().CompareTo(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(Guid other)
            => ToGuid().CompareTo(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj switch {
                SerializedGuid other => Equals(other),
                Guid other => Equals(other),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(object obj)
            => obj switch {
                SerializedGuid other => CompareTo(other),
                Guid other => CompareTo(other),
                _ => 1,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => ToGuid().GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider provider)
            => $"({_ac.ToString(format, provider)}, {_dk.ToString(format, provider)})";

        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format
            , IFormatProvider provider
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

            if (_ac.TryFormat(destination, out var acChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[acChars..];

            if (destination.Length < 2)
            {
                return False(out charsWritten);
            }

            var delimiterChars = 0;
            destination[delimiterChars++] = ',';
            destination[delimiterChars++] = ' ';
            destination = destination[delimiterChars..];

            if (_dk.TryFormat(destination, out var dkChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[dkChars..];

            if (destination.Length < 1)
            {
                return False(out charsWritten);
            }

            var closeQuoteChars = 0;
            destination[closeQuoteChars++] = ')';

            charsWritten = openQuoteChars + acChars + delimiterChars + dkChars + closeQuoteChars;
            return true;
        }

        [StructLayout(LayoutKind.Explicit)]
        private readonly struct Union
        {
            [FieldOffset(0)] public readonly Guid SystemGuid;
            [FieldOffset(0)] public readonly SerializedGuid SerializedGuid;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Union(in Guid guid) : this()
            {
                SystemGuid = guid;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Union(in SerializedGuid guid) : this()
            {
                SerializedGuid = guid;
            }
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Common
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using Unity.Collections;

    partial struct SerializedGuid : IToFixedString<FixedString32Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => ToFixedString().ToString();

        public readonly FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append('(');
            fs.Append(_ac);
            fs.Append(',');
            fs.Append(' ');
            fs.Append(_dk);
            fs.Append(')');
            return fs;
        }
    }
}

#else

namespace EncosyTower.Common
{
    using System.Runtime.CompilerServices;

    partial struct SerializedGuid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => $"({_ac}, {_dk})";
    }
}

#endif
