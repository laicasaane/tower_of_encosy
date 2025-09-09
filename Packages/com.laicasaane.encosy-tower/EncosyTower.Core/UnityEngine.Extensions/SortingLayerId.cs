using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    [Serializable]
    public struct SortingLayerId : IEquatable<SortingLayerId>
    {
        public int value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SortingLayerId(int value)
        {
            this.value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SortingLayerId(string name)
        {
            value = SortingLayer.NameToID(name);
        }

        public readonly string Name
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SortingLayer.IDToName(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is SortingLayerId other && value == other.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(SortingLayerId other)
            => value == other.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(SortingLayerId value)
            => value.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SortingLayerId(int value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SortingLayerId(string name)
            => new(name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SortingLayerId lhs, SortingLayerId rhs)
            => lhs.value == rhs.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SortingLayerId lhs, SortingLayerId rhs)
            => lhs.value != rhs.value;
    }
}
