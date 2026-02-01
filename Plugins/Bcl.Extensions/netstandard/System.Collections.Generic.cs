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
    public static EqualityComparer<T> Default { get => default; }

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

    public struct Entry
    {
        public int hashCode;
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
    public Slot[] _slots;
    public int _count;
    public int _lastIndex;
    public int _freeList;
    public IEqualityComparer<T> _comparer;
    public int _version;

    public struct Slot
    {
        public int hashCode;
        public int next;
        public T value;
    }
}
