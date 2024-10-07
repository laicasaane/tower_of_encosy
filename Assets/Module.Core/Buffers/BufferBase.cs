using System;
using System.Runtime.CompilerServices;

namespace Module.Core.Buffers
{
    public abstract class BufferBase<T> : IBufferProvider<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
    {
        private int _count;
        private int _version;

        public abstract ref T[] Buffer { get; }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Buffer.Length;
        }

        public ref int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _count;
        }

        public ref int Version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => Buffer.AsSpan(0, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => Buffer.AsSpan(0, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
            => Buffer.AsMemory(0, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
            => Buffer.AsMemory(0, Count);
    }
}