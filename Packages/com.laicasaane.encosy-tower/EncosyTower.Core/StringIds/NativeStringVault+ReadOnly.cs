#if UNITY_COLLECTIONS && UNITY_MATHEMATICS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using Unity.Collections;

namespace EncosyTower.StringIds
{
    partial struct NativeStringVault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly struct ReadOnly : IReadOnlyList<UnmanagedString>
            , ICopyToSpan<UnmanagedString>, ITryCopyToSpan<UnmanagedString>
        {
            internal readonly NativeHashMap<StringHash, StringId>.ReadOnly _map;
            internal readonly NativeArray<Range>.ReadOnly _stringRanges;
            internal readonly NativeArray<byte>.ReadOnly _stringBuffer;
            internal readonly NativeArray<Option<StringHash>>.ReadOnly _hashes;
            internal readonly NativeReference<int>.ReadOnly _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(in NativeStringVault vault)
            {
                _map = vault._map.AsReadOnly();
                _stringRanges = vault._stringRanges.AsArray().AsReadOnly();
                _stringBuffer = vault._stringBuffer.AsArray().AsReadOnly();
                _hashes = vault._hashes.AsArray().AsReadOnly();
                _count = vault._count.AsReadOnly();
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.IsCreated
                    && _stringRanges.IsCreated
                    && _stringBuffer.IsCreated
                    && _hashes.IsCreated;
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

            public UnmanagedString this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => UnmanagedString.FromBufferAt(_stringRanges[index], _stringBuffer).GetValueOrThrow();
            }

            public bool TryGetId(in UnmanagedString str, out StringId result)
            {
                var hash = str.GetHashCode64();
                var registered = _map.TryGetValue(hash, out var id);

                if (registered && TryGetString(id, out var registeredString) && str == registeredString)
                {
                    result = id;
                    return true;
                }

                result = default;
                return false;
            }

            public bool TryGetString(StringId id, out UnmanagedString result)
            {
                var indexUnsigned = (uint)id.Id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Length;

                var resultOpt = validIndex
                    ? UnmanagedString.FromBufferAt(_stringRanges[index], _stringBuffer)
                    : Option.None;

                result = resultOpt.GetValueOrDefault();
                return resultOpt.HasValue;
            }

            public bool ContainsId(StringId id)
            {
                var indexUnsigned = (uint)id.Id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Length;
                return validIndex ? _hashes[index].HasValue : false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnmanagedStringSpan GetStringSpan()
                => new(_stringRanges.AsReadOnlySpan()[1..Count], _stringBuffer);

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
                => GetStringSpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

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
                => GetStringSpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
                => new(_stringRanges, _stringBuffer);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<UnmanagedString> IEnumerable<UnmanagedString>.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(in NativeStringVault vault)
                => new(vault);
        }
    }
}

#endif
