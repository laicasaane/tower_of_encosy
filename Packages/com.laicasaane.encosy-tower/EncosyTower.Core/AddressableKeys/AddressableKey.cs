#if UNITY_ADDRESSABLES

using System;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using UnityEngine;

namespace EncosyTower.AddressableKeys
{
    [Serializable]
    public partial struct AddressableKey : IEquatable<AddressableKey>
    {
        [SerializeField] internal AssetKey _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AddressableKey(AssetKey value)
        {
            _value = value;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsValid;
        }

        public readonly AssetKey Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AddressableKey other)
            => _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is AddressableKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(AddressableKey value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AddressableKey left, AddressableKey right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AddressableKey left, AddressableKey right)
            => !left.Equals(right);
    }

    [Serializable]
    public partial struct AddressableKey<T> : IEquatable<AddressableKey<T>>, IEquatable<AddressableKey>
    {
        [SerializeField] internal AssetKey<T> _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AddressableKey(AssetKey<T> value)
        {
            _value = value;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsValid;
        }

        public readonly AssetKey<T> Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AddressableKey<T> other)
            => _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AddressableKey other)
            => _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is AddressableKey<T> otherT
                ? _value.Equals(otherT._value)
                : obj is AddressableKey other && _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(AddressableKey<T> value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AddressableKey value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AddressableKey(AddressableKey<T> value)
            => new((string)value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AssetKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AddressableKey<T>(AssetKey<T> value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AssetKey(AddressableKey<T> value)
            => (AssetKey)value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AddressableKey<T> left, AddressableKey<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AddressableKey<T> left, AddressableKey<T> right)
            => !left.Equals(right);
    }
}

#endif
