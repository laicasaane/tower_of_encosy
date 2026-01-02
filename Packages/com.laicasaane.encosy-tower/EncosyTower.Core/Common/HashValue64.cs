// MIT License
// Author: Muhammad Rehan Saeed
// https://github.com/RehanSaeed/rehansaeed.github.io/blob/19b2e092711e44e4e5995dbd703c05d0a500adbb/content/posts/2014/gethashcode-made-easy/index.md

// FNV Hash Reference
// https://gist.github.com/StephenCleary/4f6568e5ab5bee7845943fdaef8426d2

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    /// <inheritdoc cref="HashValue"/>
    /// <remarks>
    /// 64 bits version of <see cref="HashValue"/>.
    /// </remarks>
    public readonly partial struct HashValue64 : IEquatable<HashValue64>
    {
        /// <summary>
        /// The prime number used to compute the FNV hash 64 bits.
        /// </summary>
        public const ulong PRIME = 1099511628211;

        /// <summary>
        /// The starting point of the FNV hash 64 bits.
        /// </summary>
        public const ulong BASIS = 14695981039346656037;

        /// <summary>
        /// The prime number used in case the collection is null or empty.
        /// </summary>
        public const ulong EMPTY_COLLECTION_BASIS = 1111111111111111111;

        private readonly ulong  _value = BASIS;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashValue64"/> struct.
        /// </summary>
        /// <param name="value">The starting point of the FNV hash.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HashValue64(ulong value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(HashValue64 hashCode)
            => hashCode._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(HashValue64 left, HashValue64 right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(HashValue64 left, HashValue64 right)
            => !(left == right);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(long value)
            => FNV1a((ulong)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(double value)
            => FNV1a((ulong)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(ulong value)
        {
            ulong result = BASIS;

            result = (((value & 0x00000000000000FF) >> 00) ^ result) * PRIME;
            result = (((value & 0x000000000000FF00) >> 08) ^ result) * PRIME;
            result = (((value & 0x0000000000FF0000) >> 16) ^ result) * PRIME;
            result = (((value & 0x00000000FF000000) >> 24) ^ result) * PRIME;
            result = (((value & 0x000000FF00000000) >> 32) ^ result) * PRIME;
            result = (((value & 0x0000FF0000000000) >> 40) ^ result) * PRIME;
            result = (((value & 0x00FF000000000000) >> 48) ^ result) * PRIME;
            result = (((value & 0xFF00000000000000) >> 56) ^ result) * PRIME;

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(float value)
            => FNV1a((uint)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(int value)
            => FNV1a((uint)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(uint value)
        {
            ulong result = BASIS;
            ulong val = value;

            result = (((val & 0x000000FF) >> 00) ^ result) * PRIME;
            result = (((val & 0x0000FF00) >> 08) ^ result) * PRIME;
            result = (((val & 0x00FF0000) >> 16) ^ result) * PRIME;
            result = (((val & 0xFF000000) >> 24) ^ result) * PRIME;

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(short value)
            => FNV1a((ushort)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(ushort value)
        {
            ulong result = BASIS;
            ulong val = value;

            result = ((val & 255) ^ result) * PRIME;
            result = ((val >> 8) ^ result) * PRIME;

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(sbyte value)
            => FNV1a((byte)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(byte value)
        {
            ulong result = BASIS;
            ulong val = value;

            result = (val ^ result) * PRIME;

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FNV1a(string text)
        {
            return FNV1a(text.AsSpan());
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static ulong FNV1a(ReadOnlySpan<long> span)
        {
            ulong result = BASIS;

            foreach (var item in span)
            {
                ulong val = (ulong)item;

                result = (((val & 0x00000000000000FF) >> 00) ^ result) * PRIME;
                result = (((val & 0x000000000000FF00) >> 08) ^ result) * PRIME;
                result = (((val & 0x0000000000FF0000) >> 16) ^ result) * PRIME;
                result = (((val & 0x00000000FF000000) >> 24) ^ result) * PRIME;
                result = (((val & 0x000000FF00000000) >> 32) ^ result) * PRIME;
                result = (((val & 0x0000FF0000000000) >> 40) ^ result) * PRIME;
                result = (((val & 0x00FF000000000000) >> 48) ^ result) * PRIME;
                result = (((val & 0xFF00000000000000) >> 56) ^ result) * PRIME;
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static ulong FNV1a(ReadOnlySpan<ulong> span)
        {
            ulong result = BASIS;

            foreach (var item in span)
            {
                result = (((item & 0x00000000000000FF) >> 00) ^ result) * PRIME;
                result = (((item & 0x000000000000FF00) >> 08) ^ result) * PRIME;
                result = (((item & 0x0000000000FF0000) >> 16) ^ result) * PRIME;
                result = (((item & 0x00000000FF000000) >> 24) ^ result) * PRIME;
                result = (((item & 0x000000FF00000000) >> 32) ^ result) * PRIME;
                result = (((item & 0x0000FF0000000000) >> 40) ^ result) * PRIME;
                result = (((item & 0x00FF000000000000) >> 48) ^ result) * PRIME;
                result = (((item & 0xFF00000000000000) >> 56) ^ result) * PRIME;
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static ulong FNV1a(ReadOnlySpan<double> span)
        {
            ulong result = BASIS;

            foreach (var item in span)
            {
                ulong val = (ulong)item;

                result = (((val & 0x00000000000000FF) >> 00) ^ result) * PRIME;
                result = (((val & 0x000000000000FF00) >> 08) ^ result) * PRIME;
                result = (((val & 0x0000000000FF0000) >> 16) ^ result) * PRIME;
                result = (((val & 0x00000000FF000000) >> 24) ^ result) * PRIME;
                result = (((val & 0x000000FF00000000) >> 32) ^ result) * PRIME;
                result = (((val & 0x0000FF0000000000) >> 40) ^ result) * PRIME;
                result = (((val & 0x00FF000000000000) >> 48) ^ result) * PRIME;
                result = (((val & 0xFF00000000000000) >> 56) ^ result) * PRIME;
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static ulong FNV1a(ReadOnlySpan<int> span)
        {
            ulong result = BASIS;

            foreach (var item in span)
            {
                ulong val = (ulong)item;

                result = (((val & 0x000000FF) >> 00) ^ result) * PRIME;
                result = (((val & 0x0000FF00) >> 08) ^ result) * PRIME;
                result = (((val & 0x00FF0000) >> 16) ^ result) * PRIME;
                result = (((val & 0xFF000000) >> 24) ^ result) * PRIME;
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static ulong FNV1a(ReadOnlySpan<float> span)
        {
            ulong result = BASIS;

            foreach (var item in span)
            {
                ulong val = (ulong)item;

                result = (((val & 0x000000FF) >> 00) ^ result) * PRIME;
                result = (((val & 0x0000FF00) >> 08) ^ result) * PRIME;
                result = (((val & 0x00FF0000) >> 16) ^ result) * PRIME;
                result = (((val & 0xFF000000) >> 24) ^ result) * PRIME;
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        public static ulong FNV1a(ReadOnlySpan<char> span)
        {
            ulong result = BASIS;

            foreach (var item in span)
            {
                result = PRIME * (result ^ (byte)(item & 255));
                result = PRIME * (result ^ (byte)(item >> 8));
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        public static ulong FNV1a(ReadOnlySpan<short> text)
        {
            ulong result = BASIS;

            foreach (var c in text)
            {
                result = PRIME * (result ^ (byte)(c & 255));
                result = PRIME * (result ^ (byte)(c >> 8));
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        public static ulong FNV1a(ReadOnlySpan<ushort> text)
        {
            ulong result = BASIS;

            foreach (var c in text)
            {
                result = PRIME * (result ^ (byte)(c & 255));
                result = PRIME * (result ^ (byte)(c >> 8));
            }

            return result;
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static ulong FNV1a(ReadOnlySpan<byte> span)
        {
            ulong result = BASIS;
            int length = span.Length;

            for (int i = 0; i < length; ++i)
            {
                result = PRIME * (result ^ span[i]);
            }

            return result;
        }

        /// <summary>
        /// Returns the hash code of a specific item.
        /// </summary>
        /// <typeparam name="T1">The type of the item to add the hash code.</typeparam>
        /// <param name="item1">The item to add to the hash code.</param>
        /// <returns>The hash code that represents the single value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue64 Combine<T1>(T1 item1)
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
        public static HashValue64 Combine<T1, T2>(T1 item1, T2 item2)
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
        public static HashValue64 Combine<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
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
        public static HashValue64 Combine<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
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
        public static HashValue64 Combine<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
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
        public static HashValue64 Combine<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
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
        public static HashValue64 Combine<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
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
        public static HashValue64 Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
            => Combine(item1, item2, item3, item4, item5, item6, item7).Add(item8);

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The collection.</param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue64 CombineEach<T>(ReadOnlySpan<T> items)
            => new(GetHashCodeEach(BASIS, items));

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <typeparam name="TComparer">The type implements <see cref="IEqualityComparer{T}"/>.</typeparam>
        /// <param name="items">The collection.</param>
        /// <param name="comparer">
        /// The <typeparamref name="TComparer"/> to use to calculate the hash code.
        /// This value can be a null reference, in which case
        /// the internal <see cref="GetHashCode{T}(T)"/> will be used instead.
        /// </param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue64 CombineEach<T, TComparer>(ReadOnlySpan<T> items, [NotNull] TComparer comparer)
            where TComparer : IEqualityComparer<T>
            => new(GetHashCodeEach(BASIS, items, comparer));

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The collection.</param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue64 CombineEach<T>(IEnumerable<T> items)
            => items == null
            ? new(BASIS)
            : new(GetHashCodeEach(BASIS, items));

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The collection.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> to use to calculate the hash code.
        /// This value can be a null reference, which will use
        /// the default equality comparer for <typeparamref name="T"/>.
        /// </param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue64 CombineEach<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
            => items == null
            ? new(BASIS)
            : new(GetHashCodeEach(BASIS, items, comparer ?? EqualityComparer<T>.Default));

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="enumerator">The enumerator of a collection.</param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue64 CombineEach<T, TEnumerator>(TEnumerator enumerator)
            where TEnumerator : IEnumerator<T>
            => enumerator == null
            ? new(BASIS)
            : new(GetHashCodeEach<T, TEnumerator>(BASIS, enumerator));

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <typeparam name="TEnumerator">The type of the enumerator of the collection.</typeparam>
        /// <typeparam name="TComparer">The type implements <see cref="IEqualityComparer{T}"/>.</typeparam>
        /// <param name="enumerator">The enumerator of a collection.</param>
        /// <returns>The new hash code.</returns>
        /// <param name="comparer">
        /// The <typeparamref name="TComparer"/> to use to calculate the hash code.
        /// This value can be a null reference, in which case
        /// the internal <see cref="GetHashCode{T}(T)"/> will be used instead.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue64 CombineEach<T, TEnumerator, TComparer>(TEnumerator enumerator, [NotNull] TComparer comparer)
            where TEnumerator : IEnumerator<T>
            where TComparer : IEqualityComparer<T>
            => enumerator == null
            ? new(BASIS)
            : new(GetHashCodeEach<T, TEnumerator, TComparer>(BASIS, enumerator, comparer));

        /// <summary>
        /// Adds a single item to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="item">The item to add to the hash code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue64 Add<T>(T item)
            => new(CombineFNV1a(_value, GetHashCode(item)));

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
        public HashValue64 Add<T>(T item, IEqualityComparer<T> comparer)
            => new(CombineFNV1a(_value, (ulong)(comparer ?? EqualityComparer<T>.Default).GetHashCode(item)));

        /// <summary>
        /// Adds a single item to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <typeparam name="TComparer">The type implements <see cref="IEqualityComparer{T}"/>.</typeparam>
        /// <param name="item">The item to add to the hash code.</param>
        /// <param name="comparer">
        /// The <typeparamref name="TComparer"/> to use to calculate the hash code.
        /// This value can be a null reference, in which case
        /// the internal <see cref="GetHashCode{T}(T)"/> will be used instead.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue64 Add<T, TComparer>(T item, [NotNull] TComparer comparer)
            where TComparer : IEqualityComparer<T>
            => comparer == null
            ? new(CombineFNV1a(_value, GetHashCode(item)))
            : new(CombineFNV1a(_value, (ulong)comparer.GetHashCode(item)));

        /// <summary>
        /// Adds a collection of items to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="items">The collection of items to add to the hash code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue64 AddEach<T>(ReadOnlySpan<T> items)
            => new(GetHashCodeEach(_value, items));

        /// <summary>
        /// Adds a collection of items to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="items">The collection of items to add to the hash code.</param>
        /// <typeparam name="TComparer">The type implements <see cref="IEqualityComparer{T}"/>.</typeparam>
        /// <param name="comparer">
        /// The <typeparamref name="TComparer"/> to use to calculate the hash code.
        /// This value can be a null reference, in which case
        /// the internal <see cref="GetHashCode{T}(T)"/> will be used instead.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue64 AddEach<T, TComparer>(ReadOnlySpan<T> items, [NotNull] TComparer comparer)
            where TComparer : IEqualityComparer<T>
            => new(GetHashCodeEach(_value, items, comparer));

        /// <summary>
        /// Adds a collection of items to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="items">The collection of items to add to the hash code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue64 AddEach<T>(IEnumerable<T> items)
            => items == null
            ? new(_value)
            : new(GetHashCodeEach(_value, items));

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
        public HashValue64 AddEach<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
            => items == null
            ? new(_value)
            : new(GetHashCodeEach(_value, items, comparer ?? EqualityComparer<T>.Default));

        /// <summary>
        /// Adds a collection of items to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <typeparam name="TEnumerator">The type of the enumerator of the collection.</typeparam>
        /// <param name="enumerator">The enumerator of the collection.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue64 AddEach<T, TEnumerator>(TEnumerator enumerator)
            where TEnumerator : IEnumerator<T>
            => enumerator == null
            ? new(_value)
            : new(GetHashCodeEach<T, TEnumerator>(_value, enumerator));

        /// <summary>
        /// Adds a collection of items to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <typeparam name="TEnumerator">The type of the enumerator of the collection.</typeparam>
        /// <typeparam name="TComparer">The type implements <see cref="IEqualityComparer{T}"/>.</typeparam>
        /// <param name="enumerator">The enumerator of the collection.</param>
        /// <param name="comparer">
        /// The <typeparamref name="TComparer"/> to use to calculate the hash code.
        /// This value can be a null reference, in which case
        /// the internal <see cref="GetHashCode{T}(T)"/> will be used instead.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue64 AddEach<T, TEnumerator, TComparer>(TEnumerator enumerator, [NotNull] TComparer comparer)
            where TEnumerator : IEnumerator<T>
            where TComparer : IEqualityComparer<T>
            => enumerator == null
            ? new(_value)
            : new(GetHashCodeEach<T, TEnumerator, TComparer>(_value, enumerator, comparer));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(HashValue64 other)
            => _value.Equals(other._value);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is HashValue64 other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ToHashCode()
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

        /// <summary>
        /// Combines a FNV1a hash with a value.
        /// </summary>
        /// <param name="hash">Input Hash.</param>
        /// <param name="value">Value to add to the hash.</param>
        /// <returns>A combined FNV1a hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong CombineFNV1a(ulong hash, ulong value)
        {
            return (hash ^ value) * PRIME;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetHashCode<T>(T item)
            => (ulong)(item?.GetHashCode() ?? 0);

        private static ulong GetHashCodeEach<T>(
              ulong startHashCode
            , ReadOnlySpan<T> items
        )
        {
            var result = startHashCode;
            var enumerator = items.GetEnumerator();

            if (enumerator.MoveNext() == false)
            {
                return CombineFNV1a(result, EMPTY_COLLECTION_BASIS);
            }

            result = CombineFNV1a(result, (ulong)GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineFNV1a(result, (ulong)GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static ulong GetHashCodeEach<T, TComparer>(
              ulong startHashCode
            , ReadOnlySpan<T> items
            , TComparer comparer
        )
            where TComparer : IEqualityComparer<T>
        {
            var result = startHashCode;
            var enumerator = items.GetEnumerator();

            if (enumerator.MoveNext() == false)
            {
                return CombineFNV1a(result, EMPTY_COLLECTION_BASIS);
            }

            result = CombineFNV1a(result, (ulong)comparer.GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineFNV1a(result, (ulong)comparer.GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static ulong GetHashCodeEach<T>(
              ulong startHashCode
            , IEnumerable<T> items
        )
        {
            var result = startHashCode;
            var enumerator = items.GetEnumerator();

            if (enumerator.MoveNext() == false)
            {
                return CombineFNV1a(result, EMPTY_COLLECTION_BASIS);
            }

            result = CombineFNV1a(result, (ulong)GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineFNV1a(result, (ulong)GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static ulong GetHashCodeEach<T>(
              ulong startHashCode
            , IEnumerable<T> items
            , IEqualityComparer<T> comparer
        )
        {
            var result = startHashCode;
            var enumerator = items.GetEnumerator();

            if (enumerator.MoveNext() == false)
            {
                return CombineFNV1a(result, EMPTY_COLLECTION_BASIS);
            }

            result = CombineFNV1a(result, (ulong)comparer.GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineFNV1a(result, (ulong)comparer.GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static ulong GetHashCodeEach<T, TEnumerator>(
              ulong startHashCode
            , TEnumerator enumerator
        )
            where TEnumerator : IEnumerator<T>
        {
            var result = startHashCode;

            if (enumerator.MoveNext() == false)
            {
                return CombineFNV1a(result, EMPTY_COLLECTION_BASIS);
            }

            result = CombineFNV1a(result, GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineFNV1a(result, GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static ulong GetHashCodeEach<T, TEnumerator, TComparer>(
              ulong startHashCode
            , TEnumerator enumerator
            , TComparer comparer
        )
            where TEnumerator : IEnumerator<T>
            where TComparer : IEqualityComparer<T>
        {
            var result = startHashCode;

            if (enumerator.MoveNext() == false)
            {
                return CombineFNV1a(result, EMPTY_COLLECTION_BASIS);
            }

            if (comparer != null)
            {
                result = CombineFNV1a(result, (ulong)comparer.GetHashCode(enumerator.Current));

                while (enumerator.MoveNext())
                {
                    result = CombineFNV1a(result, (ulong)comparer.GetHashCode(enumerator.Current));
                }
            }
            else
            {
                result = CombineFNV1a(result, GetHashCode(enumerator.Current));

                while (enumerator.MoveNext())
                {
                    result = CombineFNV1a(result, GetHashCode(enumerator.Current));
                }
            }

            return result;
        }
    }
}
