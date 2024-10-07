#if UNITY_BURST && UNITY_COLLECTIONS

// <copyright file="FixedArray.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#pragma warning disable IDE0044 // Add readonly modifier

using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Module.Core.Collections
{
    public unsafe struct FixedArray<T, TSize> : IAsSpan<T>, IAsReadOnlySpan<T>
        where T : unmanaged
        where TSize : unmanaged
    {
        private TSize _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedArray(TSize data)
        {
            _data = data;
        }

        public readonly int Length => sizeof(TSize) / sizeof(T);

        private readonly T* Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (void* ptr = &_data)
                {
                    return (T*)ptr;
                }
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                Checks.IndexInRange(index, Length);
                return UnsafeUtility.ReadArrayElement<T>(Buffer, Checks.BurstAssumePositive(index));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IndexInRange(index, Length);
                UnsafeUtility.WriteArrayElement(Buffer, Checks.BurstAssumePositive(index), value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
            Checks.IndexInRange(index, Length);
            return ref UnsafeUtility.ArrayElementAsRef<T>(Buffer, Checks.BurstAssumePositive(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
            => new(Buffer, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
            => new(Buffer, Length);
    }
}

#endif
