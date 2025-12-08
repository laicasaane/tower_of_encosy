// MIT License
//
// Copyright (c) 2023 Philippe St-Amand
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Runtime.CompilerServices;
using EncosyTower.TypeWraps;

namespace EncosyTower.Entities.Stats
{
    [Serializable, WrapType(typeof(int), nameof(value))]
    public partial struct StatIndex
    {
        public static readonly StatIndex Null = default;

        /// <summary>
        /// The index associated with a specific DynamicBuffer&lt;Stat&gt;.
        /// </summary>
        /// <remarks>
        /// A valid index should be greater than 0.
        /// Index 0 equals to the first element in DynamicBuffer&lt;Stat&gt;
        /// whose type is <see cref="StatVariantType.None"/>.
        /// </remarks>
        public int value;

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Equals(Null) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint(StatIndex value)
            => (uint)value.value;
    }

    [Serializable, WrapType(typeof(int), nameof(value))]
    public partial struct StatIndex<TStatData>
        where TStatData : unmanaged, IStatData
    {
        public static readonly StatIndex<TStatData> Null = default;

        /// <inheritdoc cref="StatIndex.value"/>
        public int value;

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Equals(Null) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator StatIndex<TStatData>(StatIndex value)
        {
            return new(value.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StatIndex(StatIndex<TStatData> value)
        {
            return new(value.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint(StatIndex<TStatData> value)
            => (uint)value.value;
    }
}
