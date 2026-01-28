using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Collections.Generic.Exposed;

internal readonly struct DictionaryExposed<TKey, TValue>([NotNull] Dictionary<TKey, TValue> dictionary)
    where TKey : notnull
{
    public readonly Dictionary<TKey, TValue> Dictionary = dictionary;

    public ReadOnlySpan<int> Buckets
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Dictionary._buckets);
    }

    public ReadOnlySpan<Entry> Entries
    {
        get
        {
            var span = new ReadOnlySpan<Dictionary<TKey, TValue>.Entry>(Dictionary._entries, 0, Dictionary._count);
            return MemoryMarshal.Cast<Dictionary<TKey, TValue>.Entry, Entry>(span);
        }
    }

    public ulong FastModMultiplier
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._fastModMultiplier;
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._count;
    }

    public int FreeList
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._freeList;
    }

    public int FreeCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._freeCount;
    }

    public int Version
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._version;
    }

    public IEqualityComparer<TKey> Comparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._comparer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue FindValue(TKey key)
    {
        return ref Dictionary.FindValue(key);
    }

    public struct Entry
    {
        public uint hashCode;
        public int next;
        public TKey key;
        public TValue value;
    }
}
