using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using Unity.Collections;

namespace EncosyTower.StringIds
{
    partial class StringVault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly partial struct ReadOnly : IReadOnlyList<UnmanagedString>
            , ICopyToSpan<UnmanagedString>, ITryCopyToSpan<UnmanagedString>
        {
            internal readonly SharedArrayMapNative<StringHash, StringId>.ReadOnly _map;
            internal readonly SharedListNative<Range>.ReadOnly _unmanagedStringRanges;
            internal readonly SharedListNative<byte>.ReadOnly _unmanagedStringBuffer;
            internal readonly SharedListNative<Option<StringHash>>.ReadOnly _hashes;
            internal readonly NativeArray<int>.ReadOnly _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(StringVault vault)
            {
                _map = vault._map.AsNative();
                _unmanagedStringRanges = vault._unmanagedStringRanges.AsNative();
                _unmanagedStringBuffer = vault._unmanagedStringBuffer.AsNative();
                _hashes = vault._hashes.AsNative();
                _count = vault._count.AsNativeArray().AsReadOnly();
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.IsCreated
                    && _unmanagedStringRanges.IsCreated
                    && _unmanagedStringBuffer.IsCreated
                    && _hashes.IsCreated
                    && _count.IsCreated;
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

            public UnmanagedString this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => UnmanagedString.FromBufferAt(_unmanagedStringRanges[index], _unmanagedStringBuffer).GetValueOrThrow();
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
                var validIndex = indexUnsigned < (uint)_hashes.Count;

                var resultOpt = validIndex
                    ? UnmanagedString.FromBufferAt(_unmanagedStringRanges[index], _unmanagedStringBuffer)
                    : Option.None;

                result = resultOpt.GetValueOrDefault();
                return resultOpt.HasValue;
            }

            public bool ContainsId(StringId id)
            {
                var indexUnsigned = (uint)id.Id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Count;
                return validIndex ? _hashes[index].HasValue : false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnmanagedStringSpan GetUnmanagedStringSpan()
                => new(_unmanagedStringRanges.AsReadOnlySpan()[1..Count], _unmanagedStringBuffer.AsReadOnlySpan());

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
                => new(_unmanagedStringRanges, _unmanagedStringBuffer);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<UnmanagedString> IEnumerable<UnmanagedString>.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(StringVault vault)
                => new(vault);
        }
    }
}
