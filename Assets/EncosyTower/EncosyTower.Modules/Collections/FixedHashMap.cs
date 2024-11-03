#if UNITY_BURST && UNITY_COLLECTIONS

// <copyright file="FixedHashMap.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Modules.Collections
{
    public unsafe struct FixedHashMap<TKey, TValue, TCapacity>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TCapacity : unmanaged
    {
        private readonly TCapacity _data;
        private readonly int _sizeOfValue;

        public FixedHashMap(TCapacity data)
        {
            _data = data;
            _sizeOfValue = sizeof(TValue);

            Count = 0;
            Capacity = CalcCapacity();

            var totalHashesSizeInBytes = UnsafeUtility.SizeOf<uint>() * Capacity;
            UnsafeUtility.MemSet(Ptr, 0xff, totalHashesSizeInBytes);
        }

        public int Capacity { get; }

        public int Count { get; private set; }

        private uint* Ptr
        {
            get
            {
                fixed (TCapacity* key = &_data)
                {
                    return (uint*)key;
                }
            }
        }

        private int* NumItems => (int*)(Ptr + Capacity);

        private TKey* Keys => (TKey*)(Ptr + (Capacity * 2));

        public bool TryAdd(TKey key, TValue item)
        {
            var index = TryAdd(key);
            if (index != -1)
            {
                GetElementAt(index) = item;
                return true;
            }

            return false;
        }

        [Pure]
        public bool TryGetValue(TKey key, out TValue item)
        {
            var index = Find(key);

            if (index != -1)
            {
                item = GetElementAt(index);
                return true;
            }

            item = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalcCapacity()
        {
            var sizeOfElement = sizeof(uint) + sizeof(uint) + sizeof(TValue) + sizeof(TKey);
            var capacity = sizeof(TCapacity) / sizeOfElement;
            return capacity;
        }

        private int TryAdd(in TKey key)
        {
            var hash = Hash(key);
            var firstIndex = GetFirstIndex(hash);
            var index = firstIndex;

            do
            {
                var current = Ptr[index];

                if (current == uint.MaxValue)
                {
                    Ptr[index] = hash;
                    GetKeyAt(index) = key;
                    Count++;
                    NumItems[firstIndex] += 1;

                    return index;
                }

                if ((current == hash) && GetKeyAt(index).Equals(key))
                {
                    return -1;
                }

                index = (index + 1) % Capacity;
            }
            while (index != firstIndex);

            return -1;
        }

        private int Find(in TKey key)
        {
            var hash = Hash(key);
            var firstIndex = GetFirstIndex(hash);
            var num = NumItems[firstIndex];
            var index = firstIndex;

            do
            {
                if (num == 0)
                {
                    return -1;
                }

                if (Ptr[index] == hash)
                {
                    if (GetKeyAt(index).Equals(key))
                    {
                        return index;
                    }

                    num--;
                }

                index = (index + 1) % Capacity;
            }
            while (index != firstIndex);

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Hash(in TKey key)
        {
            var hash = (uint)key.GetHashCode();
            return hash == uint.MaxValue ? 0 : hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetFirstIndex(uint hash)
        {
            return (int)(hash % (uint)Capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref TKey GetKeyAt(int index)
        {
            return ref Keys[index];
        }

        [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
        private ref TValue GetElementAt(int index)
        {
            return ref *(TValue*)GetElementAt(Ptr, Capacity, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void* GetElementAt(void* src, int capacity, int index)
        {
            var ptr = (byte*)src;
            ptr += capacity * (sizeof(uint) + sizeof(int) + sizeof(TKey));
            ptr += index * _sizeOfValue;

            return ptr;
        }
    }
}

#endif
