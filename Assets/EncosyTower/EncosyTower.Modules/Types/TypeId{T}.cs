namespace EncosyTower.Modules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public readonly partial struct TypeId<T> : IEquatable<TypeId<T>>
    {
        public static readonly TypeId Undefined = default;

        internal readonly uint _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeId(uint value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type ToType()
            => TypeCache<T>.Type;

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
        public static bool operator ==(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value == rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in TypeId<T> lhs, in TypeId<T> rhs)
            => lhs._value != rhs._value;

        public static TypeId<T> Get()
        {
            var id = new TypeId<T>(TypeIdVault.Cache<T>.Id);
            TypeIdVault.Register(id._value, TypeCache<T>.Type);
            return id;
        }

        [HideInCallstack, DoesNotReturn]
        private static void ThrowIfTypeIdIsInvalid()
        {
            throw new InvalidOperationException(
                $"This {nameof(TypeId<T>)} is not valid." +
                $"A valid value must be retrieved from {nameof(TypeId<T>)}.{nameof(TypeId<T>.Get)}."
            );
        }
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
