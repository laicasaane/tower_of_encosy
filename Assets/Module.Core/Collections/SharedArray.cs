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
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Module.Core.Collections
{
    /// <summary>
    /// An array usable as both a NativeArray and managed array
    /// </summary>
    /// <typeparam name="T">The type of the array element</typeparam>
    public sealed class SharedArray<T> : SharedArray<T, T> where T : unmanaged
    {
        public SharedArray(T[] managed)
        {
            Initialize(managed);
        }

        public SharedArray(int size)
        {
            ThrowIfSizeNegative(size);
            Initialize(new T[size]);
        }
    }

    /// <summary>
    /// An array usable as both a native and managed array
    /// </summary>
    /// <typeparam name="T">The element type in the managed representation.</typeparam>
    /// <typeparam name="TNative">The element type in the NativeArray representation. Must be the same size as <typeparamref name="T"/>.</typeparam>
    public class SharedArray<T, TNative> : IDisposable, IEnumerable<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        , IAsNativeArray<TNative>, IAsNativeSlice<TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        protected GCHandle m_GcHandle;

#if UNITY_EDITOR
        protected AtomicSafetyHandle m_SafetyHandle;
#endif

        protected T[] m_Managed;
        protected NativeArray<TNative> m_Native;
        protected int m_Version;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Managed.Length;
        }

        protected SharedArray() { }

        public SharedArray(T[] managed)
        {
            ThrowIfTypesNotEqualSize();
            Initialize(managed);
        }

        public SharedArray(int size)
        {
            ThrowIfTypesNotEqualSize();
            ThrowIfSizeNegative(size);
            Initialize(size == 0 ? Array.Empty<T>() : new T[size]);
        }

        ~SharedArray() { Dispose(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<TNative>([NotNull] SharedArray<T, TNative> self)
        {
            return self.m_Native;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T[]([NotNull] SharedArray<T, TNative> self)
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(self.m_SafetyHandle);
#endif

            return self.m_Managed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>([NotNull] SharedArray<T, TNative> self)
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(self.m_SafetyHandle);
#endif

            return self.m_Managed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>([NotNull] SharedArray<T, TNative> self)
        {
#if UNITY_EDITOR
            AtomicSafetyHandle.CheckReadAndThrow(self.m_SafetyHandle);
#endif

            return self.m_Managed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>([NotNull] SharedArray<T, TNative> self)
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(self.m_SafetyHandle);
#endif

            return self.m_Managed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>([NotNull] SharedArray<T, TNative> self)
        {
#if UNITY_EDITOR
            AtomicSafetyHandle.CheckReadAndThrow(self.m_SafetyHandle);
#endif

            return self.m_Managed;
        }

        protected void Initialize(T[] managed)
        {
            m_Version++;

            ThrowIfNull(managed);
            m_Managed = managed;
            Initialize();
        }

        void Initialize()
        {
            // Unity's default garbage collector doesn't move objects around, so pinning the array in memory
            // should not even be necessary. Better to be safe, though
            m_GcHandle = GCHandle.Alloc(m_Managed, GCHandleType.Pinned);
            CreateNativeAlias();

            unsafe void CreateNativeAlias()
            {
                // this is the trick to making a NativeArray view of a managed array (or any pointer)
                fixed (void* ptr = m_Managed)
                {
                    m_Native = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TNative>(
                        ptr, m_Managed.Length, Allocator.None
                    );
                }

#if UNITY_EDITOR
                m_SafetyHandle = AtomicSafetyHandle.Create();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref m_Native, m_SafetyHandle);
#endif
            }
        }

        /// <summary>
        /// Allows taking pointer of SharedArray in 'fixed' statements 
        /// </summary>
        /// <returns></returns>
        public ref T GetPinnableReference()
        {
            if (m_Managed.Length > 0)
            {
                return ref m_Managed[0];
            }

            return ref NullRef();
        }

        public void Resize(int newSize)
        {
            m_Version++;

            ThrowIfSizeNegative(newSize);

            if (newSize == m_Managed.Length)
            {
                return;
            }

#if UNITY_EDITOR
            AtomicSafetyHandle.CheckDeallocateAndThrow(m_SafetyHandle);
            AtomicSafetyHandle.Release(m_SafetyHandle);
#endif

            if (m_GcHandle.IsAllocated)
            {
                m_GcHandle.Free();
            }

            Array.Resize(ref m_Managed, newSize);
            Initialize();
        }

        public void Clear()
        {
#if UNITY_EDITOR
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            Array.Clear(m_Managed, 0, m_Managed.Length);
        }

        public Enumerator GetEnumerator()
        {
#if UNITY_EDITOR
            // Unlike the other safety checks, only check if it's safe to read.
            // Enumerating an array of structs gives the user copies of each element, since structs pass by value.
            // This means that the source memory can't be modified while enumerating.
            AtomicSafetyHandle.CheckReadAndThrow(m_SafetyHandle);
#endif

            return new(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            if (m_Managed == null)
            {
                return;
            }

            m_Version++;

#if UNITY_EDITOR
            AtomicSafetyHandle.CheckDeallocateAndThrow(m_SafetyHandle);
            AtomicSafetyHandle.Release(m_SafetyHandle);
#endif

            if (m_GcHandle.IsAllocated)
            {
                m_GcHandle.Free();
            }

            m_Managed = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsManagedArray()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            return m_Managed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            return m_Managed.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(m_SafetyHandle);
#endif

            return m_Managed.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

            return m_Managed.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(m_SafetyHandle);
#endif

            return m_Managed.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<TNative> AsNativeArray()
        {
            return m_Native;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice<TNative> AsNativeSlice()
        {
            return m_Native.Slice();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static ref T NullRef()
        {
            return ref *(T*)null;
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ThrowIfNull(T[] managed)
        {
            if (managed == null)
            {
                throw new ArgumentNullException(nameof(managed));
            }
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static unsafe void ThrowIfSizeNegative(int size)
        {
            if (size < 0)
            {
                throw new InvalidOperationException("size must be equal or greater than 0");
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly SharedArray<T, TNative> _sharedArray;
            private readonly int _version;
            private int _index;
            private Option<T> _current;

            public Enumerator([NotNull] SharedArray<T, TNative> sharedArray)
            {
                _sharedArray = sharedArray;
                _version = sharedArray.m_Version;
                _index = -1;
                _current = default;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current.Value();
            }

            public bool MoveNext()
            {
                var sharedArray = _sharedArray;
                var array = sharedArray.m_Managed;

                if (_version == sharedArray.m_Version && ((uint)_index < (uint)array.Length))
                {
                    _current = array[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                var sharedArray = _sharedArray;

                if (_version != sharedArray.m_Version)
                {
                    ThrowEnumFailedVersion();
                }

                _index = sharedArray.Length + 1;
                _current = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _sharedArray.m_Version)
                {
                    ThrowEnumFailedVersion();
                }

                _index = 0;
                _current = default;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _sharedArray.Length + 1)
                    {
                        ThrowEnumOpCantHappen();
                    }
                    return Current;
                }
            }

            public void Dispose()
            {
            }

            [DoesNotReturn, HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            private static void ThrowEnumFailedVersion()
            {
                throw new InvalidOperationException("SharedArray was modified during enumeration.");
            }

            [DoesNotReturn, HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            private static void ThrowEnumOpCantHappen()
            {
                throw new InvalidOperationException("Invalid enumerator state: enumeration cannot proceed.");
            }
        }
    }
}
