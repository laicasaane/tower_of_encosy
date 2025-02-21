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
        private int _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedStringVault()
        {
            _map = new(default);
            _strings = new(default);
            _hashes = new(default);
            _count = 0;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MAX_CAPACITY;
        }

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public Id MakeId(in FixedString32Bytes str)
        {
            var hash = str.GetHashCode();
            var registered = _map.TryGetValue(hash, out var id);

            if (registered)
            {
                TryGetString(id, out var registeredString);

                if (str == registeredString)
                {
                    return id;
                }

                var index = _count;
                id = new Id(index);

                _strings[index] = str;
                _hashes[index] = new(hash);
                _count += 1;
            }
            else
            {
                var index = _count;
                id = new Id(index);

                if (_map.TryAdd(hash, id))
                {
                    _strings[index] = str;
                    _hashes[index] = new(hash);
                    _count += 1;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    ThrowIfFailedRegistering(false, str, id);
                }
#endif
            }

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetString(Id id, out FixedString32Bytes str)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Length;

            str = validIndex ? _strings[index] : default;
            return validIndex ? _hashes[index].HasValue : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsId(Id id)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Length;
            return validIndex ? _hashes[index].HasValue : false;
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
