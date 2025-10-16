#if UNITY_COLLECTIONS && UNITY_MATHEMATICS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using EncosyTower.Ids;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public readonly partial struct NativeStringVault
    {
        internal readonly NativeHashMap<StringHash, Id> _map;
        internal readonly NativeList<UnmanagedString> _strings;
        internal readonly NativeList<Option<StringHash>> _hashes;
        internal readonly NativeReference<int> _count;

        public NativeStringVault(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            _map = new(initialCapacity, allocator);
            _strings = new(initialCapacity, allocator);
            _hashes = new(initialCapacity, allocator);
            _count = new(allocator);
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hashes.Length;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count.Value;
        }

        public void Clear()
        {
            _map.Clear();
            _strings.Clear();
            _hashes.Clear();

            // The first item represent an invalid id
            _map.Add(default, default);
            _strings.Add(default);
            _hashes.Add(default);
            _count.ValueAsUnsafeRefRW() = 1;
        }

        public Id MakeIdFrom(in UnmanagedString str)
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

                ref var count = ref _count.ValueAsUnsafeRefRW();
                var index = count;

                id = new Id(index);

                count += 1;
                EnsureCapacity();

                _strings.ElementAsUnsafeRefRW(index) = str;
                _hashes.ElementAsUnsafeRefRW(index) = Option.Some<StringHash>(hash);
            }
            else
            {
                ref var count = ref _count.ValueAsUnsafeRefRW();
                var index = count;

                id = new Id(index);

                if (_map.TryAdd(hash, id))
                {
                    count += 1;
                    EnsureCapacity();

                    _strings.ElementAsUnsafeRefRW(index) = str;
                    _hashes.ElementAsUnsafeRefRW(index) = Option.Some<StringHash>(hash);
                }
                else
                {
                    ThrowIfFailedRegistering(false, str, id);
                }
            }

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetString(Id id, out UnmanagedString result)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Length;

            result = validIndex ? _strings[index] : default;
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

        private void EnsureCapacity()
        {
            var oldCapacity = math.min(_hashes.Capacity, _strings.Capacity);
            var newCapacity = math.max(_map.Capacity, _count.Value);

            if (newCapacity > 0 && newCapacity > oldCapacity)
            {
                _hashes.SetCapacity(newCapacity);
                _strings.SetCapacity(newCapacity);
            }

            if (_hashes.Length < newCapacity)
            {
                _hashes.ResizeUninitialized(math.max(newCapacity - _hashes.Length, 0));
            }

            if (_strings.Length < newCapacity)
            {
                _strings.ResizeUninitialized(math.max(newCapacity - _strings.Length, 0));
            }
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
    }

    partial struct NativeStringVault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly struct ReadOnly
        {
            internal readonly NativeHashMap<StringHash, Id>.ReadOnly _map;
            internal readonly NativeArray<UnmanagedString>.ReadOnly _strings;
            internal readonly NativeArray<Option<StringHash>>.ReadOnly _hashes;
            internal readonly int _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(in NativeStringVault vault)
            {
                _map = vault._map.AsReadOnly();
                _strings = vault._strings.AsReadOnly();
                _hashes = vault._hashes.AsReadOnly();
                _count = vault._count.Value;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashes.Length;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _count;
            }

            public bool TryGetId(in UnmanagedString str, out Id result)
            {
                var hash = str.GetHashCode();
                var registered = _map.TryGetValue(hash, out var id);

                if (registered && TryGetString(id, out var registeredString) && str == registeredString)
                {
                    result = id;
                    return true;
                }

                result = default;
                return false;
            }

            public bool TryGetString(Id id, out UnmanagedString result)
            {
                var indexUnsigned = (uint)id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Length;

                result = validIndex ? _strings[index] : default;
                return validIndex ? _hashes[index].HasValue : false;
            }

            public bool ContainsId(Id id)
            {
                var indexUnsigned = (uint)id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Length;
                return validIndex ? _hashes[index].HasValue : false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(in NativeStringVault vault)
                => new(vault);
        }
    }
}

#endif
