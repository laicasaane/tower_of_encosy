namespace EncosyTower.Scenes
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Common;
    using UnityEngine;

    [Serializable]
    public partial struct SceneBuildIndex
        : IEquatable<SceneBuildIndex>
        , IComparable<SceneBuildIndex>
        , IComparable
        , ISpanFormattable
    {
        [SerializeField] private int _index;
        [SerializeField] private string _name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SceneBuildIndex(int index, string name)
        {
            _index = index;
            _name = name;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _name.IsNotEmpty();
        }

        public readonly int Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _index;
        }

        public readonly string Name
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(SceneBuildIndex other)
            => _index == other._index && string.Equals(_name, other._name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj)
            => obj is SceneBuildIndex other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(SceneBuildIndex other)
            => _index.CompareTo(other._index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(object obj)
            => obj is SceneBuildIndex other ? CompareTo(other) : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => Index.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly string ToString()
            => Name ?? string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
            => Name?.ToString(formatProvider) ?? string.Empty;

        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            var src = ToString().AsSpan();

            if (src.Length > destination.Length)
            {
                charsWritten = 0;
                return false;
            }

            src.CopyTo(destination);
            charsWritten = src.Length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SceneBuildIndex left, SceneBuildIndex right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SceneBuildIndex left, SceneBuildIndex right)
            => left.Equals(right) == false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(SceneBuildIndex left, SceneBuildIndex right)
            => left.CompareTo(right) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(SceneBuildIndex left, SceneBuildIndex right)
            => left.CompareTo(right) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(SceneBuildIndex left, SceneBuildIndex right)
            => left.CompareTo(right) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(SceneBuildIndex left, SceneBuildIndex right)
            => left.CompareTo(right) >= 0;
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Scenes
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using Unity.Collections;

    partial struct SceneBuildIndex : IToFixedString<FixedString128Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString128Bytes ToFixedString()
        {
            var fs = new FixedString128Bytes();
            fs.Append(ToString());
            return fs;
        }
    }
}

#endif
