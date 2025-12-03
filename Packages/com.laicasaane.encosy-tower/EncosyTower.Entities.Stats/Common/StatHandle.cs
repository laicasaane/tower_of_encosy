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
using EncosyTower.Common;
using Unity.Entities;

namespace EncosyTower.Entities.Stats
{
    [Serializable]
    public struct StatHandle : IEquatable<StatHandle>
    {
        public static readonly StatHandle Null = default;

        public Entity entity;

        /// <summary>
        /// The index associated with a specific DynamicBuffer&lt;Stat&gt;.
        /// </summary>
        /// <remarks>
        /// A valid index should be greater than 0.
        /// Index 0 equals to the first element in DynamicBuffer&lt;Stat&gt;
        /// whose type is <see cref="StatVariantType.None"/>.
        /// </remarks>
        public StatIndex index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatHandle(Entity entity, StatIndex index)
        {
            this.entity = entity;
            this.index = index;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Equals(Null) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StatHandle left, StatHandle right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StatHandle left, StatHandle right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(out Entity entity, out StatIndex index)
        {
            entity = this.entity;
            index = this.index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(StatHandle other)
        {
            return entity.Equals(other.entity) && index == other.index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
        {
            return obj is StatHandle other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            return HashValue.Combine(index, entity);
        }
    }

    [Serializable]
    public struct StatHandle<TStatData> : IEquatable<StatHandle<TStatData>>
        where TStatData : unmanaged, IStatData
    {
        public static readonly StatHandle<TStatData> Null = default;

        public Entity entity;

        /// <inheritdoc cref="StatHandle.index"/>
        public StatIndex<TStatData> index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatHandle(Entity entity, StatIndex<TStatData> index)
        {
            this.entity = entity;
            this.index = index;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Equals(Null) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator StatHandle<TStatData>(StatHandle value)
        {
            return new(value.entity, (StatIndex<TStatData>)value.index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StatHandle(StatHandle<TStatData> value)
        {
            return new(value.entity, value.index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StatHandle<TStatData> left, StatHandle<TStatData> right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StatHandle<TStatData> left, StatHandle<TStatData> right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(out Entity entity, out StatIndex<TStatData> index)
        {
            entity = this.entity;
            index = this.index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(StatHandle<TStatData> other)
        {
            return entity.Equals(other.entity) && index == other.index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
        {
            return obj is StatHandle<TStatData> other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            return HashValue.Combine(index, entity);
        }
    }
}
