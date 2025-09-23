namespace EncosyTower.Common
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Collections;
    using EncosyTower.SystemExtensions;
    using UnityEngine;

    [Serializable]
    public unsafe partial struct SerializableGuid
        : IEquatable<SerializableGuid>, IEquatable<Guid>
        , IComparable<SerializableGuid>, IComparable<Guid>, IComparable
        , ISpanFormattable
        , IAsReadOnlySpan<byte>
    {
        /// <summary>
        /// Size in bytes.
        /// </summary>
        public const int SIZE = 16;

        public static readonly SerializableGuid Empty = new(Guid.Empty);

        [SerializeField] private fixed byte _bytes[SIZE];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableGuid(in Guid guid)
        {
            this = guid.AsSerializable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableGuid(ReadOnlySpan<byte> bytes)
        {
            this = new Guid(bytes).AsSerializable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializableGuid NewGuid()
            => Guid.NewGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializableGuid CreateVersion7()
            => GuidAPI.CreateVersion7();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Guid(in SerializableGuid guid)
            => guid.AsGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SerializableGuid(in Guid guid)
            => guid.AsSerializable();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in SerializableGuid lhs, in SerializableGuid rhs)
            => lhs.AsGuid() == rhs.AsGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in SerializableGuid lhs, in SerializableGuid rhs)
            => lhs.AsGuid() == rhs.AsGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(
              out int a
            , out short b
            , out short c
            , out byte d
            , out byte e
            , out byte f
            , out byte g
            , out byte h
            , out byte i
            , out byte j
            , out byte k
        )
        {
            (a, b, c, d, e, f, g, h, i, j, k) = this.AsGuid();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly SerializableGuid ToVersion7()
            => this.AsGuid().ToVersion7();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(SerializableGuid other)
            => this.AsGuid() == other.AsGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Guid other)
            => this.AsGuid().Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(SerializableGuid other)
            => this.AsGuid().CompareTo(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(Guid other)
            => this.AsGuid().CompareTo(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj switch {
                SerializableGuid other => Equals(other),
                Guid other => Equals(other),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(object obj)
            => obj switch {
                SerializableGuid other => CompareTo(other),
                Guid other => CompareTo(other),
                _ => 1,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => this.AsGuid().GetHashCode();

        /// <summary>
        /// Returns a string representation of the value of this instance of the Guid class,
        /// according to the provided format specifier and culture-specific format information.
        /// </summary>
        /// <param name="format">
        /// A single format specifier that indicates how to format the value of this Guid.
        /// The format parameter can be "N", "D", "B", "P", or "X".
        /// If format is null or an empty string (""), "D" is used.
        /// </param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// The value of this Guid, represented as a series of lowercase hexadecimal digits in the specified format.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider provider = null)
            => this.AsGuid().ToString(format, provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            return this.AsGuid().TryFormat(destination, out charsWritten, format);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> destination)
            => AsReadOnlySpan().CopyTo(destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan()
        {
            fixed (void* ptr = _bytes)
            {
                return new ReadOnlySpan<byte>(ptr, SIZE);
            }
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Common
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using EncosyTower.SystemExtensions;
    using Unity.Collections;
    using UnityEngine;

    partial struct SerializableGuid : IToFixedString<FixedString128Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableGuid(in FixedString128Bytes guidString)
        {
            if (GuidAPI.TryParse(guidString, out Guid result))
            {
                this = result.AsSerializable();
            }
            else
            {
                ThrowIfCannotParse(guidString);
            }
        }

        /// <summary>
        /// Returns a string representation of the value of this instance in registry format.
        /// </summary>
        /// <returns>
        /// <para>The value of this Guid, formatted by using the "D" format specifier as follows:</para>
        /// <para><code>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</code></para>
        /// <para>
        /// where the value of the GUID is represented as a series of lowercase hexadecimal digits
        /// in groups of 8, 4, 4, 4, and 12 digits and separated by hyphens.
        /// </para>
        /// <para> An example of a return value is "382c74c3-721d-4f34-80e5-57657b6cbc27". </para>
        /// <para>
        /// To convert the hexadecimal digits from a through f to uppercase,
        /// call the ToUpper() method on the returned string.
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => ToFixedString().ToString();

        /// <summary>
        /// Returns a <see cref="FixedString128Bytes"/> representation of the value of this instance in registry format.
        /// </summary>
        /// <returns>
        /// <para>The value of this Guid, formatted by using the "D" format specifier as follows:</para>
        /// <para><code>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</code></para>
        /// <para>
        /// where the value of the GUID is represented as a series of lowercase hexadecimal digits
        /// in groups of 8, 4, 4, 4, and 12 digits and separated by hyphens.
        /// </para>
        /// <para> An example of a return value is "382c74c3-721d-4f34-80e5-57657b6cbc27". </para>
        /// <para>
        /// To convert the hexadecimal digits from a through f to uppercase,
        /// call the ToUpperAscii() method on the returned string.
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString128Bytes ToFixedString()
            => this.AsGuid().ToFixedString();

        /// <summary>
        /// Returns a  <see cref="FixedString128Bytes"/> representation of the value of this instance of the Guid class,
        /// according to the provided format specifier and culture-specific format information.
        /// </summary>
        /// <param name="format">
        /// A single format specifier that indicates how to format the value of this Guid.
        /// The format parameter can be "N", "D", "B", "P", or "X".
        /// If format is null or an empty string (""), "D" is used.
        /// </param>
        /// <returns>
        /// The value of this Guid, represented as a series of lowercase hexadecimal digits in the specified format.
        /// </returns>
        /// <example>
        /// <code>
        /// SerializableGuid.ToFixedString(stackalloc char[1] { 'N' });
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString128Bytes ToFixedString(ReadOnlySpan<char> format)
            => this.AsGuid().ToFixedString(format);

        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        private static void ThrowIfCannotParse(in FixedString128Bytes guidString)
        {
            throw new ArgumentException($"Cannot parse '{guidString}' into a Guid.", nameof(guidString));
        }
    }
}

#else

namespace EncosyTower.Common
{
    using System.Runtime.CompilerServices;

    partial struct SerializableGuid
    {
        /// <summary>
        /// Returns a string representation of the value of this instance in registry format.
        /// </summary>
        /// <returns>
        /// <para>The value of this Guid, formatted by using the "D" format specifier as follows:</para>
        /// <para><code>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</code></para>
        /// <para>
        /// where the value of the GUID is represented as a series of lowercase hexadecimal digits
        /// in groups of 8, 4, 4, 4, and 12 digits and separated by hyphens.
        /// </para>
        /// <para> An example of a return value is "382c74c3-721d-4f34-80e5-57657b6cbc27". </para>
        /// <para>
        /// To convert the hexadecimal digits from a through f to uppercase,
        /// call the <see cref="string.ToUpper()"/> method on the returned string.
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => ToGuid().ToString();
    }
}

#endif
