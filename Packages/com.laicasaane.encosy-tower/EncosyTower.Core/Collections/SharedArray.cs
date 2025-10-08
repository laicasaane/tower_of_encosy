#pragma warning disable IDE1006 // Naming Styles

// https://github.com/stella3d/SharedArray

// MIT License
//
// Copyright(c) 2020 Stella Cannefax
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
// OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Common;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace EncosyTower.Collections
{
    /// <summary>
    /// An array usable as both a NativeArray and managed array
    /// </summary>
    /// <typeparam name="T">The type of the array element</typeparam>
    public sealed class SharedArray<T> : SharedArray<T, T>
        where T : unmanaged
    {
        public SharedArray(int size) : base(size)
        {
        }

        public SharedArray([NotNull] T[] source) : base(source)
        {
        }

        public SharedArray(in ArraySegment<T> source) : base(source)
        {
        }

        public SharedArray(in ReadOnlySpan<T> source) : base(source)
        {
        }

        public SharedArray([NotNull] ICollection<T> source) : base(source)
        {
        }

        public SharedArray([NotNull] ICollection<T> source, int extraSize) : base(source, extraSize)
        {
        }
    }

    /// <summary>
    /// An array usable as both a native and managed array
    /// </summary>
    /// <typeparam name="T">The element type in the managed representation.</typeparam>
    /// <typeparam name="TNative">The element type in the NativeArray representation. Must be the same size as <typeparamref name="T"/>.</typeparam>
    public class SharedArray<T, TNative> : IDisposable, IClearable, IEnumerable<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        , IAsNativeArray<TNative>, IAsNativeSlice<TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        protected GCHandle gcHandle;

#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
        protected AtomicSafetyHandle m_SafetyHandle;
#endif

        protected T[] managed;
        protected NativeArray<TNative> native;
        protected int version;

        protected SharedArray()
        {
            ThrowIfTypesNotEqualSize();
            Initialize(Array.Empty<T>());
        }

        public SharedArray(int size)
        {
            ThrowIfTypesNotEqualSize();
            ThrowIfSizeNegative(size);
            Initialize(size == 0 ? Array.Empty<T>() : new T[size]);
        }

        public SharedArray([NotNull] T[] source)
        {
            ThrowIfTypesNotEqualSize();
            Initialize(source);
        }

        public SharedArray(in ArraySegment<T> source) : this(source.ToArray())
        {
        }

        public SharedArray(in ReadOnlySpan<T> source) : this(source.ToArray())
        {
        }

        public SharedArray(in NativeArray<TNative> source)
        {
            ThrowIfTypesNotEqualSize();

            var managed = new T[source.Length];
            Initialize(managed);
            AsNativeArray().CopyFrom(source);
        }

        public SharedArray(in NativeSlice<TNative> source)
        {
            ThrowIfTypesNotEqualSize();

            var managed = new T[source.Length];
            Initialize(managed);
            source.CopyTo(AsNativeArray());
        }

        public SharedArray([NotNull] ICollection<T> source)
        {
            ThrowIfTypesNotEqualSize();

            var managed = new T[source.Count];
            source.CopyTo(managed, 0);

            Initialize(managed);
        }

        public SharedArray([NotNull] ICollection<T> source, int extraSize)
        {
            ThrowIfTypesNotEqualSize();

            var managed = new T[source.Count + extraSize];
            source.CopyTo(managed, 0);

            Initialize(managed);
        }

        ~SharedArray()
        {
            Dispose();
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => managed.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T[]([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<T>([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsArraySegment();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsReadOnlyMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<TNative>([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsNativeArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<TNative>([NotNull] SharedArray<T, TNative> self)
        {
            return self.AsNativeSlice();
        }

        /// <summary>
        /// Allows taking pointer of SharedArray in 'fixed' statements
        /// </summary>
        /// <returns></returns>
        public ref T GetPinnableReference()
        {
            version++;

            if (managed.Length > 0)
            {
                return ref managed[0];
            }

            return ref NullRef();
        }

        public void Resize(int newSize, bool copyContent = true)
        {
            version++;

            ThrowIfSizeNegative(newSize);

            if (newSize == managed.Length)
            {
                return;
            }

#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckDeallocateAndThrow(m_SafetyHandle);
            AtomicSafetyHandle.Release(m_SafetyHandle);
#endif

            if (gcHandle.IsAllocated)
            {
                gcHandle.Free();
            }

            if (copyContent)
            {
                Array.Resize(ref managed, newSize);
            }
            else
            {
                managed = new T[newSize];
            }

            Initialize();
        }

        public void Clear()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            Array.Clear(managed, 0, managed.Length);
        }

        public Enumerator GetEnumerator()
        {
            return new(this);
        }

        public void Dispose()
        {
            if (managed == null)
            {
                return;
            }

            version++;

#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckDeallocateAndThrow(m_SafetyHandle);
            AtomicSafetyHandle.Release(m_SafetyHandle);
#endif

            if (gcHandle.IsAllocated)
            {
                gcHandle.Free();
            }

            managed = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsManagedArray()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            return managed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<T> AsArraySegment()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            return managed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            return managed.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(m_SafetyHandle);
#endif

            return managed.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            return managed.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(m_SafetyHandle);
#endif

            return managed.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<TNative> AsNativeArray()
        {
            return native;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice<TNative> AsNativeSlice()
        {
            return native.Slice();
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static unsafe void ThrowIfTypesNotEqualSize()
        {
            if (sizeof(T) != sizeof(TNative))
            {
                throw new InvalidOperationException(
                    $"size of native alias type '{typeof(TNative).FullName}' ({sizeof(TNative)} bytes) " +
                    $"must be equal to size of source type '{typeof(T).FullName}' ({sizeof(T)} bytes)"
                );
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ThrowIfSizeNegative(int size)
        {
            if (size < 0)
            {
                throw new InvalidOperationException("size must be equal or greater than 0");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static ref T NullRef()
        {
            return ref *(T*)null;
        }

        private void Initialize(T[] managed)
        {
            version++;

            this.managed = managed;
            Initialize();
        }

        private void Initialize()
        {
            // Unity's default garbage collector doesn't move objects around, so pinning the array in memory
            // should not even be necessary. Better to be safe, though
            gcHandle = GCHandle.Alloc(managed, GCHandleType.Pinned);
            CreateNativeAlias();

            unsafe void CreateNativeAlias()
            {
                // this is the trick to making a NativeArray view of a managed array (or any pointer)
                fixed (void* ptr = managed)
                {
                    native = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TNative>(
                        ptr, managed.Length, Allocator.None
                    );
                }

#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
                m_SafetyHandle = AtomicSafetyHandle.Create();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref native, m_SafetyHandle);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly SharedArray<T, TNative> _sharedArray;
            private readonly int _version;
            private readonly int _length;
            private int _index;
            private Option<T> _current;

            public Enumerator([NotNull] SharedArray<T, TNative> sharedArray)
            {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
                // Unlike the other safety checks, only check if it's safe to read.
                // Enumerating an array of structs gives the user copies of each element, since structs pass by value.
                // This means that the source memory can't be modified while enumerating.
                AtomicSafetyHandle.CheckReadAndThrow(sharedArray.m_SafetyHandle);
#endif

                _sharedArray = sharedArray;
                _version = sharedArray.version;
                _length = sharedArray.Length;
                _index = -1;
                _current = Option.None;
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current.GetValueOrThrow();
            }

            public bool MoveNext()
            {
                var sharedArray = _sharedArray;
                var array = sharedArray.managed;

                if (_version == sharedArray.version && ((uint)_index < (uint)_length))
                {
                    _current = array[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _sharedArray.version)
                {
                    ThrowEnumFailedVersion();
                }

                _index = _length + 1;
                _current = Option.None;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _sharedArray.version)
                {
                    ThrowEnumFailedVersion();
                }

                _index = 0;
                _current = Option.None;
            }

            readonly object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _length + 1)
                    {
                        ThrowEnumOpCantHappen();
                    }

                    return Current;
                }
            }

            public readonly void Dispose()
            {
            }

            [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            private static void ThrowEnumFailedVersion()
            {
                throw new InvalidOperationException("SharedArray was modified during enumeration.");
            }

            [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            private static void ThrowEnumOpCantHappen()
            {
                throw new InvalidOperationException("Invalid enumerator state: enumeration cannot proceed.");
            }
        }
    }
}
