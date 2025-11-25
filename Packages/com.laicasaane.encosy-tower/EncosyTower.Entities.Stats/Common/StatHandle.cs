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
        public Entity entity;
        public int index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatHandle(Entity entity, int index)
        {
            this.entity = entity;
            this.index = index;
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
        public Entity entity;
        public int index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatHandle(Entity entity, int index)
        {
            this.entity = entity;
            this.index = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StatHandle<TStatData>(StatHandle value)
        {
            return new(value.entity, value.index);
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
