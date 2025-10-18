using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Ids;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public sealed partial class StringVault : IDisposable
    {
        internal readonly SharedArrayMap<StringHash, Id> _map;
        internal readonly SharedList<UnmanagedString> _unmanagedStrings;
        internal readonly FasterList<string> _managedStrings;
        internal readonly SharedList<Option<StringHash>> _hashes;
        internal readonly SharedReference<int> _count;
        internal readonly object _lock = new();

        public StringVault(int initialCapacity)
        {
            _map = new(initialCapacity);
            _unmanagedStrings = new(initialCapacity);
            _managedStrings = new(initialCapacity);
            _hashes = new(initialCapacity);
            _count = new();
            Clear();
        }

        public StringVault(ReadOnlySpan<string> managedStrings) : this(managedStrings.Length)
        {
            foreach (var str in managedStrings)
            {
                MakeIdFromManaged(str);
            }
        }

        public StringVault(ReadOnlySpan<UnmanagedString> unmanagedStrings) : this(unmanagedStrings.Length)
        {
            foreach (var str in unmanagedStrings)
            {
                MakeIdFromUnmanaged(str);
            }
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hashes.Count;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count.ValueRO;
        }

        public SharedList<UnmanagedString>.ReadOnly UnmanagedStrings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _unmanagedStrings.AsReadOnly();
        }

        public FasterList<string>.ReadOnly ManagedStrings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _managedStrings.AsReadOnly();
        }

        public void Dispose()
        {
            _map.Dispose();
            _unmanagedStrings.Dispose();
            _hashes.Dispose();
            _count.Dispose();
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

                _count.ValueRW = 1;
            }
        }

        /// <summary>
        /// Creates or retrieves a <see cref="StringId"/> from a managed <see cref="string"/>.
        /// </summary>
        /// <param name="str">
        /// The managed string to create or retrieve the <see cref="StringId"/> for.
        /// Should have a maximum length of 125 UTF8 characters.
        /// </param>
        /// <returns></returns>
        public Id MakeIdFromUnmanaged(in UnmanagedString str)
        {
            var hash = str.ToHashCode();
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
                    ref var count = ref _count.ValueRW;
                    var index = count;

                    id = new Id(index);

                    count += 1;
                    EnsureCapacity();

                    _unmanagedStrings[index] = str;
                    _managedStrings[index] = str.ToString();
                    _hashes[index] = Option.Some<StringHash>(hash);
                }
            }
            else
            {
                ref var count = ref _count.ValueRW;
                var index = count;

                id = new Id(index);

                lock (_lock)
                {
                    if (_map.TryAdd(hash, id))
                    {
                        count += 1;
                        EnsureCapacity();

                        _unmanagedStrings[index] = str;
                        _managedStrings[index] = str.ToString();
                        _hashes[index] = Option.Some<StringHash>(hash);
                    }
                    else
                    {
                        ThrowIfFailedRegistering(false, str, id);
                    }
                }
            }

            return id;
        }

        /// <summary>
        /// Creates or retrieves a <see cref="StringId"/> from a managed <see cref="string"/>.
        /// </summary>
        /// <param name="str">
        /// The managed string to create or retrieve the <see cref="StringId"/> for.
        /// Should have a maximum length of 125 UTF8 characters.
        /// </param>
        /// <returns></returns>
        public StringId MakeIdFromManaged([NotNull] string str)
        {
            var hash = HashValue64.FNV1a(str);
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
                    ref var count = ref _count.ValueRW;
                    var index = count;

                    id = new Id(index);

                    count += 1;
                    EnsureCapacity();

                    _unmanagedStrings[index] = str;
                    _managedStrings[index] = str;
                    _hashes[index] = Option.Some<StringHash>(hash);
                }
            }
            else
            {
                lock (_lock)
                {
                    ref var count = ref _count.ValueRW;
                    var index = count;

                    id = new Id(index);

                    if (_map.TryAdd(hash, id))
                    {
                        count += 1;
                        EnsureCapacity();
                        _unmanagedStrings[index] = str;
                        _managedStrings[index] = str;
                        _hashes[index] = Option.Some<StringHash>(hash);
                    }
                    else
                    {
                        ThrowIfFailedRegistering(false, str, id);
                    }
                }
            }

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetUnmanagedString(Id id, out UnmanagedString result)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;

            result = validIndex ? _unmanagedStrings[index] : default;
            return validIndex ? _hashes[index].HasValue : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetManagedString(Id id, out string result)
        {
            var indexUnsigned = (uint)id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;

            result = validIndex ? _managedStrings[index] : string.Empty;
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

        private void EnsureCapacity()
        {
            var oldCapacity = Math.Min(_hashes.Capacity, _unmanagedStrings.Capacity);
            var newCapacity = Math.Max(_map.Capacity, _count.ValueRO);

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

    partial class StringVault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly partial struct ReadOnly
        {
            private readonly SharedArrayMapNative<StringHash, Id, Id>.ReadOnly _map;
            private readonly SharedListNative<UnmanagedString, UnmanagedString>.ReadOnly _unmanagedStrings;
            private readonly SharedListNative<Option<StringHash>, Option<StringHash>>.ReadOnly _hashes;
            private readonly NativeArray<int>.ReadOnly _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(StringVault vault)
            {
                _map = vault._map.AsNative();
                _unmanagedStrings = vault._unmanagedStrings.AsNative();
                _hashes = vault._hashes.AsNative();
                _count = vault._count.AsNativeArray().AsReadOnly();
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.IsCreated && _unmanagedStrings.IsCreated
                    && _hashes.IsCreated && _count.IsCreated;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashes.Count;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _count[0];
            }

            public bool TryGetId(in UnmanagedString str, out Id result)
            {
                var hash = str.ToHashCode();
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
                var validIndex = indexUnsigned < (uint)_hashes.Count;

                result = validIndex ? _unmanagedStrings[index] : default;
                return validIndex ? _hashes[index].HasValue : false;
            }

            public bool ContainsId(Id id)
            {
                var indexUnsigned = (uint)id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Count;
                return validIndex ? _hashes[index].HasValue : false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(StringVault vault)
                => new(vault);
        }
    }
}
