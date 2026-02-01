using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices.Exposed;
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

    public Dictionary<TKey, TValue>.KeyCollection Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._keys;
    }

    public Dictionary<TKey, TValue>.ValueCollection Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>  Dictionary._values;
    }

    public object SyncRoot
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary._syncRoot;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int FindEntry(TKey key)
    {
        return Dictionary.FindEntry(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
    {
        return Dictionary.TryInsert(key, value, (Generic.InsertionBehavior)(byte)behavior);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue FindValue(TKey key)
    {
        var i = Dictionary.FindEntry(key);
        return ref i >= 0 ? ref Dictionary._entries[i].value : ref UnsafeExposed.NullRef<TValue>();
    }

    public struct Entry
    {
        public uint hashCode;
        public int next;
        public TKey key;
        public TValue value;
    }

    public enum InsertionBehavior : byte
    {
        None = Generic.InsertionBehavior.None,
        OverwriteExisting = Generic.InsertionBehavior.OverwriteExisting,
        ThrowOnExisting = Generic.InsertionBehavior.ThrowOnExisting,
    }
}
