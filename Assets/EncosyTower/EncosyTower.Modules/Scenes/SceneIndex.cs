using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Scenes
{
    public readonly struct SceneIndex : IEquatable<SceneIndex>
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
            get => string.IsNullOrEmpty(Name) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SceneIndex other)
            => Index == other.Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is SceneIndex other && Index == other.Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Index.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => Name ?? string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SceneIndex left, SceneIndex right)
            => left.Index == right.Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SceneIndex left, SceneIndex right)
            => left.Index != right.Index;

        [Serializable]
        public struct Serializable : ITryConvert<SceneIndex>
            , IEquatable<Serializable>
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

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => string.IsNullOrEmpty(_name) == false;
            }

            public int Index
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _index;
            }

            public string Name
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
            public override readonly bool Equals(object obj)
                => obj is Serializable other && _index == other._index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _index.GetHashCode();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => _name ?? string.Empty;

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