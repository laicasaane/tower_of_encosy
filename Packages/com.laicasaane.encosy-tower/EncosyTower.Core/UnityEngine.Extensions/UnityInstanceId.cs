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
#if UNITY_6000_2_OR_NEWER
    [Obsolete(
        "UnityInstanceId<T> is deprecated. Use UnityEntityId<T> instead."
    )]
#endif
    public readonly struct UnityInstanceId<T> : IEquatable<UnityInstanceId<T>>
        where T : UnityEngine.Object
    {
#if UNITY_6000_2_OR_NEWER
        private readonly UnityEngine.EntityId _value;
#else
        private readonly int _value;
#endif

        private readonly ByteBool _isValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityInstanceId(T obj)
        {
            ThrowIfInvalid(obj);

#if UNITY_6000_2_OR_NEWER
            _value = obj.GetEntityId();
#else
            _value = obj.GetInstanceID();
#endif

            _isValid = ByteBool.True;
        }

#if UNITY_6000_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityInstanceId(UnityEngine.EntityId entityId)
        {
            _value = entityId;
            _isValid = ByteBool.True;
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityInstanceId(int instanceId)
        {
            _value = instanceId;
            _isValid = ByteBool.True;
        }
#endif

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

#if UNITY_6000_2_OR_NEWER
            return UnityEngine.Resources.EntityIdToObject(_value) as T;
#else
            return UnityEngine.Resources.InstanceIDToObject(_value) as T;
#endif
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
        public static explicit operator UnityInstanceId<T>(int instanceId)
            => new(instanceId);

#if UNITY_6000_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityInstanceId<T>(UnityEngine.EntityId entityId)
            => new(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityEngine.EntityId(UnityInstanceId<T> instanceId)
            => instanceId._value;
#endif

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
