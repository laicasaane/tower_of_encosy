#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Ids;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public partial struct NativeStringVault
    {
        private NativeHashMap<StringHash, Id> _map;
        private NativeList<FixedString32Bytes> _strings;
        private NativeList<Option<StringHash>> _hashes;

        public NativeStringVault(int initialCapacity, Allocator allocator)
        {
            _map = new(initialCapacity, allocator);
            _strings = new(initialCapacity, allocator);
            _hashes = new(initialCapacity, allocator);
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _strings.Capacity;
        }

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Count;
        }

        public Id MakeId(in FixedString32Bytes str)
        {
            if (TryGetId(str, out var id) == false)
            {
                var result = TryAdd(str, out id);
                ThrowIfFailedRegistering(result, str, id);
            }

            return id;
        }

        public bool TryAdd(in FixedString32Bytes str, out Id id)
        {
            var index = _map.Count;
            var hash = str.GetHashCode();
            id = new Id(index);

            if (_map.TryAdd(hash, id) == false)
            {
                return default;
            }

            EnsureCapacity();
            _strings.Add(str);

            ref var elem = ref _hashes.ElementAt(index);
            elem = new(hash);

            return true;
        }

        private readonly void EnsureCapacity()
        {
            var hashes = _hashes;
            var oldCapacity = hashes.Capacity;
            var newCapacity = _strings.Capacity;

            if (newCapacity <= oldCapacity)
            {
                return;
            }

            hashes.SetCapacity(newCapacity);
            hashes.AddReplicate(default, newCapacity - oldCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetId(in FixedString32Bytes str, out Id id)
        {
            var hash = str.GetHashCode();
            return _map.TryGetValue(hash, out id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetString(Id id, out FixedString32Bytes str)
        {
            var index = (int)(uint)id;
            var hash = _hashes[index];
            str = hash.HasValue ? _strings[index] : default;
            return hash.HasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsId(Id id)
        {
            var index = (int)(uint)id;
            var hash = _hashes[index];
            return hash.HasValue;
        }

        [BurstDiscard]
        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfFailedRegistering(
              [DoesNotReturnIf(false)] bool result
            , in FixedString32Bytes str
            , Id id
        )
        {
            if (result == false)
            {
                throw new InvalidOperationException(
                    $"Cannot register a StringId by the same value \"{str}\" with different id \"{id}\"."
                );
            }
        }
    }
}

#endif
