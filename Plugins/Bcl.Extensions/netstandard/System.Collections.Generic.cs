#pragma warning disable

namespace System.Collections.Generic;

public interface IEqualityComparer<in T>
{
    bool Equals(T? x, T? y);

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

public class List<T>
{
    public T?[] _items;
    public int _size;
    public int _version;

    public int Capacity { get => default; set { } }

    public void EnsureCapacity(int min) { }
}

public class Dictionary<TKey, TValue>
{
    public int[] _buckets;
    public Entry[] _entries;
    public ulong _fastModMultiplier;
    public int _count;
    public int _freeList;
    public int _freeCount;
    public int _version;
    public IEqualityComparer<TKey> _comparer;

    public void Add(TKey key, TValue value) { }

    public int FindEntry(TKey key) => default;

    public struct Entry
    {
        public uint hashCode;
        public int next;
        public TKey key;
        public TValue value;
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

    public struct Entry
    {
        public int HashCode;
        public int Next;
        public T Value;
    }
}
