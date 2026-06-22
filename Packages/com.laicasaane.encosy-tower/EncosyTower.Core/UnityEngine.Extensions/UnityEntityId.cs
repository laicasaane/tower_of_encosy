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
using System.Runtime.InteropServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    [Serializable, StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct UnityEntityId<T> : IEquatable<UnityEntityId<T>>, ISpanFormattable
        where T : UnityEngine.Object
    {
        [SerializeField]
        private EntityId _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityEntityId(T obj)
        {
            ThrowIfInvalid(obj.IsValid());
            _value = obj.GetEntityId();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityEntityId(EntityId value)
        {
            _value = value;
        }

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
#if UNITY_6000_5_OR_NEWER
            => _value.ToString(format, formatProvider);
#else
            => ((int)_value).ToString(format, formatProvider);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
#if UNITY_6000_5_OR_NEWER
            return EntityId.ToULong(_value).TryFormat(destination, out charsWritten, format, provider);
#else
            return ((int)_value).TryFormat(destination, out charsWritten, format, provider);
#endif
        }

#if UNITY_6000_5_OR_NEWER
        /// <inheritdoc cref="EntityId.ToULong(EntityId)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ToULong(UnityEntityId<T> entityId)
            => EntityId.ToULong(entityId._value);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(UnityEntityId<T> entityId)
            => entityId._value;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityEntityId<T>(EntityId entityId)
            => new(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EntityId(UnityEntityId<T> entityId)
            => entityId._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityEntityId<T>([NotNull] T obj)
            => new(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnityEntityId<T> left, UnityEntityId<T> right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnityEntityId<T> left, UnityEntityId<T> right)
            => left._value != right._value;

#if !UNITY_6000_5_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("InstanceID is deprecated. Use EntityId instead.")]
        public UnityEntityId(InstanceID value)
        {
            _value = (int)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("32-bit integer Instance ID is deprecated. Use EntityId instead.")]
        public static implicit operator UnityEntityId<T>(int instanceId)
            => new((EntityId)instanceId);

#if !UNITY_6000_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("InstanceID is deprecated. Use EntityId instead.")]
        public static implicit operator UnityEntityId<T>(InstanceID instanceId)
            => new(instanceId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("UnityInstanceId<T> is deprecated. Use UnityEntityId<T> instead.")]
        public static implicit operator UnityEntityId<T>(UnityInstanceId<T> instanceId)
            => new(instanceId.Value);
#endif
#endif

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("UnityEngine.Object is null or invalid.", "obj");
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfNotCreated([DoesNotReturnIf(false)] bool value)
        {
            if (value == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("UnityEntityId must be created using the constructor that takes a UnityEngine.Object.");
        }
    }
}

#endif
