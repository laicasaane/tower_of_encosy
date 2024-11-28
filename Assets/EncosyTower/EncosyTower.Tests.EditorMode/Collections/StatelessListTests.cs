using System.Runtime.CompilerServices;
using EncosyTower.Modules.Buffers;
using EncosyTower.Modules.Collections;
using NUnit.Framework;

namespace EncosyTower.Tests.Collections
{
    public class BufferProvider<T> : IBufferProvider<T>
    {
        private T[] _buffer = new T[4];
        private int _count;
        private int _version;

        public ref T[] Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer;
        }

        public ref int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public ref int Version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _version;
        }
    }

    public class StatelessListTests
    {
        [Test]
        public void StatelessList_Tests()
        {
            var buffer = new BufferProvider<int>();
            var list = new StatelessList<BufferProvider<int>, int>(buffer);

            Assert.AreEqual(true, list.IsValid);
            Assert.AreEqual(4, list.Capacity);
            Assert.AreEqual(0, list.Count);

            list.Add(5);
            list.Add(8);
            list.Add(2);
            list.Add(0);

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(5, list[0]);
            Assert.AreEqual(8, list[1]);
            Assert.AreEqual(2, list[2]);
            Assert.AreEqual(0, list[3]);

            list.Remove(8);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2, list[1]);
        }
    }
}
