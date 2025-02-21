using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Ids;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public partial class StringVault
    {
        private readonly Dictionary<StringHash, Id> _map;
        private readonly FasterList<string> _strings;
        private readonly FasterList<Option<StringHash>> _hashes;

        public StringVault(int initialCapacity)
        {
            _map = new(initialCapacity);
            _strings = new(initialCapacity);
            _hashes = new(initialCapacity);
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _strings.Capacity;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Count;
        }

        public Id MakeId(string str)
        {
            if (TryGetId(str, out var id) == false)
            {
                var result = TryAdd(str, out id);
                ThrowIfFailedRegistering(result, str, id);
            }

            return id;
        }

        public bool TryAdd(string str, out Id id)
        {
            var index = _map.Count;
            var hash = str.GetHashCode(StringComparison.Ordinal);
            id = new Id(index);

            if (_map.TryAdd(hash, id))
            {
                EnsureCapacity();
                _strings.Add(str);
                _hashes[index] = new(hash);
                return true;
            }

            return default;
        }

        private void EnsureCapacity()
        {
            var hashes = _hashes;
            var oldCapacity = hashes.Capacity;
            var newCapacity = _strings.Capacity;

            if (newCapacity <= oldCapacity)
            {
                return;
            }

            hashes.IncreaseCapacityTo(newCapacity);
            hashes.AddReplicateNoInit(newCapacity - oldCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetId(string str, out Id id)
        {
            var hash = str.GetHashCode(StringComparison.Ordinal);
            return _map.TryGetValue(hash, out id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetString(Id id, out string str)
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

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfFailedRegistering([DoesNotReturnIf(false)] bool result, string str, Id id)
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
