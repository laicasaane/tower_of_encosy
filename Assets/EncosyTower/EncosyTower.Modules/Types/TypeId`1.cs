namespace EncosyTower.Modules
{
    using System;
    using System.Runtime.CompilerServices;

    public readonly partial struct TypeId<T> : IEquatable<TypeId<T>>
    {
        private static readonly TypeId<T> s_value;

        static TypeId()
        {
            s_value = (TypeId<T>)TypeCache.GetId<T>();
        }

        internal readonly uint _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TypeId(uint value)
        {
            _value = value;
        }

        public static TypeId<T> Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TypeId<T> other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is TypeId<T> other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id<T>(in TypeId<T> id)
            => id._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator TypeId<T>(in TypeId id)
            => new(id._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator TypeId(in TypeId<T> id)
            => new(id._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value == rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value != rhs._value;
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Modules
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct TypeId<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_value);
            return fs;
        }
    }
}

#endif
