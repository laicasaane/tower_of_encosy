namespace System.Collections.Generic;

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
    public Dictionary<TKey, TValue?>.Entry[] _entries;

    public void Add(TKey key, TValue value) { }

    public int FindEntry(TKey key) => default;

    public struct Entry
    {
        public TValue? value;
    }
}
