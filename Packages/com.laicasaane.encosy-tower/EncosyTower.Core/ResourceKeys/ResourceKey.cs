using System;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    [Serializable]
    public partial struct ResourceKey : IEquatable<ResourceKey>
    {
        [SerializeField] internal AssetKey _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ResourceKey(AssetKey value)
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
        public readonly bool Equals(ResourceKey other)
            => _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is ResourceKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(ResourceKey value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ResourceKey left, ResourceKey right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ResourceKey left, ResourceKey right)
            => !left.Equals(right);
    }

    [Serializable]
    public partial struct ResourceKey<T> : IEquatable<ResourceKey<T>>, IEquatable<ResourceKey>
        where T : UnityEngine.Object
    {
        [SerializeField] internal AssetKey<T> _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ResourceKey(AssetKey<T> value)
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
        public readonly bool Equals(ResourceKey<T> other)
            => _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ResourceKey other)
            => _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is ResourceKey<T> otherT
                ? _value.Equals(otherT._value)
                : obj is ResourceKey other && _value.Equals(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(ResourceKey<T> value)
            => value.Value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(ResourceKey value)
            => new(value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ResourceKey(ResourceKey<T> value)
            => new((string)value.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(AssetKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ResourceKey<T>(AssetKey<T> value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AssetKey(ResourceKey<T> value)
            => (AssetKey)value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ResourceKey<T> left, ResourceKey<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ResourceKey<T> left, ResourceKey<T> right)
            => !left.Equals(right);
    }
}
