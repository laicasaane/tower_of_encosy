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
using EncosyTower.Debugging;
using EncosyTower.Types;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace EncosyTower.Buffers
{
    /// <summary>
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, with these, datastructure can use interchangeably
    /// native and managed memory and other strategies.
    /// </summary>
    public struct NativeStrategy<T> : IBufferStrategy<T>, IAsNativeSlice<T>, IAsNativeSliceReadOnly<T>
        where T : unmanaged
    {
#if __ENCOSY_VALIDATION__
        static NativeStrategy()
        {
            ThrowHelper.ThrowIfNotUnmanagedType<T>(EncosyTypeExtensions.IsUnmanaged<T>());
        }
#endif

        internal NativeReference<AllocatorStrategy> _nativeAllocator;
        internal NBInternal<T> _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStrategy(int size, AllocatorStrategy allocatorStrategy, bool clear = true) : this()
        {
            ThrowIfInvalidAllocatorStrategy(allocatorStrategy.IsValid);

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
            ThrowIfBufferAlreadyAllocated(_realBuffer.AsNativeArray().IsCreated);

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

            ThrowIfInvalidAllocatorStrategy(allocatorStrategy.IsValid);
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
            ThrowIfResizeUninitializedBuffer(_nativeAllocator.IsCreated && _realBuffer.AsNativeArray().IsCreated);

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

            ThrowIfInvalidAllocatorStrategy(allocatorStrategy.IsValid);
        }

        private void Resize(int newSize, bool copyContent, bool memClear, AllocatorManager.AllocatorHandle allocator)
        {
            var oldBuffer = _realBuffer.AsNativeArray();
            var oldLength = oldBuffer.Length;
            var newBuffer = memClear
                ? NativeArray.Create<T>(newSize, allocator)
                : NativeArray.CreateFast<T>(newSize, allocator);

            if (copyContent)
            {
                var copyLength = math.min(oldLength, newSize);
                oldBuffer.AsReadOnlySpan()[..copyLength].CopyTo(newBuffer.AsSpan()[..copyLength]);
            }

            oldBuffer.Dispose();
            _realBuffer = new NBInternal<T>(newBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FastClear() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
            => _realBuffer.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly NativeArray<T> AsNativeArray()
            => _realBuffer.AsNativeArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NB<T> AsRealBuffer()
            => _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSlice<T> AsNativeSlice()
            => _realBuffer.AsNativeSlice();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSliceReadOnly<T> AsNativeSliceReadOnly()
            => _realBuffer.AsNativeSliceReadOnly();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
            => _realBuffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
            => _realBuffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnly AsReadOnly()
            => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeStrategy<U> Reinterpret<U>()
            where U : unmanaged
            => new(_nativeAllocator, _realBuffer.Reinterpret<U>());

        public void Dispose()
        {
            var array = _realBuffer.AsNativeArray();

            ThrowIfAlreadyDisposed(array.IsCreated);

            if (array.IsCreated)
            {
                array.Dispose();
            }

            _realBuffer = default;
        }

        public readonly struct ReadOnly : IReadOnlyBufferStrategy<T>, IAsNativeSliceReadOnly<T>
        {
#if __ENCOSY_VALIDATION__
            static ReadOnly()
            {
                ThrowHelper.ThrowIfNotUnmanagedType<T>(EncosyTypeExtensions.IsUnmanaged<T>());
            }
#endif

            internal readonly NativeReference<AllocatorStrategy>.ReadOnly _nativeAllocator;
            internal readonly NBInternal<T>.ReadOnly _realBuffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnly(NativeStrategy<T> strategy)
            {
                _nativeAllocator = strategy._nativeAllocator;
                _realBuffer = strategy._realBuffer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnly(
                  NativeReference<AllocatorStrategy>.ReadOnly nativeAllocator
                , NBInternal<T>.ReadOnly realBuffer
            )
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

            public ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _realBuffer[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal readonly NativeArray<T>.ReadOnly AsNativeArray()
                => _realBuffer.AsNativeArray();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly NB<T>.ReadOnly AsRealBuffer()
                => _realBuffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly NativeSliceReadOnly<T> AsNativeSliceReadOnly()
                => _realBuffer.AsNativeSliceReadOnly();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly ReadOnlySpan<T> AsReadOnlySpan()
                => _realBuffer.AsReadOnlySpan();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly NativeStrategy<U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
                => new(_nativeAllocator, _realBuffer.Reinterpret<U>());

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(NativeStrategy<T> strategy)
                => new(strategy);
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalidAllocatorStrategy([DoesNotReturnIf(false)] bool validStrategy)
        {
            if (validStrategy == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new(
                    "Allocator strategy must be either Unity.Collections.Allocator " +
                    "or Unity.Collections.AllocatorManager.AllocatorHandle"
                );
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfBufferAlreadyAllocated([DoesNotReturnIf(true)] bool isCreated)
        {
            if (isCreated)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot allocate an already allocated buffer.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfResizeUninitializedBuffer([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot resize an uninitialized buffer.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfAlreadyDisposed([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot dispose an already disposed buffer.");
        }
    }
}

#endif
