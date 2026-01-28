#pragma warning disable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Collections.Generic.Unsafe;

internal readonly struct HashSetExposed<T>([NotNull] HashSet<T> set)
{
    public readonly HashSet<T> Set = set;

    public ReadOnlySpan<int> Buckets
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Set._buckets);
    }

    public ReadOnlySpan<Entry> Entries
    {
        get
        {
            var span = new ReadOnlySpan<HashSet<T>.Entry>(Set._entries, 0, Set._count);
            return MemoryMarshal.Cast<HashSet<T>.Entry, Entry>(span);
        }
    }

    public ulong FastModMultiplier
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Set._fastModMultiplier;
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Set._count;
    }

    public int FreeList
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Set._freeList;
    }

    public int FreeCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Set._freeCount;
    }

    public int Version
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Set._version;
    }

    public IEqualityComparer<T> Comparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Set._comparer;
    }

    public struct Entry
    {
        public int HashCode;
        public int Next;
        public T Value;
    }
}
