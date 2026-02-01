using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using EncosyTower.Debugging;
using Unity.Collections;
using Unity.Collections.Internals;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections
{
    [NativeContainer]
    [NativeContainerSupportsMinMaxWriteRestriction]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(NativeSliceReadOnlyDebugView<>))]
    public unsafe struct NativeSliceReadOnly<T> : IEnumerable<T>, IEquatable<NativeSliceReadOnly<T>>
        where T : struct
    {
        [NativeDisableUnsafePtrRestriction]
        internal byte* _buffer;

        internal int _stride;
        internal int _length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal int _minIndex;
        internal int _maxIndex;

#pragma warning disable IDE1006 // Naming Styles
        internal AtomicSafetyHandle m_Safety;
#pragma warning restore IDE1006 // Naming Styles
#endif

        public readonly unsafe T this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

                return UnsafeUtility.ReadArrayElementWithStride<T>(_buffer, index, _stride);
            }
        }

        public readonly int Stride
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _stride;
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeSlice<T> slice)
            : this(slice, 0, slice.Length)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeSlice<T> slice, int start)
            : this(slice, start, slice.Length - start)
        {
        }

        public NativeSliceReadOnly(NativeSlice<T> slice, int start, int length)
        {
            var accessor = new NativeSliceAccessor<T>(slice);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (start < 0)
            {
                ThrowHelper.ThrowSliceNegativeStartException(start);
            }

            if (length < 0)
            {
                ThrowHelper.ThrowSliceNegativeLengthException(length);
            }

            if (start + length > slice.Length)
            {
                ThrowHelper.ThrowSliceRangeExceedsLengthException(slice.Length, start, length, nameof(slice));
            }

            if ((accessor.MinIndex != 0 || accessor.MaxIndex != accessor.Length - 1)
                && (start < accessor.MinIndex || accessor.MaxIndex < start || accessor.MaxIndex < start + length - 1)
            )
            {
                ThrowHelper.ThrowSliceOnRestrictedRangeException(nameof(slice));
            }
#endif

            _stride = accessor.Stride;
            _buffer = accessor.Buffer + _stride * start;
            _length = length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _minIndex = 0;
            _maxIndex = length - 1;
            m_Safety = accessor.Safety;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeSliceReadOnly<T> slice)
            : this(slice, 0, slice.Length)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeSliceReadOnly<T> slice, int start)
            : this(slice, start, slice.Length - start)
        {
        }

        public NativeSliceReadOnly(NativeSliceReadOnly<T> slice, int start, int length)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (start < 0)
            {
                ThrowHelper.ThrowSliceNegativeStartException(start);
            }

            if (length < 0)
            {
                ThrowHelper.ThrowSliceNegativeLengthException(length);
            }

            if (start + length > slice.Length)
            {
                ThrowHelper.ThrowSliceRangeExceedsLengthException(slice.Length, start, length, nameof(slice));
            }

            if ((slice._minIndex != 0 || slice._maxIndex != slice.Length - 1)
                && (start < slice._minIndex || slice._maxIndex < start || slice._maxIndex < start + length - 1)
            )
            {
                ThrowHelper.ThrowSliceOnRestrictedRangeException(nameof(slice));
            }
