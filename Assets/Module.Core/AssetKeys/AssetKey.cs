using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Module.Core
{
    public readonly partial struct AssetKey : IEquatable<AssetKey>
    {
        private readonly string _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetKey(string value)
        {
            _value = value;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.IsNullOrEmpty(_value) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AssetKey other)
            => string.Equals(_value, other._value, StringComparison.Ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is AssetKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value ?? string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(AssetKey value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AssetKey left, AssetKey right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AssetKey left, AssetKey right)
            => !left.Equals(right);

        [Serializable]
        public partial struct Serializable : ITryConvert<AssetKey>
            , IEquatable<Serializable>
        {
            [SerializeField]
            private string _value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(string value)
            {
                _value = value;
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => string.IsNullOrEmpty(_value) == false;
            }

            public string Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out AssetKey result)
            {
                result = new(_value);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => string.Equals(_value, other._value, StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable other && Equals(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _value.GetHashCode(StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => _value ?? string.Empty;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator string(Serializable value)
                => value._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(string value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator AssetKey(Serializable value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(AssetKey value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => string.Equals(left._value, right._value, StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => !string.Equals(left._value, right._value, StringComparison.Ordinal);
        }
    }
}
