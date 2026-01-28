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

public class List<T>
{
    public T[] _items;
    public int _size;
    public int _version;

    public int Capacity { get => default; set { } }

    public void Grow(int capacity) { }
}

public class Dictionary<TKey, TValue> where TKey : notnull
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

    public ref TValue FindValue(TKey key)
    {
        throw new NotImplementedException();
    }

    public struct Entry
    {
        public uint hashCode;
        public int next;
        public TKey key;
        public TValue value;
    }

    public static class CollectionsMarshalHelper
    {
        public static ref TValue GetValueRefOrAddDefault(Dictionary<TKey, TValue> dictionary, TKey key, out bool exists)
        {
            throw new NotImplementedException();
        }
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
