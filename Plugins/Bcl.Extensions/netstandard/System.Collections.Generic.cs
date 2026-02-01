#pragma warning disable

namespace System.Collections.Generic;

public interface IEqualityComparer<in T>
{
    bool Equals(T x, T y);

    int GetHashCode(T obj);
}

public interface IEnumerable<out T> : IEnumerable
{
    new IEnumerator<T> GetEnumerator();
}

public interface IEnumerator<out T> : IDisposable, IEnumerator
{
    new T Current { get; }
}

public abstract partial class EqualityComparer<T> : IEqualityComparer<T>
{
    public static EqualityComparer<T> Default = default;

    public bool Equals(T x, T y)
    {
        throw new NotImplementedException();
    }

    public int GetHashCode(T obj)
    {
        throw new NotImplementedException();
    }
}

public class List<T>
{
    public T[] _items;
    public int _size;
    public int _version;

    public int Capacity { get => default; set { } }

    public void Grow(int capacity) { }
}

public enum InsertionBehavior : byte
{
    None,
    OverwriteExisting,
    ThrowOnExisting
}

public class Dictionary<TKey, TValue> where TKey : notnull
{
    public int[] _buckets;
    public Entry[] _entries;
    public int _count;
    public int _freeList;
    public int _freeCount;
    public int _version;
    public IEqualityComparer<TKey> _comparer;
    public KeyCollection _keys;
    public ValueCollection _values;
    public object _syncRoot;

    public int Initialize(int capacity) => default;

    public void Add(TKey key, TValue value) { }

    public int FindEntry(TKey key) => default;

    public void Resize() { }

    public void Resize(int newSize, bool forceNewHashCodes) { }

    public bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
    {
        return default;
    }

    public struct Entry
    {
        public uint hashCode;
        public int next;
        public TKey key;
        public TValue value;
    }

    public sealed class KeyCollection
    {
    }

    public sealed class ValueCollection
    {
    }
}

public class HashSet<T>
{
    public int[] _buckets;
    public Entry[] _entries;
    public ulong _fastModMultiplier;
    public int _count;
    public int _freeList;
    public int _freeCount;
    public int _version;
    public IEqualityComparer<T> _comparer;

    public int FindItemIndex(T item)
    {
        throw new NotImplementedException();
    }

    public struct Entry
    {
        public int HashCode;
        public int Next;
        public T Value;
    }
}
