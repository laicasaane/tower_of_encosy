using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Debugging;
using EncosyTower.Ids;
using UnityEngine;

namespace EncosyTower.StringIds
{
    public sealed partial class StringVault : IDisposable, IClearable, IIncreaseCapacity
        , IReadOnlyList<string>
        , ICopyToSpan<string>, ITryCopyToSpan<string>
        , ICopyToSpan<UnmanagedString>, ITryCopyToSpan<UnmanagedString>
    {
        /// <summary>
        /// The default instance used by <see cref="StringToId"/> and <see cref="IdToString"/>.
        /// </summary>
        public static StringVault Default => GlobalStringVault.s_vault;

        internal readonly SharedArrayMap<StringHash, StringId> _map;
        internal readonly SharedList<Range> _unmanagedStringRanges;
        internal readonly SharedList<byte> _unmanagedStringBuffer;
        internal readonly FasterList<string> _managedStrings;
        internal readonly SharedList<Option<StringHash>> _hashes;
        internal readonly SharedReference<int> _count;
        internal readonly object _lock = new();

        public StringVault(int initialCapacity)
        {
            _map = new(initialCapacity);
            _unmanagedStringRanges = new(initialCapacity);
            _unmanagedStringBuffer = new(initialCapacity * 512);
            _managedStrings = new(initialCapacity);
            _hashes = new(initialCapacity);
            _count = new();
            Clear();
        }

        public StringVault(ReadOnlySpan<string> managedStrings) : this(managedStrings.Length)
        {
            foreach (var str in managedStrings)
            {
                GetOrMakeId(str);
            }
        }

        public StringVault(ReadOnlySpan<UnmanagedString> unmanagedStrings) : this(unmanagedStrings.Length)
        {
            foreach (var str in unmanagedStrings)
            {
                GetOrMakeId(str);
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

        public string this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _managedStrings[index];
        }

        public void Dispose()
        {
            _map.Dispose();
            _unmanagedStringRanges.Dispose();
            _unmanagedStringBuffer.Dispose();
            _hashes.Dispose();
            _count.Dispose();
        }

        public void Clear()
        {
            lock (_lock)
            {
                _map.Clear();
                _unmanagedStringRanges.Clear();
                _unmanagedStringBuffer.Clear();
                _managedStrings.Clear();
                _hashes.Clear();

                // The first item represent an invalid id
                _map.Add(default, default);
                _unmanagedStringRanges.Add(default);
                _managedStrings.Add(string.Empty);
                _hashes.Add(default);

                _count.ValueRW = 1;
            }
        }

        /// <summary>
        /// Creates or retrieves a <see cref="StringId"/> from an <see cref="UnmanagedString"/>.
        /// </summary>
        /// <param name="str">
        /// The managed string to create or retrieve the <see cref="StringId"/> for.
        /// It should have a maximum length of 125 UTF8 characters.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// The unmanaged string will be synchronized with its managed representation.
        /// </remarks>
        public StringId GetOrMakeId(in UnmanagedString str)
        {
            var hash = str.GetHashCode64();
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

                    _unmanagedStringRanges[index] = WriteToBuffer(str);
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

                        _unmanagedStringRanges[index] = WriteToBuffer(str);
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
        /// It should have a maximum length of 125 UTF8 characters.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// The managed string will be synchronized with its unmanaged representation.
        /// </remarks>
        public StringId GetOrMakeId([NotNull] string str)
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

                    _unmanagedStringRanges[index] = WriteToBuffer(str);
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
                        _unmanagedStringRanges[index] = WriteToBuffer(str);
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
        public Option<UnmanagedString> TryGetUnmanagedString(StringId id)
            => TryGetUnmanagedString(id, out var result) ? Option.Some(result) : Option.None;

        public bool TryGetUnmanagedString(StringId id, out UnmanagedString result)
        {
            var indexUnsigned = (uint)id.Id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;

            var resultOpt = validIndex
                ? UnmanagedString.FromBufferAt(_unmanagedStringRanges.AsReadOnly()[index], _unmanagedStringBuffer.AsReadOnlySpan())
                : Option.None;

            result = resultOpt.GetValueOrDefault();
            return resultOpt.HasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<string> TryGetManagedString(StringId id)
            => TryGetManagedString(id, out var result) ? Option.Some(result) : Option.None;

        public bool TryGetManagedString(StringId id, out string result)
        {
            var indexUnsigned = (uint)id.Id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;

            result = validIndex ? _managedStrings[index] : string.Empty;
            return validIndex ? _hashes[index].HasValue : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsId(StringId id)
        {
            var indexUnsigned = (uint)id.Id;
            var index = (int)indexUnsigned;
            var validIndex = indexUnsigned < (uint)_hashes.Count;
            return validIndex ? _hashes[index].HasValue : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<string> GetManagedStringSpan()
            => _managedStrings.AsReadOnlySpan()[..Count];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnmanagedStringSpan GetUnmanagedStringSpan()
            => new(_unmanagedStringRanges.AsReadOnlySpan()[1..Count], _unmanagedStringBuffer.AsReadOnlySpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<string> destination)
            => CopyTo(destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<string> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<string> destination)
            => CopyTo(sourceStartIndex, destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<string> destination, int length)
            => GetManagedStringSpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<string> destination)
            => TryCopyTo(destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<string> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<string> destination)
            => TryCopyTo(sourceStartIndex, destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<string> destination, int length)
            => GetManagedStringSpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<UnmanagedString> destination)
            => CopyTo(destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<UnmanagedString> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<UnmanagedString> destination)
            => CopyTo(sourceStartIndex, destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<UnmanagedString> destination, int length)
            => GetUnmanagedStringSpan().CopyTo(sourceStartIndex, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<UnmanagedString> destination)
            => TryCopyTo(destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<UnmanagedString> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<UnmanagedString> destination)
            => TryCopyTo(sourceStartIndex, destination, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<UnmanagedString> destination, int length)
            => GetUnmanagedStringSpan().TryCopyTo(sourceStartIndex, destination, length);

        public void IncreaseCapacityBy(int amount)
        {
            Checks.IsTrue(amount > 0, "amount must be greater than 0");
            IncreaseCapacityTo(Capacity + amount);
        }

        public void IncreaseCapacityTo(int newCapacity)
        {
            _map.IncreaseCapacityTo(newCapacity);
            EnsureCapacity();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterListEnumerator<string> GetEnumerator()
            => _managedStrings.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void EnsureCapacity()
        {
            var oldCapacity = Math.Min(_hashes.Capacity, _unmanagedStringRanges.Capacity);
            var newCapacity = Math.Max(_map.Capacity, _count.ValueRO);

            if (newCapacity > 0 && newCapacity > oldCapacity)
            {
                _hashes.IncreaseCapacityTo(newCapacity);
                _unmanagedStringRanges.IncreaseCapacityTo(newCapacity);
                _managedStrings.IncreaseCapacityTo(newCapacity);
            }

            if (_hashes.Count < newCapacity)
            {
                _hashes.AddReplicateNoInit(Math.Max(newCapacity - _hashes.Count, 0));
            }

            if (_unmanagedStringRanges.Count < newCapacity)
            {
                _unmanagedStringRanges.AddReplicateNoInit(Math.Max(newCapacity - _unmanagedStringRanges.Count, 0));
            }

            if (_managedStrings.Count < newCapacity)
            {
                _managedStrings.AddReplicateNoInit(Math.Max(newCapacity - _managedStrings.Count, 0));
            }

            var newBufferCapacity = newCapacity * 512;

            if (_unmanagedStringBuffer.Capacity < newBufferCapacity)
            {
                _unmanagedStringBuffer.IncreaseCapacityTo(newBufferCapacity);
            }
        }

        private Range WriteToBuffer(in UnmanagedString str)
        {
            var buffer = _unmanagedStringBuffer;
            var startIndex = buffer.Count;
            var amount = str.Length;
            var strSpan = buffer.AddReplicateNoInit(amount);
            str.AsReadOnlySpan().CopyTo(strSpan);

            return new(startIndex, startIndex + amount);
        }

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfFailedRegistering([DoesNotReturnIf(false)] bool result, in UnmanagedString str, StringId id)
        {
            if (result == false)
            {
                throw new InvalidOperationException(
                    $"Cannot register a StringId by the same value \"{str}\" with different id \"{id}\"."
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfFailedRegistering([DoesNotReturnIf(false)] bool result, string str, StringId id)
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
