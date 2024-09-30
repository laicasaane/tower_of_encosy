using System.Runtime.CompilerServices;

namespace Module.Core.Buffers
{
    public abstract class BufferBase<T> : IBufferProvider<T>
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
    }
}