#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class EncosyFixedStringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this bool value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value ? bool.TrueString : bool.FalseString);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this byte value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this sbyte value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this short value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this ushort value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this int value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this uint value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this long value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this ulong value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(this float value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value);
            return fs;
        }

        public static FixedString32Bytes ToFixedString(this System.Index value)
        {
            var fs = new FixedString32Bytes();
            fs.Append(value.Value);
            return fs;
        }

        public static FixedString32Bytes ToFixedString(this System.Range value)
        {
            var fs = new FixedString32Bytes();
            fs.Append('[');
            fs.Append(value.Start.ToFixedString());
            fs.Append(',');
            fs.Append(' ');
            fs.Append(value.End.ToFixedString());
            fs.Append(']');
            return fs;
        }

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString32Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 29 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString32Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString32Bytes self)
            => new(self.GetUnsafePtr(), FixedString32Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString64Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 61 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString64Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString64Bytes self)
            => new(self.GetUnsafePtr(), FixedString64Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString128Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 125 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString128Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString128Bytes self)
            => new(self.GetUnsafePtr(), FixedString128Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString512Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 509 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString512Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString512Bytes self)
            => new(self.GetUnsafePtr(), FixedString512Bytes.UTF8MaxLengthInBytes);

        /// <summary>
        /// Returns a span covering the entire internal buffer of the <see cref="FixedString4096Bytes"/>.
        /// </summary>
        /// <remarks>
        /// The internal buffer is 4093 bytes long.
        /// </remarks>
        /// <seealso cref="FixedString4096Bytes.UTF8MaxLengthInBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString4096Bytes self)
            => new(self.GetUnsafePtr(), FixedString4096Bytes.UTF8MaxLengthInBytes);
    }
}

#endif
