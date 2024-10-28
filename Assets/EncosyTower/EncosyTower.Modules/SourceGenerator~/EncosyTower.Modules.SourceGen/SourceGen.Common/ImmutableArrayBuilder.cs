// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file is ported and adapted from ComputeSharp (Sergio0694/ComputeSharp),
// more info in ThirdPartyNotices.txt in the root of the project.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.SourceGen
{
    /// <summary>
    /// A helper type to build sequences of values with pooled buffers.
    /// </summary>
    /// <typeparam name="T">The type of items to create sequences for.</typeparam>
    public struct ImmutableArrayBuilder<T> : IDisposable
    {
        /// <summary>
        /// The shared <see cref="ObjectPool{T}"/> instance to share <see cref="Writer"/> objects.
        /// </summary>
        private static readonly ObjectPool<Writer> s_sharedObjectPool = new(static () => new Writer());

        /// <summary>
        /// The rented <see cref="Writer"/> instance to use.
        /// </summary>
        private Writer _writer;

        /// <summary>
        /// Creates a <see cref="ImmutableArrayBuilder{T}"/> value with a pooled underlying data writer.
        /// </summary>
        /// <returns>A <see cref="ImmutableArrayBuilder{T}"/> instance to write data to.</returns>
        public static ImmutableArrayBuilder<T> Rent()
        {
            return new(s_sharedObjectPool.Allocate());
        }

        /// <summary>
        /// Creates a new <see cref="ImmutableArrayBuilder{T}"/> object with the specified parameters.
        /// </summary>
        /// <param name="writer">The target data writer to use.</param>
        private ImmutableArrayBuilder(Writer writer)
        {
            this._writer = writer;
        }

        /// <inheritdoc cref="ImmutableArray{T}.Builder.Count"/>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this._writer!.Count;
        }

        /// <summary>
        /// Gets the data written to the underlying buffer so far, as a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        public readonly ReadOnlySpan<T> WrittenSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this._writer!.WrittenSpan;
        }

        /// <inheritdoc cref="ImmutableArray{T}.Builder.Add(T)"/>
        public readonly void Add(T item)
        {
            this._writer!.Add(item);
        }

        /// <summary>
        /// Adds the specified items to the end of the array.
        /// </summary>
        /// <param name="items">The items to add at the end of the array.</param>
        public readonly void AddRange(ReadOnlySpan<T> items)
        {
            this._writer!.AddRange(items);
        }

        /// <summary>
        /// Adds the specified items to the end of the array.
        /// </summary>
        /// <param name="items">The items to add at the end of the array.</param>
        public readonly void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <inheritdoc cref="ImmutableArray{T}.Builder.ToImmutable"/>
        public readonly ImmutableArray<T> ToImmutable()
        {
            T[] array = this._writer!.WrittenSpan.ToArray();

            return Unsafe.As<T[], ImmutableArray<T>>(ref array);
        }

        /// <inheritdoc cref="ImmutableArray{T}.Builder.ToArray"/>
        public readonly T[] ToArray()
        {
            return this._writer!.WrittenSpan.ToArray();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return this._writer!.WrittenSpan.ToString();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Writer writer = this._writer;

            this._writer = null;

            if (writer is not null)
            {
                writer.Clear();

                s_sharedObjectPool.Free(writer);
            }
        }

        /// <summary>
        /// A class handling the actual buffer writing.
        /// </summary>
        private sealed class Writer
        {
            /// <summary>
            /// The underlying <typeparamref name="T"/> array.
            /// </summary>
            private T[] _array;

            /// <summary>
            /// The starting offset within <see cref="_array"/>.
            /// </summary>
            private int _index;

            /// <summary>
            /// Creates a new <see cref="Writer"/> instance with the specified parameters.
            /// </summary>
            public Writer()
            {
                if (typeof(T) == typeof(char))
                {
                    this._array = new T[1024];
                }
                else
                {
                    this._array = new T[8];
                }

                this._index = 0;
            }

            /// <inheritdoc cref="ImmutableArrayBuilder{T}.Count"/>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this._index;
            }

            /// <inheritdoc cref="ImmutableArrayBuilder{T}.WrittenSpan"/>
            public ReadOnlySpan<T> WrittenSpan
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(this._array, 0, this._index);
            }

            /// <inheritdoc cref="ImmutableArrayBuilder{T}.Add"/>
            public void Add(T value)
            {
                EnsureCapacity(1);

                this._array[this._index++] = value;
            }

            /// <inheritdoc cref="ImmutableArrayBuilder{T}.AddRange"/>
            public void AddRange(ReadOnlySpan<T> items)
            {
                EnsureCapacity(items.Length);

                items.CopyTo(this._array.AsSpan(this._index));

                this._index += items.Length;
            }

            /// <summary>
            /// Clears the items in the current writer.
            /// </summary>
            public void Clear()
            {
                if (typeof(T) != typeof(char))
                {
                    this._array.AsSpan(0, this._index).Clear();
                }

                this._index = 0;
            }

            /// <summary>
            /// Ensures that <see cref="_array"/> has enough free space to contain a given number of new items.
            /// </summary>
            /// <param name="requestedSize">The minimum number of items to ensure space for in <see cref="_array"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void EnsureCapacity(int requestedSize)
            {
                if (requestedSize > this._array.Length - this._index)
                {
                    ResizeBuffer(requestedSize);
                }
            }

            /// <summary>
            /// Resizes <see cref="_array"/> to ensure it can fit the specified number of new items.
            /// </summary>
            /// <param name="sizeHint">The minimum number of items to ensure space for in <see cref="_array"/>.</param>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private void ResizeBuffer(int sizeHint)
            {
                var minimumSize = this._index + sizeHint;
                var requestedSize = Math.Max(this._array.Length * 2, minimumSize);

                var newArray = new T[requestedSize];

                Array.Copy(this._array, newArray, this._index);

                this._array = newArray;
            }
        }
    }

    /// <summary>
    /// Private helpers for the <see cref="ImmutableArrayBuilder{T}"/> type.
    /// </summary>
    internal static class ImmutableArrayBuilder
    {
        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> for <c>"index"</c>.
        /// </summary>
        public static void ThrowArgumentOutOfRangeExceptionForIndex()
        {
            throw new ArgumentOutOfRangeException("index");
        }
    }
}
