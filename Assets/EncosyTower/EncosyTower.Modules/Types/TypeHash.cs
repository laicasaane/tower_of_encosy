#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

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
        public static readonly TypeHash Null = new();

        private readonly int _value;

#if __ENCOSY_VALIDATION__
        private readonly ByteBool _isValid;
#endif

        private TypeHash(Type type)
        {
            _value = type.GetHashCode();

#if __ENCOSY_VALIDATION__
            _isValid = true;
#endif
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeHash typeHash)
            {
                return
#if __ENCOSY_VALIDATION__
                    _isValid == typeHash._isValid &&
#endif
                    _value == typeHash._value;
            }

#if __ENCOSY_VALIDATION__
            if (_isValid == false)
            {
                return false;
            }
#endif

            return obj switch {
                int hashCode => _value == hashCode,
                Type type => _value == type.GetHashCode(),
                _ => false,
            };
        }

        public bool Equals(TypeHash other)
        {
            return
#if __ENCOSY_VALIDATION__
                _isValid == other._isValid &&
#endif
                _value == other._value;
        }

        public override int GetHashCode()
        {
#if __ENCOSY_VALIDATION__
            if (_isValid == false)
            {
                throw GetNullReferenceException();
            }
#endif

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
        {
            return
#if __ENCOSY_VALIDATION__
                lhs._isValid == rhs._isValid &&
#endif
                lhs._value == rhs._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TypeHash lhs, TypeHash rhs)
        {
            return
#if __ENCOSY_VALIDATION__
                lhs._isValid != rhs._isValid ||
#endif
                lhs._value != rhs._value;
        }

#if __ENCOSY_VALIDATION__
        private static NullReferenceException GetNullReferenceException()
        {
            return new NullReferenceException(
                $"Cannot use an invalid {nameof(TypeHash)} value. " +
                $"{nameof(TypeHash)} value must be retrieved from System.Type " +
                $"by the implicit operator."
            );
        }
#endif
    }
}
