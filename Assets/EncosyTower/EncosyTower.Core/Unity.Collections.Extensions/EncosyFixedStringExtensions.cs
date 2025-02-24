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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString32Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<byte> AsReadOnlySpan(this in FixedString32Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString64Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<byte> AsReadOnlySpan(this in FixedString64Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString128Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<byte> AsReadOnlySpan(this in FixedString128Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString512Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<byte> AsReadOnlySpan(this in FixedString512Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> AsSpan(this in FixedString4096Bytes self)
            => new(self.GetUnsafePtr(), self.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<byte> AsReadOnlySpan(this in FixedString4096Bytes self)
            => new(self.GetUnsafePtr(), self.Length);
    }
}

#endif
