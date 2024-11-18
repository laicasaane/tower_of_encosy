using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Collections;
using UnityEngine;

namespace EncosyTower.Modules.NameKeys
{
    public partial class NameVault
    {
        private readonly Dictionary<NameHash, Id> _map;
        private readonly FasterList<string> _names;
        private readonly FasterList<Option<NameHash>> _hashes;

        public NameVault(int initialCapacity)
        {
            _map = new(initialCapacity);
            _names = new(initialCapacity);
            _hashes = new(initialCapacity);
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

        public Id NameToId(string name)
        {
            if (TryGetId(name, out var id) == false)
            {
                var result = TryAdd(name, out id);
                ThrowIfFailedRegistering(result, name, id);
            }

            return id;
        }

        public bool TryAdd(string name, out Id id)
        {
            var index = _map.Count;
            var hash = name.GetHashCode(StringComparison.Ordinal);
            id = new Id(index);

            if (_map.TryAdd(hash, id))
            {
                EnsureCapacity();
                _names.Add(name);
                _hashes[index] = new(hash);
                return true;
            }

            return default;
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

            hashes.IncreaseCapacityTo(newCapacity);
            hashes.AddReplicateNoInit(newCapacity - oldCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetId(string name, out Id id)
        {
            var hash = name.GetHashCode(StringComparison.Ordinal);
            return _map.TryGetValue(hash, out id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetName(Id id, out string name)
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

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfFailedRegistering([DoesNotReturnIf(false)] bool result, string name, Id id)
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
