using System;
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
        private readonly ArrayMap<StringHash, Id> _map;
        private readonly FasterList<string> _strings;
        private readonly FasterList<Option<StringHash>> _hashes;
        private int _count;

        public StringVault(int initialCapacity)
        {
            _map = new(initialCapacity);
            _strings = new(initialCapacity);
            _hashes = new(initialCapacity);
            _count = 0;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _strings.Count;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public Id MakeId([NotNull] string str)
        {
            var hash = str.GetHashCode(StringComparison.Ordinal);
            var registered = _map.TryGetValue(hash, out var id);

            if (registered)
            {
                TryGetString(id, out var registeredString);

                if (string.Equals(str, registeredString, StringComparison.Ordinal))
                {
                    return id;
                }

                var index = _count;
                id = new Id(index);

                _count += 1;
                EnsureCapacity();

                _strings[index] = str;
                _hashes[index] = new Option<StringHash>(hash);
            }
            else
            {
                var index = _count;
                id = new Id(index);

                if (_map.TryAdd(hash, id))
                {
                    _count += 1;
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

        private void EnsureCapacity()
        {
            var oldCapacity = Math.Min(_hashes.Capacity, _strings.Capacity);
            var newCapacity = Math.Max(_map.Capacity, _count);

            if (newCapacity <= oldCapacity || newCapacity < 1)
            {
                return;
            }

            _hashes.IncreaseCapacityTo(newCapacity);
            _hashes.AddReplicateNoInit(Math.Max(newCapacity - _hashes.Count, 0));

            _strings.IncreaseCapacityTo(newCapacity);
            _strings.AddReplicateNoInit(Math.Max(newCapacity - _strings.Count, 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetString(Id id, out string str)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;

            str = validIndex ? _strings[index] : default;
            return validIndex ? _hashes[index].HasValue : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsId(Id id)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;
            return validIndex ? _hashes[index].HasValue : false;
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
