// MIT License
// Author: Muhammad Rehan Saeed
// https://github.com/RehanSaeed/rehansaeed.github.io/blob/19b2e092711e44e4e5995dbd703c05d0a500adbb/content/posts/2014/gethashcode-made-easy/index.md

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    /// <summary>
    /// Combines the hash code for multiple values into a single hash code
    /// to help with implementing <see cref="object.GetHashCode()"/>.
    /// </summary>
    /// <remarks>
    /// This type provides an alternative to <see cref="System.HashCode"/>.
    /// Because, by design, <see cref="System.HashCode"/> is unstable within Burst.
    /// </remarks>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/csharp-system-support.html#systemhashcode"/>
    public readonly struct HashValue : IEquatable<HashValue>
    {
        private const int EMPTY_COLLECTION_PRIME_NUMBER = 19;

        private readonly int _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashValue"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HashValue(int value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(HashValue hashCode)
            => hashCode._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(HashValue left, HashValue right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(HashValue left, HashValue right)
            => !(left == right);

        /// <summary>
        /// Returns the hash code of a specific item.
        /// </summary>
        /// <typeparam name="T1">The type of the item to add the hash code.</typeparam>
        /// <param name="item1">The item to add to the hash code.</param>
        /// <returns>The hash code that represents the single value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1>(T1 item1)
            => new(GetHashCode(item1));

        /// <summary>
        /// Combines two items into a hash code.
        /// </summary>
        /// <typeparam name="T1">The first type of the item to add the hash code.</typeparam>
        /// <typeparam name="T2">The second type of the item to add the hash code.</typeparam>
        /// <param name="item1">The first item to add to the hash code.</param>
        /// <param name="item2">The second item to add to the hash code.</param>
        /// <returns>The hash code that represents the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1, T2>(T1 item1, T2 item2)
            => Combine(item1).Add(item2);

        /// <summary>
        /// Combines three items into a hash code.
        /// </summary>
        /// <typeparam name="T1">The first type of the item to add the hash code.</typeparam>
        /// <typeparam name="T2">The second type of the item to add the hash code.</typeparam>
        /// <typeparam name="T3">The third type of the item to add the hash code.</typeparam>
        /// <param name="item1">The first item to add to the hash code.</param>
        /// <param name="item2">The second item to add to the hash code.</param>
        /// <param name="item3">The third item to add to the hash code.</param>
        /// <returns>The hash code that represents the three values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
            => Combine(item1, item2).Add(item3);

        /// <summary>
        /// Combines four items into a hash code.
        /// </summary>
        /// <typeparam name="T1">The first type of the item to add the hash code.</typeparam>
        /// <typeparam name="T2">The second type of the item to add the hash code.</typeparam>
        /// <typeparam name="T3">The third type of the item to add the hash code.</typeparam>
        /// <typeparam name="T4">The fourth type of the item to add the hash code.</typeparam>
        /// <param name="item1">The first item to add to the hash code.</param>
        /// <param name="item2">The second item to add to the hash code.</param>
        /// <param name="item3">The third item to add to the hash code.</param>
        /// <param name="item4">The fourth item to add to the hash code.</param>
        /// <returns>The hash code that represents the four values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
            => Combine(item1, item2, item3).Add(item4);

        /// <summary>
        /// Combines five items into a hash code.
        /// </summary>
        /// <typeparam name="T1">The first type of the item to add the hash code.</typeparam>
        /// <typeparam name="T2">The second type of the item to add the hash code.</typeparam>
        /// <typeparam name="T3">The third type of the item to add the hash code.</typeparam>
        /// <typeparam name="T4">The fourth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T5">The fifth type of the item to add the hash code.</typeparam>
        /// <param name="item1">The first item to add to the hash code.</param>
        /// <param name="item2">The second item to add to the hash code.</param>
        /// <param name="item3">The third item to add to the hash code.</param>
        /// <param name="item4">The fourth item to add to the hash code.</param>
        /// <param name="item5">The fifth item to add to the hash code.</param>
        /// <returns>The hash code that represents the five values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
            => Combine(item1, item2, item3, item4).Add(item5);

        /// <summary>
        /// Combines six items into a hash code.
        /// </summary>
        /// <typeparam name="T1">The first type of the item to add the hash code.</typeparam>
        /// <typeparam name="T2">The second type of the item to add the hash code.</typeparam>
        /// <typeparam name="T3">The third type of the item to add the hash code.</typeparam>
        /// <typeparam name="T4">The fourth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T5">The fifth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T6">The sixth type of the item to add the hash code.</typeparam>
        /// <param name="item1">The first item to add to the hash code.</param>
        /// <param name="item2">The second item to add to the hash code.</param>
        /// <param name="item3">The third item to add to the hash code.</param>
        /// <param name="item4">The fourth item to add to the hash code.</param>
        /// <param name="item5">The fifth item to add to the hash code.</param>
        /// <param name="item6">The sixth item to add to the hash code.</param>
        /// <returns>The hash code that represents the six values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
            => Combine(item1, item2, item3, item4, item5).Add(item6);

        /// <summary>
        /// Combines seven items into a hash code.
        /// </summary>
        /// <typeparam name="T1">The first type of the item to add the hash code.</typeparam>
        /// <typeparam name="T2">The second type of the item to add the hash code.</typeparam>
        /// <typeparam name="T3">The third type of the item to add the hash code.</typeparam>
        /// <typeparam name="T4">The fourth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T5">The fifth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T6">The sixth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T7">The seventh type of the item to add the hash code.</typeparam>
        /// <param name="item1">The first item to add to the hash code.</param>
        /// <param name="item2">The second item to add to the hash code.</param>
        /// <param name="item3">The third item to add to the hash code.</param>
        /// <param name="item4">The fourth item to add to the hash code.</param>
        /// <param name="item5">The fifth item to add to the hash code.</param>
        /// <param name="item6">The sixth item to add to the hash code.</param>
        /// <param name="item7">The seventh item to add to the hash code.</param>
        /// <returns>The hash code that represents the seven values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
            => Combine(item1, item2, item3, item4, item5, item6).Add(item7);

        /// <summary>
        /// Combines eight items into a hash code.
        /// </summary>
        /// <typeparam name="T1">The first type of the item to add the hash code.</typeparam>
        /// <typeparam name="T2">The second type of the item to add the hash code.</typeparam>
        /// <typeparam name="T3">The third type of the item to add the hash code.</typeparam>
        /// <typeparam name="T4">The fourth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T5">The fifth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T6">The sixth type of the item to add the hash code.</typeparam>
        /// <typeparam name="T7">The seventh type of the item to add the hash code.</typeparam>
        /// <typeparam name="T8">The eighth type of the item to add the hash code.</typeparam>
        /// <param name="item1">The first item to add to the hash code.</param>
        /// <param name="item2">The second item to add to the hash code.</param>
        /// <param name="item3">The third item to add to the hash code.</param>
        /// <param name="item4">The fourth item to add to the hash code.</param>
        /// <param name="item5">The fifth item to add to the hash code.</param>
        /// <param name="item6">The sixth item to add to the hash code.</param>
        /// <param name="item7">The seventh item to add to the hash code.</param>
        /// <param name="item8">The eighth item to add to the hash code.</param>
        /// <returns>The hash code that represents the eight values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
            => Combine(item1, item2, item3, item4, item5, item6, item7).Add(item8);

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The collection.</param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue CombineEach<T>(IEnumerable<T> items)
            => items == null
            ? new(0)
            : new(GetHashCodeEach(items, 0, null));

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="enumerator">The enumerator of a collection.</param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue CombineEach<T, TEnumerator>(TEnumerator enumerator)
            where TEnumerator : IEnumerator<T>
            => enumerator == null
            ? new(0)
            : new(GetHashCodeEach<T, TEnumerator>(enumerator, 0));

        /// <summary>
        /// Adds a single item to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="item">The item to add to the hash code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue Add<T>(T item)
            => new(CombineHashes(_value, GetHashCode(item)));

        /// <summary>
        /// Adds a single item to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="item">The item to add to the hash code.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> to use to calculate the hash code.
        /// This value can be a null reference, which will use
        /// the default equality comparer for <typeparamref name="T"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue Add<T>(T item, IEqualityComparer<T> comparer)
            => new(CombineHashes(_value, (comparer ?? EqualityComparer<T>.Default).GetHashCode(item)));

        /// <summary>
        /// Adds a single item to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <typeparam name="TEqualityComparer">The type implements <see cref="IEqualityComparer{T}"/>.</typeparam>
        /// <param name="item">The item to add to the hash code.</param>
        /// <param name="comparer">
        /// The <typeparamref name="TEqualityComparer"/> to use to calculate the hash code.
        /// This value can be a null reference, in which case
        /// the internal <see cref="GetHashCode{T}(T)"/> will be used instead.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue Add<T, TEqualityComparer>(T item, TEqualityComparer comparer)
            where TEqualityComparer : IEqualityComparer<T>
            => comparer == null
            ? new(CombineHashes(_value, GetHashCode(item)))
            : new(CombineHashes(_value, comparer.GetHashCode(item)));

        /// <summary>
        /// Adds a collection of items to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="items">The collection of items to add to the hash code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue AddEach<T>(IEnumerable<T> items)
            => items == null
            ? new(_value)
            : new(GetHashCodeEach(items, _value, null));

        /// <summary>
        /// Adds a collection of items to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="items">The collection of items to add to the hash code.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> to use to calculate the hash code.
        /// This value can be a null reference, which will use
        /// the default equality comparer for <typeparamref name="T"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue AddEach<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
            => items == null
            ? new(_value)
            : new(GetHashCodeEach(items, _value, comparer ?? EqualityComparer<T>.Default));

        /// <summary>
        /// Adds a collection of items to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <typeparam name="TEnumerator">The type of the enumerator of the collection.</typeparam>
        /// <param name="enumerator">The enumerator of the collection.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue AddEach<T, TEnumerator>(TEnumerator enumerator)
            where TEnumerator : IEnumerator<T>
            => enumerator == null
            ? new(_value)
            : new(GetHashCodeEach<T, TEnumerator>(enumerator, _value));

        /// <summary>
        /// Adds a collection of items to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <typeparam name="TEnumerator">The type of the enumerator of the collection.</typeparam>
        /// <typeparam name="TEqualityComparer">The type implements <see cref="IEqualityComparer{T}"/>.</typeparam>
        /// <param name="enumerator">The enumerator of the collection.</param>
        /// <param name="comparer">
        /// The <typeparamref name="TEqualityComparer"/> to use to calculate the hash code.
        /// This value can be a null reference, in which case
        /// the internal <see cref="GetHashCode{T}(T)"/> will be used instead.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue AddEach<T, TEnumerator, TEqualityComparer>(TEnumerator enumerator, TEqualityComparer comparer)
            where TEnumerator : IEnumerator<T>
            where TEqualityComparer : IEqualityComparer<T>
            => enumerator == null
            ? new(_value)
            : new(GetHashCodeEach<T, TEnumerator, TEqualityComparer>(enumerator, _value, comparer));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(HashValue other)
            => _value.Equals(other._value);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is HashValue other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToHashCode()
            => _value;

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>Does not return.</returns>
        /// <exception cref="NotSupportedException">
        /// Implicitly convert this struct to an <see cref="int" /> to get the hash code.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
            => throw new NotSupportedException(
                "Implicitly convert this struct to an int to get the hash code or use ToHashCode method."
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CombineHashes(int h1, int h2)
        {
            unchecked
            {
                // Code copied from System.Tuple so it must be the best way
                // to combine hash codes or at least a good one.
                return ((h1 << 5) + h1) ^ h2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHashCode<T>(T item)
            => item?.GetHashCode() ?? 0;

        private static int GetHashCodeEach<T>(
              IEnumerable<T> items
            , int startHashCode
            , IEqualityComparer<T> comparer
        )
        {
            var result = startHashCode;
            var enumerator = items.GetEnumerator();

            if (enumerator.MoveNext() == false)
            {
                return CombineHashes(result, EMPTY_COLLECTION_PRIME_NUMBER);
            }

            result = CombineHashes(result, comparer.GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineHashes(result, comparer.GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static int GetHashCodeEach<T, TEnumerator>(TEnumerator enumerator, int startHashCode)
            where TEnumerator : IEnumerator<T>
        {
            var result = startHashCode;

            if (enumerator.MoveNext() == false)
            {
                return CombineHashes(result, EMPTY_COLLECTION_PRIME_NUMBER);
            }

            result = CombineHashes(result, GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineHashes(result, GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static int GetHashCodeEach<T, TEnumerator, TEqualityComparer>(
              TEnumerator enumerator
            , int startHashCode
            , TEqualityComparer comparer
        )
            where TEnumerator : IEnumerator<T>
            where TEqualityComparer : IEqualityComparer<T>
        {
            var result = startHashCode;

            if (enumerator.MoveNext() == false)
            {
                return CombineHashes(result, EMPTY_COLLECTION_PRIME_NUMBER);
            }

            if (comparer != null)
            {
                result = CombineHashes(result, comparer.GetHashCode(enumerator.Current));

                while (enumerator.MoveNext())
                {
                    result = CombineHashes(result, comparer.GetHashCode(enumerator.Current));
                }
            }
            else
            {
                result = CombineHashes(result, GetHashCode(enumerator.Current));

                while (enumerator.MoveNext())
                {
                    result = CombineHashes(result, GetHashCode(enumerator.Current));
                }
            }

            return result;
        }
    }
}
