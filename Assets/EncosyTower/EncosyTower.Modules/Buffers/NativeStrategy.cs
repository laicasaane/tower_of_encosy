// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/DualMemorySupport/NativeStrategy.cs

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

#if UNITY_COLLECTIONS
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Modules.Buffers
{
    /// <summary>
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, with these, datastructure can use interchangeably
    /// native and managed memory and other strategies.
    /// </summary>
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : struct
    {
#if DEBUG && !PROFILE_SVELTO
        static NativeStrategy()
        {
            if (TypeCache<T>.IsUnmanaged == false)
                throw new InvalidOperationException("Only unmanaged data can be stored natively");
        }
#endif

        internal NativeReference<Allocator> _nativeAllocator;
        internal NBInternal<T> _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStrategy(int size, Allocator allocator, bool clear = true) : this()
        {
            Alloc(size, allocator, clear);
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.Capacity;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.IsValid;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int newCapacity, Allocator allocator, bool memClear = true)
        {
#if ENABLE_DEBUG_CHECKS
            if (_realBuffer.ToNativeArray().IsCreated)
                throw new InvalidOperationException("Cannot allocate an already allocated buffer");

            if (allocator != Allocator.Persistent && allocator != Allocator.Temp && allocator != Allocator.TempJob)
                throw new Exception("Invalid allocator used for native strategy");
#endif

            _nativeAllocator = new(allocator, NativeArrayOptions.UninitializedMemory) {
                Value = allocator
            };

            var memType = memClear ? NativeArrayOptions.ClearMemory : NativeArrayOptions.UninitializedMemory;
            var array = new NativeArray<T>(newCapacity, allocator, memType);
            _realBuffer = new NBInternal<T>(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize, bool copyContent = true, bool memClear = true)
        {
#if ENABLE_DEBUG_CHECKS
            if (_nativeAllocator.IsCreated == false || _realBuffer.ToNativeArray().IsCreated == false)
                throw new InvalidOperationException("Cannot resize an uninitialized buffer");
#endif

            var capacity = Capacity;

            if (newSize != capacity)
            {
                var realBuffer = _realBuffer.ToNativeArray();
                var memType = memClear ? NativeArrayOptions.ClearMemory : NativeArrayOptions.UninitializedMemory;
                var newBuffer = new NativeArray<T>(newSize, _nativeAllocator.Value, memType);
                var copyLength = Math.Min(capacity, newSize);
                realBuffer.AsReadOnlySpan().CopyTo(newBuffer.AsSpan()[..copyLength]);
                realBuffer.Dispose();
                _realBuffer = new NBInternal<T>(newBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
            => _realBuffer.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NB<T> ToRealBuffer()
            => _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => _realBuffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _realBuffer.AsSpan();

        public void Dispose()
        {
            var array = _realBuffer.ToNativeArray();

            if (array.IsCreated)
                array.Dispose();
            else
                throw new InvalidOperationException("Trying to dispose a disposed buffer");

            _realBuffer = default;
        }
    }
}

#endif
