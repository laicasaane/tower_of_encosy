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
#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using Unity.Collections;
using Unity.Mathematics;

namespace EncosyTower.Buffers
{
    /// <summary>
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, with these, datastructure can use interchangeably
    /// native and managed memory and other strategies.
    /// </summary>
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : unmanaged
    {
#if DEBUG && !PROFILE_SVELTO
        static NativeStrategy()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                throw new InvalidOperationException("Only unmanaged data can be stored natively");
        }
#endif

        internal NativeReference<AllocatorStrategy> _nativeAllocator;
        internal NBInternal<T> _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStrategy(int size, AllocatorStrategy allocatorStrategy, bool clear = true) : this()
        {
            ThrowIfInvalidAllocatorStrategy(allocatorStrategy);
            Alloc(size, allocatorStrategy, clear);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeStrategy(NativeReference<AllocatorStrategy> nativeAllocator, NBInternal<T> realBuffer)
        {
            _nativeAllocator = nativeAllocator;
            _realBuffer = realBuffer;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.Capacity;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.IsCreated;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int newCapacity, AllocatorStrategy allocatorStrategy, bool memClear = true)
        {
#if __ENCOSY_VALIDATION__
            if (_realBuffer.ToNativeArray().IsCreated)
                throw new InvalidOperationException("Cannot allocate an already allocated buffer");
#endif

            if (allocatorStrategy.TryGetAllocatorHandle(out var handle))
            {
                Alloc(newCapacity, handle, memClear);
                return;
            }

            if (allocatorStrategy.TryGetAllocator(out var allocator))
            {
                Alloc(newCapacity, allocator, memClear);
                return;
            }

            ThrowIfInvalidAllocatorStrategy(allocatorStrategy);
        }

        private void Alloc(int newCapacity, AllocatorManager.AllocatorHandle allocator, bool memClear = true)
        {
            _nativeAllocator = new(allocator, NativeArrayOptions.UninitializedMemory) {
                Value = allocator
            };

            var array = memClear
                ? NativeArray.Create<T>(newCapacity, allocator)
                : NativeArray.CreateFast<T>(newCapacity, allocator);

            _realBuffer = new NBInternal<T>(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize)
            => Resize(newSize, true, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize, bool copyContent)
            => Resize(newSize, copyContent, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize, bool copyContent, bool memClear)
        {
#if __ENCOSY_VALIDATION__
            if (_nativeAllocator.IsCreated == false || _realBuffer.ToNativeArray().IsCreated == false)
                throw new InvalidOperationException("Cannot resize an uninitialized buffer");
#endif

            var capacity = Capacity;

            if (newSize == capacity)
            {
                return;
            }

            var allocatorStrategy = _nativeAllocator.Value;

            if (allocatorStrategy.TryGetAllocatorHandle(out var handle))
            {
                Resize(newSize, copyContent, memClear, handle);
                return;
            }

            if (allocatorStrategy.TryGetAllocator(out var allocator))
            {
                Resize(newSize, copyContent, memClear, allocator);
                return;
            }

            ThrowIfInvalidAllocatorStrategy(allocatorStrategy);
        }

        private void Resize(int newSize, bool copyContent, bool memClear, AllocatorManager.AllocatorHandle allocator)
        {
            var realBuffer = _realBuffer.ToNativeArray();
            var newBuffer = memClear
                ? NativeArray.Create<T>(newSize, allocator)
                : NativeArray.CreateFast<T>(newSize, allocator);

            if (copyContent)
            {
                var copyLength = math.min(Capacity, newSize);
                realBuffer.AsReadOnlySpan().CopyTo(newBuffer.AsSpan()[..copyLength]);
            }

            realBuffer.Dispose();
            _realBuffer = new NBInternal<T>(newBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FastClear() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
            => _realBuffer.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly NativeArray<T> ToNativeArray()
            => _realBuffer.ToNativeArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NB<T> ToRealBuffer()
            => _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSlice<T> AsNativeSlice()
            => _realBuffer.AsNativeSlice();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
            => _realBuffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
            => _realBuffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeStrategy<U> Reinterpret<U>()
            where U : unmanaged
        {
            return new NativeStrategy<U>(_nativeAllocator, _realBuffer.Reinterpret<U>());
        }

        public void Dispose()
        {
            var array = _realBuffer.ToNativeArray();

            if (array.IsCreated)
                array.Dispose();
#if __ENCOSY_VALIDATION__
            else
                throw new InvalidOperationException("Trying to dispose a disposed buffer");
#endif

            _realBuffer = default;
        }

        [DoesNotReturn, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalidAllocatorStrategy(AllocatorStrategy strategy)
        {
            if (strategy.IsValid)
            {
                return;
            }

            throw new InvalidOperationException(
                "Allocator strategy must be either Unity.Collections.Allocator " +
                "or Unity.Collections.AllocatorManager.AllocatorHandle"
            );
        }
    }
}

#endif
