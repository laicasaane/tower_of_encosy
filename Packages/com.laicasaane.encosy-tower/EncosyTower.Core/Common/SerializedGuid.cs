namespace EncosyTower.Common
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using EncosyTower.Collections;
    using EncosyTower.SystemExtensions;
    using UnityEngine;

    [Serializable]
    public unsafe partial struct SerializedGuid
        : IEquatable<SerializedGuid>, IEquatable<Guid>
        , IComparable<SerializedGuid>, IComparable<Guid>, IComparable
        , ISpanFormattable
        , IAsReadOnlySpan<byte>
    {
        private const int SIZE = 16;

        public static readonly SerializedGuid Empty = new(Guid.Empty);

        [SerializeField] private fixed byte _bytes[SIZE];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedGuid(in Guid guid)
        {
            this = new Union(guid).SerializedGuid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedGuid(ReadOnlySpan<byte> bytes)
        {
            this = new Union(new Guid(bytes)).SerializedGuid;
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
        public readonly string ToString(string format, IFormatProvider provider)
            => ToGuid().ToString(format, provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            return ToGuid().TryFormat(destination, out charsWritten, format);
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
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using EncosyTower.SystemExtensions;
    using Unity.Collections;
    using UnityEngine;

    partial struct SerializedGuid : IToFixedString<FixedString128Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedGuid(in FixedString128Bytes guidString)
        {
            if (GuidAPI.TryParse(guidString, out Guid result))
            {
                this = new Union(result).SerializedGuid;
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
            => ToGuid().ToFixedString();

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
        /// SerializedGuid.ToFixedString(stackalloc char[1] { 'N' });
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString128Bytes ToFixedString(ReadOnlySpan<char> format)
            => ToGuid().ToFixedString(format);

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

    partial struct SerializedGuid
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
