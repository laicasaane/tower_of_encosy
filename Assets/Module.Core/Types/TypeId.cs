namespace Module.Core
{
    using System;
    using System.Runtime.CompilerServices;

    public readonly partial struct TypeId : IEquatable<TypeId>
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
        {
            return TypeIdVault.TryGetType(_value, out var type)
                ? type
                : TypeIdVault.UndefinedType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TypeId other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is TypeId other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Id(in TypeId id)
            => id._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in TypeId lhs, in TypeId rhs)
            => lhs._value == rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in TypeId lhs, in TypeId rhs)
            => lhs._value != rhs._value;

        public static TypeId Get<T>()
        {
            var id = new TypeId(TypeIdVault.Cache<T>.Id);
            TypeIdVault.Register(id._value, TypeCache<T>.Type);
            return id;
        }
    }
}

#if UNITY_COLLECTIONS

namespace Module.Core
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct TypeId
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
