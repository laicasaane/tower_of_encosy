using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class ValueRef
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueRef<T> Create<T>() where T : struct
            => new(Guid.NewGuid());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueRef<T> Create<T>(ref T value) where T : struct
            => new(Guid.NewGuid(), ref value);
    }

    public sealed class ValueRef<T> where T : struct
    {
        public readonly Guid Id;

        private T _value;

        internal ValueRef(in Guid id)
        {
            Id = id;
            Value = default;
        }

        internal ValueRef(Guid id, ref T value) : this(id)
        {
            Value = value;
        }

        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(ValueRef<T> refT)
            => refT?._value ?? default;
    }
}
