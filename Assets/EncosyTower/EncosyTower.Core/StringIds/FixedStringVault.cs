#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Ids;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// The vault to store instances of <see cref="FixedString32Bytes"/>.
    /// </summary>
    /// <remarks>
    /// Due to technical limitations of <see cref="Unity.Burst.SharedStatic{T}"/>
    /// the vault can only store up to 256 unique strings.
    /// </remarks>
    public partial struct FixedStringVault
    {
        public const int MAX_CAPACITY = 256;

        private FixedHashMap<StringHash, Id, MapData> _map;
        private FixedArray<FixedString32Bytes, StringArrayData> _strings;
        private FixedArray<Option<StringHash>, HashArrayData> _hashes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedStringVault()
        {
            _map = new(default);
            _strings = new(default);
            _hashes = new(default);
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Capacity;
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

            if (_map.TryAdd(hash, id))
            {
                _strings[index] = str;
                _hashes[index] = new(hash);
                return true;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetId(in FixedString32Bytes str, out Id id)
        {
            var hash = str.GetHashCode();
            return _map.TryGetValue(hash, out id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetString(Id id, out FixedString32Bytes str)
        {
            var index = (int)(uint)id;
            var hash = _hashes[index];
            str = hash.HasValue ? _strings[index] : default;
            return hash.HasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsId(Id id)
        {
            var index = (int)(uint)id;
            var hash = _hashes[index];
            return hash.HasValue;
        }

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

        [StructLayout(LayoutKind.Explicit, Size = MAX_CAPACITY * 16)]
        private struct MapData { }

        [StructLayout(LayoutKind.Explicit, Size = MAX_CAPACITY * 32)]
        private struct StringArrayData { }

        [StructLayout(LayoutKind.Explicit, Size = MAX_CAPACITY * 8)]
        private struct HashArrayData { }
    }
}

#endif
