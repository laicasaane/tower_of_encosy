#pragma warning disable IDE1006 // Naming Styles

// https://github.com/stella3d/SharedReference

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace EncosyTower.Collections
{
    /// <summary>
    /// An array of length 1, usable as both a NativeArray and managed array
    /// </summary>
    /// <typeparam name="T">The type of the array element</typeparam>
    public sealed class SharedReference<T> : SharedReference<T, T>
        where T : unmanaged
    {
        public SharedReference() : base()
        {
        }

        public SharedReference(T value) : base(value)
        {
        }
    }

    /// <summary>
    /// An array of length 1, usable as both a native and managed array
    /// </summary>
    /// <typeparam name="T">The element type in the managed representation.</typeparam>
    /// <typeparam name="TNative">The element type in the NativeArray representation. Must be the same size as <typeparamref name="T"/>.</typeparam>
    public class SharedReference<T, TNative> : IDisposable
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        , IAsNativeArray<TNative>, IAsNativeSlice<TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        protected GCHandle gcHandle;

#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
        // ReSharper disable once InconsistentNaming
        protected AtomicSafetyHandle m_SafetyHandle;
#endif

        protected T[] managed;
        protected NativeArray<TNative> native;
        protected int version;

        protected SharedReference()
        {
            ThrowIfTypesNotEqualSize();
            Initialize(default);
        }

        public SharedReference(T value)
        {
            ThrowIfTypesNotEqualSize();
            Initialize(value);
        }

        ~SharedReference()
        {
            version++;
            Dispose();
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => managed.Length;
        }

        public ref T ValueRW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
                AtomicSafetyHandle.CheckWriteAndThrow(m_SafetyHandle);
#endif

                version++;
                return ref managed[0];
            }
        }

        public ref readonly T ValueRO
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR && !DISABLE_SHAREDARRAY_SAFETY
                AtomicSafetyHandle.CheckReadAndThrow(m_SafetyHandle);
#endif

                return ref managed[0];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T[]([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsReadOnlyMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsSpanNative();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsReadOnlySpanNative();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsNativeArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            return self.AsNativeSlice();
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
        public Span<TNative> AsSpanNative()
        {
            return native.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TNative> AsReadOnlySpanNative()
        {
            return native.AsReadOnlySpan();
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

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ThrowIfSizeNegative(int size)
        {
            if (size < 0)
            {
                throw new InvalidOperationException("size must be equal or greater than 0");
            }
        }

        private void Initialize(T value)
        {
            version++;

            this.managed = new T[] { value };

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
    }
}