#endif

            _stride = slice.Stride;
            _buffer = slice._buffer + _stride * start;
            _length = length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _minIndex = 0;
            _maxIndex = length - 1;
            m_Safety = slice.m_Safety;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeArray<T> array)
            : this(array, 0, array.Length)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeArray<T> array, int start)
            : this(array, start, array.Length - start)
        {
        }

        public NativeSliceReadOnly(NativeArray<T> array, int start, int length)
        {
            var accessor = new NativeArrayAccessor<T>(array);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (start < 0)
            {
                ThrowHelper.ThrowSliceNegativeStartException(start);
            }

            if (length < 0)
            {
                ThrowHelper.ThrowSliceNegativeLengthException(length);
            }

            if (start + length > array.Length)
            {
                ThrowHelper.ThrowSliceRangeExceedsLengthException(array.Length, start, length, nameof(array));
            }

            if ((accessor.MinIndex != 0 || accessor.MaxIndex != accessor.Length - 1)
                && (start < accessor.MinIndex || accessor.MaxIndex < start || accessor.MaxIndex < start + length - 1)
            )
            {
                ThrowHelper.ThrowSliceOnRestrictedRangeException(nameof(array));
            }

            if (start + length < 0)
            {
                ThrowHelper.ThrowSliceIntegerOverflowException();
            }
#endif

            _stride = UnsafeUtility.SizeOf<T>();
            byte* buffer = (byte*)accessor.Buffer + _stride * start;
            _buffer = buffer;
            _length = length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _minIndex = 0;
            _maxIndex = length - 1;
            m_Safety = accessor.Safety;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeArray<T>.ReadOnly array)
            : this(array, 0, array.Length)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceReadOnly(NativeArray<T>.ReadOnly array, int start)
            : this(array, start, array.Length - start)
        {
        }

        public NativeSliceReadOnly(NativeArray<T>.ReadOnly array, int start, int length)
        {
            var accessor = new NativeArrayReadOnlyAccessor<T>(array);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (start < 0)
            {
                ThrowHelper.ThrowSliceNegativeStartException(start);
            }

            if (length < 0)
            {
                ThrowHelper.ThrowSliceNegativeLengthException(length);
            }

            if (start + length > array.Length)
            {
                ThrowHelper.ThrowSliceRangeExceedsLengthException(array.Length, start, length, nameof(array));
            }

            if (start < 0 || (accessor.Length - 1) < start || (accessor.Length - 1) < start + length - 1)
            {
                ThrowHelper.ThrowSliceOnRestrictedRangeException(nameof(array));
            }

            if (start + length < 0)
            {
                ThrowHelper.ThrowSliceIntegerOverflowException();
            }
#endif

            _stride = UnsafeUtility.SizeOf<T>();
            byte* buffer = (byte*)accessor.Buffer + _stride * start;
            _buffer = buffer;
            _length = length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _minIndex = 0;
            _maxIndex = length - 1;
            m_Safety = accessor.Safety;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSliceReadOnly<T>(NativeSlice<T> slice)
            => new(slice);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSliceReadOnly<T>(NativeArray<T> array)
            => new(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSliceReadOnly<T>(NativeArray<T>.ReadOnly array)
            => new(array);

        /// <summary>
        /// Reinterprets a NativeSliceReadOnly with a different data type (type punning).
        /// </summary>
        /// <typeparam name="U">The target data type.</typeparam>
        /// <returns>
        /// A new NativeSliceReadOnly that views the same memory, but is reinterpreted as the target type.
        /// </returns>
        public readonly NativeSliceReadOnly<U> SliceConvert<U>()
            where U : struct
        {
            var sizeofU = UnsafeUtility.SizeOf<U>();

            NativeSliceReadOnly<U> outputSlice;
            outputSlice._buffer = _buffer;
            outputSlice._stride = sizeofU;
            outputSlice._length = (_length * _stride) / sizeofU;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (_stride != UnsafeUtility.SizeOf<T>())
            {
                ThrowHelper.ThrowSliceConvertStrideMismatchException();
            }

            if (_minIndex != 0 || _maxIndex != _length - 1)
            {
                ThrowHelper.ThrowSliceConvertOnRestrictedRangeException();
            }

            if (_stride * _length % sizeofU != 0)
            {
                ThrowHelper.ThrowSliceConvertSizeMismatchException();
            }

            outputSlice._minIndex = 0;
            outputSlice._maxIndex = outputSlice._length - 1;
            outputSlice.m_Safety = m_Safety;
#endif

            return outputSlice;
        }

        public readonly NativeSliceReadOnly<U> SliceWithStride<U>(int offset)
            where U : struct
        {
            NativeSliceReadOnly<U> outputSlice;
            outputSlice._buffer = _buffer + offset;
            outputSlice._stride = _stride;
            outputSlice._length = _length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (offset < 0)
            {
                ThrowHelper.ThrowSliceWithStrideOffsetOutOfRangeException();
            }

            if (offset + UnsafeUtility.SizeOf<U>() > UnsafeUtility.SizeOf<T>())
            {
                ThrowHelper.ThrowSliceWithStrideOffsetAndSizeExceededException();
            }

            outputSlice._minIndex = _minIndex;
            outputSlice._maxIndex = _maxIndex;
            outputSlice.m_Safety = m_Safety;
#endif

            return outputSlice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSliceReadOnly<U> SliceWithStride<U>()
            where U : struct
        {
            return SliceWithStride<U>(0);
        }

        public readonly void CopyTo(NativeArray<T> array)
        {
            if (Length != array.Length)
            {
                ThrowHelper.ThrowArrayLengthMismatchException(array.Length, Length);
                return;
            }
            var sizeOf = UnsafeUtility.SizeOf<T>();
            UnsafeUtility.MemCpyStride(array.GetUnsafePtr(), sizeOf, this.GetUnsafeReadOnlyPtr(), Stride, sizeOf, _length);
        }

        public readonly void CopyTo(T[] array)
        {
            if (Length != array.Length)
            {
                ThrowHelper.ThrowArrayLengthMismatchException(array.Length, Length);
            }

            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            IntPtr addr = handle.AddrOfPinnedObject();

            var sizeOf = UnsafeUtility.SizeOf<T>();
            UnsafeUtility.MemCpyStride((byte*)addr, sizeOf, this.GetUnsafeReadOnlyPtr(), Stride, sizeOf, _length);

            handle.Free();
        }

        public readonly T[] ToArray()
        {
            T[] array = new T[Length];
            CopyTo(array);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(NativeSliceReadOnly<T> other)
            => _buffer == other._buffer && _stride == other._stride && _length == other._length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is NativeSliceReadOnly<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashValue.Combine((int)_buffer, _stride, _length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NativeSliceReadOnly<T> left, NativeSliceReadOnly<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NativeSliceReadOnly<T> left, NativeSliceReadOnly<T> right)
            => !left.Equals(right);

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private readonly NativeSliceReadOnly<T> _slice;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ref NativeSliceReadOnly<T> slice)
            {
                _slice = slice;
                _index = -1;
            }

            public readonly T Current => _slice[_index];

            readonly object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index++;
                return _index < _slice.Length;
            }

            public void Reset()
            {
                _index = -1;
            }

            public readonly void Dispose() { }
        }
    }

    internal sealed class NativeSliceReadOnlyDebugView<T>
        where T : struct
    {
        private NativeSliceReadOnly<T> _slice;

        public T[] Items => _slice.ToArray();

        public NativeSliceReadOnlyDebugView(NativeSliceReadOnly<T> slice)
        {
            _slice = slice;
        }
    }
}
