// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T?> AsSpan<T>(List<T>? list)
        => list is null ? default : new Span<T?>(list._items, 0, list._size);

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
    public static unsafe ref TValue? GetValueRefOrNullRef<TKey, TValue>(Dictionary<TKey, TValue?> dictionary, TKey key) where TKey : notnull
    {
        int num = dictionary.FindEntry(key);
        if (num >= 0)
        {
            return ref dictionary._entries[num].value;
        }
        return ref *(TValue*)null;
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
    public static ref TValue? GetValueRefOrAddDefault<TKey, TValue>(Dictionary<TKey, TValue?> dictionary, TKey key, out bool exists) where TKey : notnull
    {
        int num = dictionary.FindEntry(key);
        exists = true;
        if (num < 0)
        {
            exists = false;
            dictionary.Add(key, default);
            num = dictionary.FindEntry(key);
        }
        return ref dictionary._entries[num].value;
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
            list.EnsureCapacity(count);
        }
        else if (count < list._size &&
#if NETSTANDARD2_1_OR_GREATER
            RuntimeHelpers.IsReferenceOrContainsReferences<T>()
#else
            ClearCache<T>.MustClear
#endif
        )
        {
            Array.Clear(list._items, count, list._size - count);
        }

        list._size = count;
    }

    private static void ThrowArgumentOutOfRangeException_NeedNonNegNum(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName, "Non-negative number required.");
    }

#if !NETSTANDARD2_1_OR_GREATER
    private static unsafe class ClearCache<T>
    {
        public static readonly bool MustClear = sizeof(T) >= sizeof(void*) && !typeof(T).IsPrimitive && !typeof(T).IsEnum;
    }
#endif
}
