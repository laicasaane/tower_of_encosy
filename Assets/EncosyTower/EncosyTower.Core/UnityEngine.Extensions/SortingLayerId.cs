using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    [Serializable]
    public struct SortingLayerId : IEquatable<SortingLayerId>
    {
        public int id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SortingLayerId(int value)
        {
            id = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SortingLayerId(string name)
        {
            id = SortingLayer.NameToID(name);
        }

        public readonly int Layer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SortingLayer.GetLayerValueFromID(id);
        }

        public readonly string Name
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SortingLayer.IDToName(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => id.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is SortingLayerId other && id == other.id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(SortingLayerId other)
            => id == other.id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => id.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(SortingLayerId value)
            => value.id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SortingLayerId(int value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SortingLayerId(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SortingLayerId lhs, SortingLayerId rhs)
            => lhs.id == rhs.id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SortingLayerId lhs, SortingLayerId rhs)
            => lhs.id != rhs.id;
    }
}
