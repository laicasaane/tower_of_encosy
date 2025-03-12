#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.UnityExtensions
{
    public readonly struct UnityInstanceId<T> : IEquatable<UnityInstanceId<T>>
        where T : UnityEngine.Object
    {
        private readonly int _value;
        private readonly ByteBool _isValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityInstanceId(T obj)
        {
            ThrowIfInvalid(obj);
            _value = obj.GetInstanceID();
            _isValid = ByteBool.True;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isValid;
        }

        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ToObject()
        {
            ThrowIfNotCreated(IsValid);
            return UnityEngine.Resources.InstanceIDToObject(_value) as T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnityInstanceId<T> other)
            => _isValid == other._isValid && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is UnityInstanceId<T> other && _isValid == other._isValid && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_isValid, _value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _isValid ? ToObject().ToString() : _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(UnityInstanceId<T> instanceId)
            => instanceId._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityInstanceId<T>([NotNull] T obj)
            => new(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnityInstanceId<T> left, UnityInstanceId<T> right)
            => left._isValid == right._isValid && left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnityInstanceId<T> left, UnityInstanceId<T> right)
            => left._isValid != right._isValid || left._value != right._value;

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalid(T obj)
        {
            if (obj.IsInvalid())
            {
                throw new ArgumentNullException(nameof(obj));
            }
        }

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfNotCreated([DoesNotReturnIf(false)] bool value)
        {
            if (value == false)
            {
                throw new InvalidOperationException(
                    "UnityInstanceId must be created using the constructor that takes a UnityEngine.Object."
                );
            }
        }
    }
}
