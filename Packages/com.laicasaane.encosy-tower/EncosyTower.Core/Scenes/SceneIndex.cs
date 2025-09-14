namespace EncosyTower.Scenes
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Common;
    using EncosyTower.Conversion;
    using UnityEngine;

    public readonly partial struct SceneIndex
        : IEquatable<SceneIndex>
        , IComparable<SceneIndex>
        , IComparable
        , ISpanFormattable
    {
        public readonly int Index;
        public readonly string Name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SceneIndex(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Name.IsNotEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SceneIndex other)
            => Index == other.Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is SceneIndex other && Index == other.Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(SceneIndex other)
            => Index.CompareTo(other.Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object obj)
            => obj is SceneIndex other ? Index.CompareTo(other.Index) : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Index.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => Name ?? string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
            => Name?.ToString(formatProvider) ?? string.Empty;

        public bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format
            , IFormatProvider provider
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
        public static bool operator ==(SceneIndex left, SceneIndex right)
            => left.Index == right.Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SceneIndex left, SceneIndex right)
            => left.Index != right.Index;

        [Serializable]
        public partial struct Serializable : ITryConvert<SceneIndex>
            , IEquatable<Serializable>
            , IComparable<Serializable>
            , IComparable
            , ISpanFormattable
        {
            [SerializeField]
            private int _index;

            [SerializeField]
            private string _name;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(int index, string name)
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
            public readonly bool TryConvert(out SceneIndex result)
            {
                result = new(_index, _name);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => _index == other._index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override bool Equals(object obj)
                => obj is Serializable other && _index == other._index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int CompareTo(Serializable other)
                => Index.CompareTo(other.Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int CompareTo(object obj)
                => obj is Serializable other ? Index.CompareTo(other.Index) : 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override int GetHashCode()
                => _index.GetHashCode();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override string ToString()
                => _name ?? string.Empty;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly string ToString(string format, IFormatProvider formatProvider)
                => ToString().ToString(formatProvider);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryFormat(
                  Span<char> destination
                , out int charsWritten
                , ReadOnlySpan<char> format
                , IFormatProvider provider
            )
            {
                return ((SceneIndex)this).TryFormat(destination, out charsWritten, format, provider);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SceneIndex(Serializable value)
                => new(value._index, value._name);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(SceneIndex value)
                => new(value.Index, value.Name);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left._index == right._index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => left._index != right._index;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Scenes
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using Unity.Collections;

    partial struct SceneIndex : IToFixedString<FixedString128Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString128Bytes ToFixedString()
        {
            var fs = new FixedString128Bytes();
            fs.Append(ToString());
            return fs;
        }

        partial struct Serializable : IToFixedString<FixedString128Bytes>
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
}

#endif
