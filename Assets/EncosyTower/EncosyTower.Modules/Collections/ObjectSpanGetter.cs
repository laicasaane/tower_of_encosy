using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules
{
    public readonly ref struct ObjectSpanGetter
    {
        private readonly Span<object> _buffer;
        private readonly int _index;

        internal ObjectSpanGetter(in Span<object> buffer, int index)
        {
            _buffer = buffer;
            _index = index;
        }

        public ObjectSpanGetter TryGet<T>(out Option<T> result)
        {
            var buffer = _buffer;

            if ((uint)_index < (uint)buffer.Length
                && buffer[_index] is T value
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

        public ObjectSpanGetter TryGetThenMoveNext<T>(out Option<T> result)
        {
            var buffer = _buffer;

            if ((uint)_index < (uint)buffer.Length
                && buffer[_index] is T value
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

        public ObjectSpanGetter Get<T>(out T result)
        {
            ThrowIfIndexOutOfRange(_index, _buffer);
            result = (T)_buffer[_index];
            return this;
        }

        public ObjectSpanGetter GetThenMoveNext<T>(out T result)
        {
            ThrowIfIndexOutOfRange(_index, _buffer);
            result = (T)_buffer[_index];
            return new(_buffer, _index + 1);
        }

        /// <summary>
        /// Reset the index of this getter to 0.
        /// </summary>
        /// <returns>A copy of the current getter with the index 0</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectSpanGetter Reset()
            => new(_buffer, 0);

        /// <summary>
        /// Reset the index of this getter to <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index at that the getter will begin to read</param>
        /// <returns>A copy of the current getter with the new index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectSpanGetter MoveTo(int index)
            => new(_buffer, index);

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfIndexOutOfRange(int index, Span<object> buffer)
        {
            if ((uint)index >= (uint)buffer.Length)
            {
                throw new IndexOutOfRangeException("Index is out of range of the Memory<object>.");
            }
        }
    }
}