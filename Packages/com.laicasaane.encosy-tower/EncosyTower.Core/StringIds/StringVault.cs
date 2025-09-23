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
        private readonly SharedArrayMap<StringHash, Id> _map;
        private readonly SharedList<UnmanagedString> _unmanagedStrings;
        private readonly FasterList<string> _managedStrings;
        private readonly SharedList<Option<StringHash>> _hashes;
        private readonly object _lock = new();
        private int _count;

        public StringVault(int initialCapacity)
        {
            _map = new(initialCapacity);
            _unmanagedStrings = new(initialCapacity);
            _managedStrings = new(initialCapacity);
            _hashes = new(initialCapacity);
            Clear();
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hashes.Count;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _map.Clear();
                _unmanagedStrings.Clear();
                _managedStrings.Clear();
                _hashes.Clear();

                // The first item represent an invalid id
                _map.Add(default, default);
                _unmanagedStrings.Add(default);
                _managedStrings.Add(string.Empty);
                _hashes.Add(default);

                _count = 1;
            }
        }

        public StringId MakeIdFromUnmanaged(in UnmanagedString str)
        {
            var hash = str.GetHashCode();
            var registered = _map.TryGetValue(hash, out var id);

            if (registered)
            {
                TryGetUnmanagedString(id, out var registeredString);

                if (str == registeredString)
                {
                    return id;
                }

                lock (_lock)
                {
                    var index = _count;
                    id = new Id(index);

                    _count += 1;
                    EnsureCapacity();

                    _unmanagedStrings[index] = str;
                    _managedStrings[index] = str.ToString();
                    _hashes[index] = new Option<StringHash>(hash);
                }
            }
            else
            {
                var index = _count;
                id = new Id(index);

                lock (_lock)
                {
                    if (_map.TryAdd(hash, id))
                    {
                        _count += 1;
                        EnsureCapacity();

                        _unmanagedStrings[index] = str;
                        _managedStrings[index] = str.ToString();
                        _hashes[index] = new Option<StringHash>(hash);
                    }
                    else
                    {
                        ThrowIfFailedRegistering(false, str, id);
                    }
                }
            }

            return id;
        }

        public StringId MakeIdFromManaged([NotNull] string str)
        {
            var hash = str.GetHashCode(StringComparison.Ordinal);
            var registered = _map.TryGetValue(hash, out var id);

            if (registered)
            {
                TryGetManagedString(id, out var registeredString);

                if (str == registeredString)
                {
                    return id;
                }

                lock (_lock)
                {
                    var index = _count;
                    id = new Id(index);

                    _count += 1;
                    EnsureCapacity();

                    _unmanagedStrings[index] = str;
                    _managedStrings[index] = str;
                    _hashes[index] = new Option<StringHash>(hash);
                }
            }
            else
            {
                lock (_lock)
                {
                    var index = _count;
                    id = new Id(index);

                    if (_map.TryAdd(hash, id))
                    {
                        _count += 1;
                        EnsureCapacity();
                        _unmanagedStrings[index] = str;
                        _managedStrings[index] = str;
                        _hashes[index] = new Option<StringHash>(hash);
                    }
                    else
                    {
                        ThrowIfFailedRegistering(false, str, id);
                    }
                }
            }

            return id;
        }

        private void EnsureCapacity()
        {
            var oldCapacity = Math.Min(_hashes.Capacity, _unmanagedStrings.Capacity);
            var newCapacity = Math.Max(_map.Capacity, _count);

            if (newCapacity > 0 && newCapacity > oldCapacity)
            {
                _hashes.IncreaseCapacityTo(newCapacity);
                _unmanagedStrings.IncreaseCapacityTo(newCapacity);
                _managedStrings.IncreaseCapacityTo(newCapacity);
            }

            if (_hashes.Count < newCapacity)
            {
                _hashes.AddReplicateNoInit(Math.Max(newCapacity - _hashes.Count, 0));
            }

            if (_unmanagedStrings.Count < newCapacity)
            {
                _unmanagedStrings.AddReplicateNoInit(Math.Max(newCapacity - _unmanagedStrings.Count, 0));
            }

            if (_managedStrings.Count < newCapacity)
            {
                _managedStrings.AddReplicateNoInit(Math.Max(newCapacity - _managedStrings.Count, 0));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetUnmanagedString(Id id, out UnmanagedString str)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;

            str = validIndex ? _unmanagedStrings[index] : default;
            return validIndex ? _hashes[index].HasValue : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetManagedString(Id id, out string str)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;

            str = validIndex ? _managedStrings[index] : string.Empty;
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

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfFailedRegistering([DoesNotReturnIf(false)] bool result, in UnmanagedString str, Id id)
        {
            if (result == false)
            {
                throw new InvalidOperationException(
                    $"Cannot register a StringId by the same value \"{str}\" with different id \"{id}\"."
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
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
