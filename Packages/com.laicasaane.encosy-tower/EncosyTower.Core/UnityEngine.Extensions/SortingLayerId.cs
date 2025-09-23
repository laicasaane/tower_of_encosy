using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    [Serializable]
    public struct SortingLayerId
        : IEquatable<SortingLayerId>
        , IComparable<SortingLayerId>
        , IComparable
        , ISpanFormattable
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
        public string ToString(string format, IFormatProvider formatProvider = null)
            => value.ToString(format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(SortingLayerId other)
            => value.CompareTo(other.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(object obj)
            => obj is SortingLayerId other ? value.CompareTo(other.value) : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            return value.TryFormat(destination, out charsWritten, format, provider);
        }

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
