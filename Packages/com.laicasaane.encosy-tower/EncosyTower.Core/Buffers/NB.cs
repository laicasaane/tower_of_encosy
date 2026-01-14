// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/DualMemorySupport/NB.cs

// MIT License
//
// Copyright (c) 2015-2020 Sebastiano Mandalà
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Debugging;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Buffers
{
    /// <summary>
    /// NB stands for NativeBuffer
    /// <br/>
    /// NativeBuffers were initially mainly designed to be used inside Unity Jobs. They wrap an EntityDB array of components
    /// but do not track it. Hence, it's meant to be used temporary and locally as the array can become invalid
    /// after a submission of entities. However, they cannot be used as ref struct.
    /// <br/>
    /// NBs are wrappers of native arrays. Are not meant to resize or be freed.
    /// <br/>
    /// NBs cannot have a count, because a count of the meaningful number of items is not tracked.
    /// Example: an NB could be initialized with a size 10 and count 0. Then the buffer is used to fill entities
    /// but the count will stay zero. It's not the NB responsibility to track the count
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This struct should be <c>ref</c>, however with jobs is a common pattern to use NB as a field of a struct.
    /// This pattern should be replaced with the introduction of new structs that can be hold but must be requested
    /// through some contracts like: <c>AsReader</c>, <c>AsWriter</c>, <c>AsReadOnly</c>, <c>AsParallelReader</c>,
    /// <c>AsParallelWriter</c> and so on. In this way NB can keep track about how the buffer is used and can throw
    /// exceptions if the buffer is used in the wrong way.
    /// </remarks>
    public readonly /*ref*/ struct NB<T> where T : struct
    {
        internal readonly NBInternal<T> _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NB(NBInternal<T> nbInternal)
        {
            _buffer = nbInternal;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Capacity;
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if __ENCOSY_VALIDATION__
                ThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_buffer.Capacity);
#endif

                return ref _buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
            => _buffer.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => source[..length].CopyTo(AsSpan().Slice(destinationStartIndex, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => source[..length].TryCopyTo(AsSpan().Slice(destinationStartIndex, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination)
            => CopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
            => CopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            => AsReadOnlySpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
            => TryCopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            => TryCopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            => AsReadOnlySpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeArray<T> ToNativeArray()
            => _buffer.ToNativeArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => _buffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _buffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NB<U> Reinterpret<U>()
            where U : unmanaged
            => new(_buffer.Reinterpret<U>());

        public readonly /*ref*/ struct ReadOnly
        {
            internal readonly NBInternal<T>.ReadOnly _buffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ReadOnly(NBInternal<T> nbInternal)
            {
                _buffer = nbInternal;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ReadOnly(NBInternal<T>.ReadOnly nbInternal)
            {
                _buffer = nbInternal;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.Capacity;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.IsCreated;
            }

            public ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
#if __ENCOSY_VALIDATION__
                    ThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_buffer.Capacity);
#endif

                    return ref _buffer[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination)
                => CopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination, int length)
                => CopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
                => AsReadOnlySpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination)
                => TryCopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination, int length)
                => TryCopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
                => AsReadOnlySpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal NativeArray<T>.ReadOnly ToNativeArray()
                => _buffer.ToNativeArray();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan()
                => _buffer.AsReadOnlySpan();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NB<U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
                => new(_buffer.Reinterpret<U>());
        }
    }

    internal readonly struct NBInternal<T> : IBuffer<T> where T : struct
    {
        static NBInternal()
        {
#if __ENCOSY_VALIDATION__
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                throw new InvalidOperationException("NativeBuffer (NB) supports only unmanaged types");
#endif
        }

#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        private readonly NativeArray<T> _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NBInternal(NativeArray<T> buffer)
        {
            _buffer = buffer;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if __ENCOSY_VALIDATION__
                ThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_buffer.Length);
#endif

                unsafe
                {
                    return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer.GetUnsafePtr(), index);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
                UnsafeUtility.MemClear(_buffer.GetUnsafePtr(), _buffer.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => new CopyFromSpan<T>(AsSpan()).TryCopyFrom(destinationStartIndex, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination)
            => CopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
            => CopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            => new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
            => TryCopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            => TryCopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            => new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeArray<T> ToNativeArray()
            => _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice<T> AsNativeSlice()
            => new(_buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => _buffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _buffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NBInternal<U> Reinterpret<U>()
            where U : unmanaged
            => new(_buffer.Reinterpret<U>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NB<T>(NBInternal<T> proxy)
            => new(proxy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator NBInternal<T>(NB<T> proxy)
            => new(proxy.ToNativeArray());

        internal readonly struct ReadOnly : IReadOnlyBuffer<T>
        {
            static ReadOnly()
            {
#if __ENCOSY_VALIDATION__
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    throw new InvalidOperationException("NativeBuffer (NB) supports only unmanaged types");
#endif
            }

#if UNITY_BURST
            [Unity.Burst.NoAlias]
#endif
            private readonly NativeArray<T>.ReadOnly _buffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnly(NBInternal<T> nb)
            {
                _buffer = nb._buffer.AsReadOnly();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnly(NativeArray<T>.ReadOnly buffer)
            {
                _buffer = buffer;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.Length;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.IsCreated;
            }

            public ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
#if __ENCOSY_VALIDATION__
                    ThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_buffer.Length);
#endif

                    unsafe
                    {
                        return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer.GetUnsafeReadOnlyPtr(), index);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination)
                => CopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination, int length)
                => CopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
                => new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination)
                => TryCopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination, int length)
                => TryCopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
                => new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal NativeArray<T>.ReadOnly ToNativeArray()
                => _buffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan()
                => _buffer.AsReadOnlySpan();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NBInternal<U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
                => new(_buffer.Reinterpret<U>());

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator NB<T>.ReadOnly(ReadOnly nbInternal)
                => new(nbInternal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(NBInternal<T> nb)
                => new(nb);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(NB<T> nb)
                => new(nb._buffer);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(NB<T>.ReadOnly nb)
                => new(nb._buffer._buffer);
        }
    }
}
