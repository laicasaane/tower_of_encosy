#if UNITY_6000_2_OR_NEWER

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
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    public readonly struct UnityEntityId<T> : IEquatable<UnityEntityId<T>>
        where T : UnityEngine.Object
    {
        private readonly EntityId _value;
        private readonly ByteBool _isValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityEntityId(T obj)
        {
            ThrowIfInvalid(obj);
            _value = obj.GetEntityId();
            _isValid = ByteBool.True;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityEntityId(EntityId entityId)
        {
            _value = entityId;
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
            return Resources.EntityIdToObject(_value) as T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnityEntityId<T> other)
            => _isValid == other._isValid && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is UnityEntityId<T> other && _isValid == other._isValid && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_isValid, _value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _isValid ? ToObject().ToString() : _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(UnityEntityId<T> instanceId)
            => instanceId._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityEntityId<T>(int instanceId)
            =>  new(instanceId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityEntityId<T>(EntityId entityId)
            => new(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EntityId(UnityEntityId<T> instanceId)
            => instanceId._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityEntityId<T>([NotNull] T obj)
            => new(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnityEntityId<T> left, UnityEntityId<T> right)
            => left._isValid == right._isValid && left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnityEntityId<T> left, UnityEntityId<T> right)
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
                    "UnityEntityId must be created using the constructor that takes a UnityEngine.Object."
                );
            }
        }
    }
}

#endif
