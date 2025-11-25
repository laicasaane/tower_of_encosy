// MIT License
// Author: Muhammad Rehan Saeed
// https://github.com/RehanSaeed/rehansaeed.github.io/blob/19b2e092711e44e4e5995dbd703c05d0a500adbb/content/posts/2014/gethashcode-made-easy/index.md

// FNV Hash Reference
// https://gist.github.com/StephenCleary/4f6568e5ab5bee7845943fdaef8426d2

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EncosyTower.SourceGen
{
    /// <summary>
    /// Combines the hash code for multiple values into a single hash code
    /// to help with implementing <see cref="object.GetHashCode()"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="HashValue"/> provides an alternative to <see cref="System.HashCode"/>.
    /// Because, by design, <see cref="System.HashCode"/> is unstable within Burst.</item>
    /// <item><see cref="HashValue"/> uses Fowler/Noll/Vo hash algorithm (FNV-1a) for a decent distribution.</item>
    /// </list>
    /// </remarks>
    /// <seealso href="https://discussions.unity.com/t/hashcode-combine/1682736/10"/>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/csharp-system-support.html#systemhashcode"/>
    public readonly partial struct HashValue : IEquatable<HashValue>
    {
        /// <summary>
        /// The prime number used to compute the FNV hash 32 bits.
        /// </summary>
        public const uint PRIME = 16777619;

        /// <summary>
        /// The starting point of the FNV hash 32 bits.
        /// </summary>
        public const int BASIS = unchecked((int)2166136261);

        /// <summary>
        /// The prime number used in case the collection is null or empty.
        /// </summary>
        public const int EMPTY_COLLECTION_BASIS = 1111111231;

        private readonly int  _value = BASIS;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashValue"/> struct.
        /// </summary>
        /// <param name="value">The starting point of the FNV hash.</param>
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
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(long value)
            => FNV1a((ulong)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(double value)
            => FNV1a((ulong)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(ulong value)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

                result = (((value & 0x00000000000000FF) >> 00) ^ result) * PRIME;
                result = (((value & 0x000000000000FF00) >> 08) ^ result) * PRIME;
                result = (((value & 0x0000000000FF0000) >> 16) ^ result) * PRIME;
                result = (((value & 0x00000000FF000000) >> 24) ^ result) * PRIME;
                result = (((value & 0x000000FF00000000) >> 32) ^ result) * PRIME;
                result = (((value & 0x0000FF0000000000) >> 40) ^ result) * PRIME;
                result = (((value & 0x00FF000000000000) >> 48) ^ result) * PRIME;
                result = (((value & 0xFF00000000000000) >> 56) ^ result) * PRIME;

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        public static int FNV1a(float value)
            => FNV1a((uint)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        public static int FNV1a(int value)
            => FNV1a((uint)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        public static int FNV1a(uint value)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;
                ulong val = value;

                result = (((val & 0x000000FF) >> 00) ^ result) * PRIME;
                result = (((val & 0x0000FF00) >> 08) ^ result) * PRIME;
                result = (((val & 0x00FF0000) >> 16) ^ result) * PRIME;
                result = (((val & 0xFF000000) >> 24) ^ result) * PRIME;

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(short value)
            => FNV1a((ushort)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(ushort value)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;
                ulong val = value;

                result = ((val & 255) ^ result) * PRIME;
                result = ((val >> 08) ^ result) * PRIME;

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(sbyte value)
            => FNV1a((byte)value);

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(byte value)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;
                ulong val = value;

                result = (val ^ result) * PRIME;

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FNV1a(string text)
        {
            return FNV1a(text.AsSpan());
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static int FNV1a(ReadOnlySpan<long> span)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

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

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static int FNV1a(ReadOnlySpan<ulong> span)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

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

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static int FNV1a(ReadOnlySpan<double> span)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

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

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static int FNV1a(ReadOnlySpan<int> span)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

                foreach (var item in span)
                {
                    ulong val = (ulong)item;

                    result = (((val & 0x000000FF) >> 00) ^ result) * PRIME;
                    result = (((val & 0x0000FF00) >> 08) ^ result) * PRIME;
                    result = (((val & 0x00FF0000) >> 16) ^ result) * PRIME;
                    result = (((val & 0xFF000000) >> 24) ^ result) * PRIME;
                }

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static int FNV1a(ReadOnlySpan<float> span)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

                foreach (var item in span)
                {
                    ulong val = (ulong)item;

                    result = (((val & 0x000000FF) >> 00) ^ result) * PRIME;
                    result = (((val & 0x0000FF00) >> 08) ^ result) * PRIME;
                    result = (((val & 0x00FF0000) >> 16) ^ result) * PRIME;
                    result = (((val & 0xFF000000) >> 24) ^ result) * PRIME;
                }

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        public static int FNV1a(ReadOnlySpan<char> span)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

                foreach (var item in span)
                {
                    result = PRIME * (result ^ (byte)(item & 255));
                    result = PRIME * (result ^ (byte)(item >> 8));
                }

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        public static int FNV1a(ReadOnlySpan<short> text)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

                foreach (var c in text)
                {
                    result = PRIME * (result ^ (byte)(c & 255));
                    result = PRIME * (result ^ (byte)(c >> 8));
                }

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <returns>Hash of input string.</returns>
        public static int FNV1a(ReadOnlySpan<ushort> text)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;

                foreach (var c in text)
                {
                    result = PRIME * (result ^ (byte)(c & 255));
                    result = PRIME * (result ^ (byte)(c >> 8));
                }

                return (int)result;
            }
        }

        /// <summary>
        /// Generates a FNV1a hash.
        /// </summary>
        /// <param name="span">Span to hash.</param>
        /// <returns>Hash of input span.</returns>
        public static int FNV1a(ReadOnlySpan<byte> span)
        {
            unchecked
            {
                ulong result = (ulong)BASIS;
                int length = span.Length;

                for (var i = 0; i < length; i++)
                {
                    result = PRIME * (result ^ span[i]);
                }

                return (int)result;
            }
        }

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
        public static HashValue CombineEach<T>(ReadOnlySpan<T> items)
            => items == null
            ? new(BASIS)
            : new(GetHashCodeEach(BASIS, items));

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
        public static HashValue CombineEach<T, TComparer>(ReadOnlySpan<T> items, TComparer comparer)
            where TComparer : IEqualityComparer<T>
            => items == null
            ? new(BASIS)
            : new(GetHashCodeEach(BASIS, items, comparer));

        /// <summary>
        /// Takes the hash code of the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The collection.</param>
        /// <returns>The new hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashValue CombineEach<T>(IEnumerable<T> items)
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
        public static HashValue CombineEach<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
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
        public static HashValue CombineEach<T, TEnumerator>(TEnumerator enumerator)
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
        public static HashValue CombineEach<T, TEnumerator, TComparer>(TEnumerator enumerator, TComparer comparer)
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
        public HashValue Add<T>(T item)
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
        public HashValue Add<T>(T item, IEqualityComparer<T> comparer)
            => new(CombineFNV1a(_value, (comparer ?? EqualityComparer<T>.Default).GetHashCode(item)));

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
        public HashValue Add<T, TComparer>(T item, TComparer comparer)
            where TComparer : IEqualityComparer<T>
            => comparer == null
            ? new(CombineFNV1a(_value, GetHashCode(item)))
            : new(CombineFNV1a(_value, comparer.GetHashCode(item)));

        /// <summary>
        /// Adds a collection of items to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="items">The collection of items to add to the hash code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue AddEach<T>(ReadOnlySpan<T> items)
            => items == null
            ? new(_value)
            : new(GetHashCodeEach(_value, items));

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
        public HashValue AddEach<T, TComparer>(ReadOnlySpan<T> items, TComparer comparer)
            where TComparer : IEqualityComparer<T>
            => items == null
            ? new(_value)
            : new(GetHashCodeEach(_value, items, comparer));

        /// <summary>
        /// Adds a collection of items to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the item to add to the hash code.</typeparam>
        /// <param name="items">The collection of items to add to the hash code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue AddEach<T>(IEnumerable<T> items)
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
        public HashValue AddEach<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
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
        public HashValue AddEach<T, TEnumerator>(TEnumerator enumerator)
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
        public HashValue AddEach<T, TEnumerator, TComparer>(TEnumerator enumerator, TComparer comparer)
            where TEnumerator : IEnumerator<T>
            where TComparer : IEqualityComparer<T>
            => enumerator == null
            ? new(_value)
            : new(GetHashCodeEach<T, TEnumerator, TComparer>(_value, enumerator, comparer));

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

        /// <summary>
        /// Combines a FNV1a hash with a value.
        /// </summary>
        /// <param name="hash">Input Hash.</param>
        /// <param name="value">Value to add to the hash.</param>
        /// <returns>A combined FNV1a hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CombineFNV1a(int hash, int value)
        {
            return (int)((hash ^ value) * PRIME);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHashCode<T>(T item)
            => item?.GetHashCode() ?? 0;

        private static int GetHashCodeEach<T>(
              int startHashCode
            , ReadOnlySpan<T> items
        )
        {
            var result = startHashCode;
            var enumerator = items.GetEnumerator();

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

        private static int GetHashCodeEach<T, TComparer>(
              int startHashCode
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

            result = CombineFNV1a(result, comparer.GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineFNV1a(result, comparer.GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static int GetHashCodeEach<T>(
              int startHashCode
            , IEnumerable<T> items
        )
        {
            var result = startHashCode;
            var enumerator = items.GetEnumerator();

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

        private static int GetHashCodeEach<T>(
              int startHashCode
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

            result = CombineFNV1a(result, comparer.GetHashCode(enumerator.Current));

            while (enumerator.MoveNext())
            {
                result = CombineFNV1a(result, comparer.GetHashCode(enumerator.Current));
            }

            return result;
        }

        private static int GetHashCodeEach<T, TEnumerator>(
              int startHashCode
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

        private static int GetHashCodeEach<T, TEnumerator, TComparer>(
              int startHashCode
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
                result = CombineFNV1a(result, comparer.GetHashCode(enumerator.Current));

                while (enumerator.MoveNext())
                {
                    result = CombineFNV1a(result, comparer.GetHashCode(enumerator.Current));
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
