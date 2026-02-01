// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.Exposed;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices.Exposed;

namespace System.Runtime.InteropServices;

/// <summary>
/// An unsafe class that provides a set of methods to access the underlying data representations of collections.
/// </summary>
public static class CollectionsMarshal
{
    /// <summary>
    /// Get a <see cref="Span{T}"/> view over a <see cref="List{T}"/>'s data.
    /// Items should not be added or removed from the <see cref="List{T}"/> while the <see cref="Span{T}"/> is in use.
    /// </summary>
    /// <param name="list">The list to get the data view over.</param>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(List<T> list)
    {
        Span<T> span = default;

        if (list is not null)
        {
            int size = list._size;
            T[] items = list._items;
            Debug.Assert(items is not null, "Implementation depends on List<T> always having an array.");

            if ((uint)size > (uint)items.Length)
            {
                // List<T> was erroneously mutated concurrently with this call, leading to a count larger than its array.
                ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
            }

            Debug.Assert(typeof(T[]) == list._items.GetType(), "Implementation depends on List<T> always using a T[] and not U[] where U : T.");
            span = new Span<T>(items, 0, size);
        }

        return span;
    }

    /// <summary>
    /// Gets either a ref to a <typeparamref name="TValue"/> in the <see cref="Dictionary{TKey, TValue}"/> or a ref null if it does not exist in the <paramref name="dictionary"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary to get the ref to <typeparamref name="TValue"/> from.</param>
    /// <param name="key">The key used for lookup.</param>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <remarks>
    /// Items should not be added or removed from the <see cref="Dictionary{TKey, TValue}"/> while the ref <typeparamref name="TValue"/> is in use.
    /// The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
    /// </remarks>
    public static ref TValue GetValueRefOrNullRef<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
    {
        var exposed = new DictionaryExposed<TKey, TValue>(dictionary);
        return ref exposed.FindValue(key);
    }

    /// <summary>
    /// Gets a ref to a <typeparamref name="TValue"/> in the <see cref="Dictionary{TKey, TValue}"/>, adding a new entry with a default value if it does not exist in the <paramref name="dictionary"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary to get the ref to <typeparamref name="TValue"/> from.</param>
    /// <param name="key">The key used for lookup.</param>
    /// <param name="exists">Whether or not a new entry for the given key was added to the dictionary.</param>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <remarks>Items should not be added to or removed from the <see cref="Dictionary{TKey, TValue}"/> while the ref <typeparamref name="TValue"/> is in use.</remarks>
    public static ref TValue GetValueRefOrAddDefault<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, out bool exists) where TKey : notnull
    {
        // NOTE: this method is mirrored by Dictionary<TKey, TValue>.TryInsert above.
        // If you make any changes here, make sure to keep that version in sync as well.

        if (key == null)
        {
            ThrowArgumentNullException_Key();
        }

        if (dictionary._buckets == null)
        {
            dictionary.Initialize(0);
        }

        Debug.Assert(dictionary._buckets != null);

        Dictionary<TKey, TValue>.Entry[]? entries = dictionary._entries;
        Debug.Assert(entries != null, "expected entries to be non-null");

        IEqualityComparer<TKey>? comparer = dictionary._comparer;
        Debug.Assert(comparer is not null || typeof(TKey).IsValueType);
        int hashCode = ((typeof(TKey).IsValueType && comparer == null) ? key.GetHashCode() : comparer!.GetHashCode(key));

        uint collisionCount = 0;
        ref int bucket = ref GetBucket(dictionary, hashCode);
        int i = bucket - 1; // Value in _buckets is 1-based

        if (typeof(TKey).IsValueType && // comparer can only be null for value types; enable JIT to eliminate entire if block for ref types
            comparer == null)
        {
            // ValueType: Devirtualize with EqualityComparer<TKey>.Default intrinsic
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].hashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entries[i].key, key))
                {
                    exists = true;

                    return ref entries[i].value!;
                }

                i = entries[i].next;

                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                }
            }
        }
        else
        {
            Debug.Assert(comparer is not null);
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    exists = true;

                    return ref entries[i].value!;
                }

                i = entries[i].next;

                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                }
            }
        }

        int index;
        if (dictionary._freeCount > 0)
        {
            index = dictionary._freeList;
            dictionary._freeList = entries[index].next;
            dictionary._freeCount--;
        }
        else
        {
            int count = dictionary._count;
            if (count == entries.Length)
            {
                dictionary.Resize();
                bucket = ref GetBucket(dictionary, hashCode);
            }
            index = count;
            dictionary._count = count + 1;
            entries = dictionary._entries;
        }

        ref Dictionary<TKey, TValue>.Entry entry = ref entries![index];
        entry.hashCode = hashCode;
        entry.next = bucket - 1; // Value in _buckets is 1-based
        entry.key = key;
        entry.value = default!;
        bucket = index + 1; // Value in _buckets is 1-based
        dictionary._version++;

        // Value types never rehash
        if (!typeof(TKey).IsValueType && collisionCount > HashHelpers.HashCollisionThreshold)
        {
            // If we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
            // i.e. EqualityComparer<string>.Default.
            dictionary.Resize(entries.Length, true);

            exists = false;

            // At this point the entries array has been resized, so the current reference we have is no longer valid.
            // We're forced to do a new lookup and return an updated reference to the new entry instance. This new
            // lookup is guaranteed to always find a value though and it will never return a null reference here.
            var exposed = new DictionaryExposed<TKey, TValue>(dictionary);
            ref TValue? value = ref exposed.FindValue(key)!;

            Debug.Assert(UnsafeExposed.IsNullRef(value) == false, "the lookup result cannot be a null ref here");

            return ref value!;
        }

        exists = false;

        return ref entry.value!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ref int GetBucket(Dictionary<TKey, TValue> dictionary, int hashCode)
        {
            return ref dictionary._buckets[hashCode % dictionary._buckets.Length];
        }

        [DoesNotReturn]
        static void ThrowArgumentNullException_Key()
        {
            throw new ArgumentNullException("key");
        }

        [DoesNotReturn]
        static void ThrowInvalidOperationException_ConcurrentOperationsNotSupported()
        {
            throw new InvalidOperationException(
                "Operations that change non-concurrent collections must have exclusive access. " +
                "A concurrent update was performed on this collection and corrupted its state. " +
                "The collection's state is no longer correct."
            );
        }
    }

    /// <summary>
    /// Sets the count of the <see cref="List{T}"/> to the specified value.
    /// </summary>
    /// <param name="list">The list to set the count of.</param>
    /// <param name="count">The value to set the list's count to.</param>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <exception cref="NullReferenceException">
    /// <paramref name="list"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="count"/> is negative.
    /// </exception>
    /// <remarks>
    /// When increasing the count, uninitialized data is being exposed.
    /// </remarks>
    public static void SetCount<T>(List<T> list, int count)
    {
        if (count < 0)
        {
            ThrowArgumentOutOfRangeException_NeedNonNegNum(nameof(count));
        }

        list._version++;

        if (count > list.Capacity)
        {
            list.Grow(count);
        }
        else if (count < list._size && RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(list._items, count, list._size - count);
        }

        list._size = count;
    }

    private static void ThrowArgumentOutOfRangeException_NeedNonNegNum(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName, "Non-negative number required.");
    }

    private static void ThrowInvalidOperationException_ConcurrentOperationsNotSupported()
    {
        throw new InvalidOperationException("Operations that change the collection cannot be performed during enumeration.");
    }
}
