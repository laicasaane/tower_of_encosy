#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public readonly struct UnityInstanceId<T> : IEquatable<UnityInstanceId<T>>
        where T : UnityEngine.Object
    {
        private readonly int _instanceId;
        private readonly ByteBool _isValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityInstanceId(UnityEngine.Object obj)
        {
            ThrowIfInvalid(obj);
            _instanceId = obj.GetInstanceID();
            _isValid = ByteBool.True;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isValid;
        }

        public int InstanceId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _instanceId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ToObject()
        {
            ThrowIfNotCreated(IsValid);
            return UnityEngine.Resources.InstanceIDToObject(_instanceId) as T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnityInstanceId<T> other)
            => _isValid == other._isValid && _instanceId == other._instanceId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is UnityInstanceId<T> other && _isValid == other._isValid && _instanceId == other._instanceId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_isValid, _instanceId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _isValid ? ToObject().ToString() : _instanceId.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityInstanceId<T>([NotNull] UnityEngine.Object obj)
            => new(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnityInstanceId<T> left, UnityInstanceId<T> right)
            => left._isValid == right._isValid && left._instanceId == right._instanceId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnityInstanceId<T> left, UnityInstanceId<T> right)
            => left._isValid != right._isValid || left._instanceId != right._instanceId;

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalid(UnityEngine.Object obj)
        {
            if (obj == false || obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
        }

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfNotCreated([DoesNotReturnIf(false)] bool value)
        {
            if (value == false)
            {
                throw new InvalidOperationException("UnityInstanceId must be created properly");
            }
        }
    }
}
