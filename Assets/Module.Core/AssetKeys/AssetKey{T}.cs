using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Module.Core
{
    public readonly partial struct AssetKey<T> : IEquatable<AssetKey<T>>, IEquatable<AssetKey>
    {
        private readonly AssetKey _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetKey(string value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetKey(AssetKey value)
        {
            _value = value;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AssetKey<T> other)
            => _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AssetKey other)
            => _value.Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is AssetKey<T> otherT
                ? _value.Equals(otherT._value)
                : obj is AssetKey other && _value.Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey(AssetKey<T> value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(AssetKey<T> value)
            => value._value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey<T>(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey<T>(AssetKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AssetKey<T> left, AssetKey<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AssetKey<T> left, AssetKey<T> right)
            => !left.Equals(right);
    }

    partial struct AssetKey
    {
        [Serializable]
        public partial struct Serializable<T> : ITryConvert<AssetKey<T>>
            , IEquatable<Serializable<T>>
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

            public readonly bool TryConvert(out AssetKey<T> result)
            {
                result = new(_value);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable<T> other)
                => string.Equals(_value, other._value, StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable<T> other && Equals(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _value.GetHashCode(StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => _value ?? string.Empty;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator string(Serializable<T> value)
                => value._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(string value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(Serializable value)
                => new(value.Value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator AssetKey<T>(Serializable<T> value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(AssetKey<T> value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable<T> left, Serializable<T> right)
                => left._value == right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable<T> left, Serializable<T> right)
                => left._value != right._value;
        }
    }
}
