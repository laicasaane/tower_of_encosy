using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public readonly struct Scoped<TScope, TValue> : IEquatable<Scoped<TScope, TValue>>
        where TValue : IEquatable<TValue>
    {
        public readonly TValue Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scoped([NotNull] TValue value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Scoped<TScope, TValue> other)
            => Value.Equals(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Scoped<TScope, TValue> other && Value.Equals(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Scoped<TScope, TValue>([NotNull] TValue value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TValue(Scoped<TScope, TValue> value)
            => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Scoped<TScope, TValue> left, Scoped<TScope, TValue> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Scoped<TScope, TValue> left, Scoped<TScope, TValue> right)
            => !left.Equals(right);
    }
}
