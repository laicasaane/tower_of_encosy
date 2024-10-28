using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    /// <summary>
    /// Represents the hash code of <see cref="System"/>.<see cref="Type"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="TypeHash"/> should not be manually created but must be
    /// retrieved retrieved from <see cref="System"/>.<see cref="Type"/>
    /// by the implicit operator.
    /// </remarks>
    public readonly struct TypeHash : IEquatable<TypeHash>
    {
        private const byte VALID = 1;

        public static readonly TypeHash Null = new();

        private readonly int _value;
        private readonly byte _isValid;

        private TypeHash(Type type)
        {
            _value = type.GetHashCode();
            _isValid = VALID;
        }

        public override bool Equals(object obj)
            => obj switch {
                TypeHash typeHash => _isValid == typeHash._isValid && _value == typeHash._value,
                int hashCode => _isValid == VALID && _value == hashCode,
                Type type => _isValid == VALID && _value == type.GetHashCode(),
                _ => false
            };

        public bool Equals(TypeHash other)
            => _isValid == other._isValid && _value == other._value;

        public override int GetHashCode()
        {
            if (_isValid != VALID)
            {
                throw new NullReferenceException(
                    $"Cannot use an invalid {nameof(TypeHash)} value. " +
                    $"{nameof(TypeHash)} value must be retrieved from System.Type " +
                    $"by the implicit operator."
                );
            }

            return _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(TypeHash value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TypeHash(Type type)
            => new(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TypeHash lhs, TypeHash rhs)
            => lhs._isValid == rhs._isValid && lhs._value == rhs._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TypeHash lhs, TypeHash rhs)
            => lhs._isValid != rhs._isValid || lhs._value != rhs._value;
    }
}