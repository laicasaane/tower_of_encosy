using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Module.Core
{
    public readonly struct ObjectMemoryGetter
    {
        private readonly Memory<object> _buffer;
        private readonly int _index;

        internal ObjectMemoryGetter(in Memory<object> buffer, int index)
        {
            _buffer = buffer;
            _index = index;
        }

        public readonly ObjectMemoryGetter TryGet<T>(out Option<T> result)
        {
            var buffer = _buffer;

            if ((uint)_index < (uint)buffer.Length
                && buffer.Span[_index] is T value
            )
            {
                result = value;
            }
            else
            {
                result = default;
            }

            return this;
        }

        public readonly ObjectMemoryGetter TryGetThenMoveNext<T>(out Option<T> result)
        {
            var buffer = _buffer;

            if ((uint)_index < (uint)buffer.Length
                && buffer.Span[_index] is T value
            )
            {
                result = value;
            }
            else
            {
                result = default;
            }

            return new(buffer, _index + 1);
        }

        public readonly ObjectMemoryGetter Get<T>(out T result)
        {
            ThrowIfIndexOutOfRange();
            result = (T)_buffer.Span[_index];
            return this;
        }

        public readonly ObjectMemoryGetter GetThenMoveNext<T>(out T result)
        {
            ThrowIfIndexOutOfRange();
            result = (T)_buffer.Span[_index];
            return new(_buffer, _index + 1);
        }

        /// <summary>
        /// Reset the index of this getter to 0.
        /// </summary>
        /// <returns>A copy of the current getter with the index 0</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ObjectMemoryGetter Reset()
            => new(_buffer, 0);

        /// <summary>
        /// Reset the index of this getter to <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index at that the getter will begin to read</param>
        /// <returns>A copy of the current getter with the new index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ObjectMemoryGetter MoveTo(int index)
            => new(_buffer, index);

        [DoesNotReturn, HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void ThrowIfIndexOutOfRange()
        {
            if ((uint)_index >= (uint)_buffer.Length)
            {
                throw new IndexOutOfRangeException("Index is out of range of the Memory<object>.");
            }
        }
    }
}