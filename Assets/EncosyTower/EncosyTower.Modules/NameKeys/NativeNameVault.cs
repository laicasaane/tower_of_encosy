#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.Modules.NameKeys
{
    public partial struct NativeNameVault
    {
        private NativeHashMap<NameHash, Id> _map;
        private NativeList<FixedString32Bytes> _names;
        private NativeList<Option<NameHash>> _hashes;

        public NativeNameVault(int initialCapacity, Allocator allocator)
        {
            _map = new(initialCapacity, allocator);
            _names = new(initialCapacity, allocator);
            _hashes = new(initialCapacity, allocator);
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _names.Capacity;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Count;
        }

        public Id NameToId(in FixedString32Bytes name)
        {
            if (TryGetId(name, out var id) == false)
            {
                var result = TryAdd(name, out id);
                ThrowIfFailedRegistering(result, name, id);
            }

            return id;
        }

        public bool TryAdd(in FixedString32Bytes name, out Id id)
        {
            var index = _map.Count;
            var hash = name.GetHashCode();
            id = new Id(index);

            if (_map.TryAdd(hash, id) == false)
            {
                return default;
            }

            EnsureCapacity();
            _names.Add(name);

            ref var elem = ref _hashes.ElementAt(index);
            elem = new(hash);

            return true;
        }

        private void EnsureCapacity()
        {
            var hashes = _hashes;
            var oldCapacity = hashes.Capacity;
            var newCapacity = _names.Capacity;

            if (newCapacity <= oldCapacity)
            {
                return;
            }

            hashes.SetCapacity(newCapacity);
            hashes.AddReplicate(default, newCapacity - oldCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetId(in FixedString32Bytes name, out Id id)
        {
            var hash = name.GetHashCode();
            return _map.TryGetValue(hash, out id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetName(Id id, out FixedString32Bytes name)
        {
            var index = (int)(uint)id;
            var hash = _hashes[index];
            name = hash.HasValue ? _names[index] : default;
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
            , in FixedString32Bytes name
            , Id id
        )
        {
            if (result == false)
            {
                throw new InvalidOperationException(
                    $"Cannot register a NameKey by the same name \"{name}\" with different id \"{id}\"."
                );
            }
        }
    }
}

#endif
