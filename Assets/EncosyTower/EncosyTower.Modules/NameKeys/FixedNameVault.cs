#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Modules.Collections;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.Modules.NameKeys
{
    public partial struct FixedNameVault
    {
        public const int MAX_CAPACITY = 256;

        private FixedHashMap<NameHash, Id, MapData> _map;
        private FixedArray<FixedString32Bytes, NameArrayData> _names;
        private FixedArray<Option<NameHash>, HashArrayData> _hashes;

        public FixedNameVault()
        {
            _map = new(default);
            _names = new(default);
            _hashes = new(default);
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Capacity;
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

            if (_map.TryAdd(hash, id))
            {
                _names[index] = name;
                _hashes[index] = new(hash);
                return true;
            }

            return default;
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

        [StructLayout(LayoutKind.Explicit, Size = MAX_CAPACITY * 16)]
        private struct MapData { }

        [StructLayout(LayoutKind.Explicit, Size = MAX_CAPACITY * 32)]
        private struct NameArrayData { }

        [StructLayout(LayoutKind.Explicit, Size = MAX_CAPACITY * 8)]
        private struct HashArrayData { }
    }
}

#endif
