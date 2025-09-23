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
    [Serializable]
    public struct UnityEntityId<T> : IEquatable<UnityEntityId<T>>, ISpanFormattable
        where T : UnityEngine.Object
    {
        [SerializeField]
        private EntityId _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityEntityId(T obj)
        {
            ThrowIfInvalid(obj);
            _value = obj.GetEntityId();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityEntityId(EntityId value)
        {
            _value = value;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityEntityId(InstanceID value)
        {
            _value = (int)value;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Resources.EntityIdIsValid(_value);
        }

        public readonly EntityId Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<T> ToObject()
        {
            ThrowIfNotCreated(IsValid);
            return Resources.EntityIdToObject(_value) as T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UnityEntityId<T> other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj)
            => obj is UnityEntityId<T> other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => _value.ToString();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(UnityEntityId<T> instanceId)
            => instanceId._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityEntityId<T>(int instanceId)
            =>  new((EntityId)instanceId);

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
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnityEntityId<T> left, UnityEntityId<T> right)
            => left._value != right._value;

#pragma warning disable CS0618 // Type or member is obsolete
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UnityEntityId<T>(InstanceID instanceId)
            => new(instanceId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityEntityId<T>(UnityInstanceId<T> instanceId)
            => new(instanceId.Value);
#pragma warning restore CS0618 // Type or member is obsolete

        [HideInCallstack, StackTraceHidden, DoesNotReturn, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalid(T obj)
        {
            if (obj.IsInvalid())
            {
                throw new ArgumentNullException(nameof(obj));
            }
        }

        [HideInCallstack, StackTraceHidden, DoesNotReturn, Conditional("__ENCOSY_VALIDATION__")]
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
