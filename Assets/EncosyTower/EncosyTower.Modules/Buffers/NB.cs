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

// ReSharper disable InconsistentNaming

#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Modules.Buffers
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
        private readonly NBInternal<T> _bufferImplementation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NB(NBInternal<T> nbInternal)
        {
            _bufferImplementation = nbInternal;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bufferImplementation.Capacity;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bufferImplementation.IsValid;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && ENABLE_PARANOID_CHECKS
                if (index >= _buffer.Length)
                    throw new IndexOutOfRangeException("Paranoid check failed!");
#endif

                return ref _bufferImplementation[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            _bufferImplementation.CopyTo(sourceStartIndex, destination, destinationStartIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _bufferImplementation.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeArray<T> ToNativeArray()
        {
            return _bufferImplementation.ToNativeArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return _bufferImplementation.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return _bufferImplementation.AsSpan();
        }
    }

    internal readonly struct NBInternal<T> : IBuffer<T> where T : struct
    {
        static NBInternal()
        {
#if ENABLE_DEBUG_CHECKS
            if (TypeCache<T>.IsUnmanaged == false)
                throw new InvalidOperationException("NativeBuffer (NB) supports only unmanaged types");
#endif
        }

#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        private readonly NativeArray<T> _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NBInternal(NativeArray<T> array)
        {
            _buffer = array;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && ENABLE_PARANOID_CHECKS
                if (index >= _buffer.Length)
                    throw new IndexOutOfRangeException("Paranoid check failed!");
#endif
                unsafe
                {
                    return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer.GetUnsafePtr(), index);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            for (var i = 0; i < count; i++)
            {
                destination[i] = this[i];
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
        internal NativeArray<T> ToNativeArray()
        {
            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return _buffer.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return _buffer.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NB<T>(NBInternal<T> proxy)
            => new(proxy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NBInternal<T>(NB<T> proxy)
            => new(proxy.ToNativeArray());
    }
}
