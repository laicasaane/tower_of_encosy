using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Module.Core
{
    public readonly struct T<TScope, TValue> : IEquatable<T<TScope, TValue>>
        where TValue : IEquatable<TValue>
    {
        public readonly TValue Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T([NotNull] TValue value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(T<TScope, TValue> other)
            => Value.Equals(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is T<TScope, TValue> other && Value.Equals(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T<TScope, TValue>([NotNull] TValue value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TValue(T<TScope, TValue> value)
            => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(T<TScope, TValue> left, T<TScope, TValue> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(T<TScope, TValue> left, T<TScope, TValue> right)
            => !left.Equals(right);
    }
}
