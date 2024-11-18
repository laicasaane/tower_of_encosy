using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules
{
    public readonly struct Option<T> : IEquatable<Option<T>>
    {
        private readonly T _value;

        public readonly ByteBool HasValue;

        public Option(T value)
        {
            _value = value;

            HasValue = value is UnityEngine.Object obj
                ? (ByteBool)(obj == true)
                : value is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out bool hasValue, out T value)
        {
            hasValue = HasValue;
            value = _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Value()
        {
            ThrowIfHasNoValue(HasValue);
            return _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryValue(out T value)
        {
            value = HasValue ? _value : default;
            return HasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ValueOrDefault(T defaultValue = default)
            => HasValue ? _value : defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Option<T> other)
            => HasValue == other.HasValue && EqualityComparer<T>.Default.Equals(_value, other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Option<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(HasValue, _value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => HasValue ? $"Optional+Value({_value})" : "Optional+None";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Option<T>(T value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Option<T> left, Option<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Option<T> left, Option<T> right)
            => !left.Equals(right);

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfHasNoValue([DoesNotReturnIf(false)] bool check)
        {
            if (check == false)
            {
                throw new InvalidOperationException($"The instance of Option<{typeof(T)}> has no value");
            }
        }
    }
}
