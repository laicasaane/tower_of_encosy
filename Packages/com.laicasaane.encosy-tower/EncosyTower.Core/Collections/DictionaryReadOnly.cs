using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.Unsafe;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    public readonly struct DictionaryReadOnly<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private static readonly DictionaryReadOnly<TKey, TValue> s_empty = new(new());

        internal readonly DictionaryExposed<TKey, TValue> _dictionary;

        public DictionaryReadOnly()
        {
            _dictionary = s_empty._dictionary;
        }

        public DictionaryReadOnly([NotNull] Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = new(dictionary);
        }

        public static DictionaryReadOnly<TKey, TValue> Empty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_empty;
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Dictionary != null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Entries.Length;
        }

        public bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Dictionary.Keys;
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Dictionary.Values;
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Keys;
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Values;
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Dictionary[key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator DictionaryReadOnly<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
            => dictionary is not null ? new(dictionary) : Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
            => _dictionary.Dictionary.ContainsKey(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(TValue value)
            => _dictionary.Dictionary.ContainsValue(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
            => _dictionary.Dictionary.TryGetValue(key, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
            => _dictionary.Dictionary.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
