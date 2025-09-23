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
#if UNITY_6000_2_OR_NEWER
    [Obsolete(
        "UnityInstanceId<T> is deprecated. Use UnityEntityId<T> instead."
    )]
#endif
    [Serializable]
    public struct UnityInstanceId<T> : IEquatable<UnityInstanceId<T>>, ISpanFormattable
        where T : UnityEngine.Object
    {
        [SerializeField]
#if UNITY_6000_2_OR_NEWER
        private EntityId _value;
#else
        private int _value;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityInstanceId(T obj)
        {
            ThrowIfInvalid(obj);

#if UNITY_6000_2_OR_NEWER
            _value = obj.GetEntityId();
#else
            _value = obj.GetInstanceID();
#endif
        }

#if UNITY_6000_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityInstanceId(EntityId value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityInstanceId(InstanceID value)
        {
            _value = (int)value;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityInstanceId(int value)
        {
            _value = value;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if UNITY_6000_2_OR_NEWER
            get => Resources.EntityIdIsValid(_value);
#else
            get => Resources.InstanceIDIsValid(_value);
#endif
        }

#if UNITY_6000_2_OR_NEWER
        public readonly EntityId Value
#else
        public readonly int Value
#endif
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<T> ToObject()
        {
            ThrowIfNotCreated(IsValid);

#if UNITY_6000_2_OR_NEWER
            return Resources.EntityIdToObject(_value) as T;
#else
            return Resources.InstanceIDToObject(_value) as T;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UnityInstanceId<T> other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is UnityInstanceId<T> other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => _value.ToString();

#if UNITY_6000_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
            => ((int)_value).ToString(format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            return ((int)_value).TryFormat(destination, out charsWritten, format, provider);
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
            => _value.ToString(format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            return _value.TryFormat(destination, out charsWritten, format, provider);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(UnityInstanceId<T> instanceId)
            => instanceId._value;

#if UNITY_6000_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityInstanceId<T>(int instanceId)
            => new((EntityId)instanceId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityInstanceId<T>(EntityId entityId)
            => new(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EntityId(UnityInstanceId<T> instanceId)
            => instanceId._value;
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityInstanceId<T>(int instanceId)
            => new(instanceId);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityInstanceId<T>([NotNull] T obj)
            => new(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnityInstanceId<T> left, UnityInstanceId<T> right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnityInstanceId<T> left, UnityInstanceId<T> right)
            => left._value != right._value;

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
