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

namespace EncosyTower.Entities.Stats
{
    [Serializable]
    public struct StatModifierHandle : IEquatable<StatModifierHandle>
    {
        public StatHandle affectedStatHandle;
        public uint modifierId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatModifierHandle(StatHandle affectedStatHandle, uint modifierId)
        {
            this.affectedStatHandle = affectedStatHandle;
            this.modifierId = modifierId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(StatModifierHandle other)
        {
            return affectedStatHandle == other.affectedStatHandle && modifierId == other.modifierId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
        {
            return obj is StatModifierHandle other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(affectedStatHandle, modifierId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StatModifierHandle left, StatModifierHandle right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StatModifierHandle left, StatModifierHandle right)
        {
            return left.Equals(right) == false;
        }
    }
}
