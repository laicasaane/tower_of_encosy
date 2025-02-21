#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Ids;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public partial struct NativeStringVault : IDisposable
    {
        private NativeHashMap<StringHash, Id> _map;
        private NativeList<FixedString32Bytes> _strings;
        private NativeList<Option<StringHash>> _hashes;
        private NativeReference<int> _count;

        public NativeStringVault(int initialCapacity, Allocator allocator)
        {
            _map = new(initialCapacity, allocator);
            _strings = new(initialCapacity, allocator);
            _hashes = new(initialCapacity, allocator);
            _count = new(allocator);
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.IsCreated;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _strings.Length;
        }

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count.Value;
        }

        public void Dispose()
        {
            _map.Dispose();
            _strings.Dispose();
            _hashes.Dispose();
            _count.Dispose();
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

                var index = _count.Value;
                id = new Id(index);

                _count.Value += 1;
                EnsureCapacity();

                _strings[index] = str;
                _hashes[index] = new Option<StringHash>(hash);
            }
            else
            {
                var index = _count.Value;
                id = new Id(index);

                if (_map.TryAdd(hash, id))
                {
                    _count.Value += 1;
                    EnsureCapacity();

                    _strings[index] = str;
                    _hashes[index] = new Option<StringHash>(hash);
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

        private readonly void EnsureCapacity()
        {
            var oldCapacity = math.min(_hashes.Capacity, _strings.Capacity);
            var newCapacity = math.max(_map.Capacity, _count.Value);

            if (newCapacity <= oldCapacity || newCapacity < 1)
            {
                return;
            }

            _hashes.SetCapacity(newCapacity);
            _hashes.AddReplicate(default, math.max(newCapacity - _hashes.Length, 0));

            _strings.SetCapacity(newCapacity);
            _strings.AddReplicate(default, math.max(newCapacity - _strings.Length, 0));
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
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Length;

            str = validIndex ? _strings[index] : default;
            return validIndex ? _hashes[index].HasValue : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsId(Id id)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Length;
            return validIndex ? _hashes[index].HasValue : false;
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
