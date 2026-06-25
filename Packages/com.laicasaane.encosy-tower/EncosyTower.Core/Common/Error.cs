using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Common
{
    public readonly struct Error
    {
        public static readonly Error None = default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Error<T> Err<T>(T value)
            => new(value);

        /// <summary>
        /// Returns an <see cref="Error{T}"/> containing <paramref name="trueValue"/>
        /// if <paramref name="condition"/> is true;
        /// otherwise, returns <see cref="Error{T}.None"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Error<T> ErrIf<T>(bool condition, T trueValue)
            => condition ? Err(trueValue) : Error<T>.None;

        /// <summary>
        /// Returns an <see cref="Error{T}"/> containing <paramref name="trueValue"/>
        /// if <paramref name="condition"/> is true;
        /// otherwise, returns an <see cref="Error{T}"/> containing <paramref name="falseValue"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Error<T> ErrIf<T>(bool condition, T trueValue, T falseValue)
            => condition ? Err(trueValue) : Err(falseValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(in Error<T> a, in Error<T> b)
            where T : IEquatable<T>
        {
            return a._hasValue == b._hasValue && a._value.Equals(b._value);
        }
    }

    public readonly struct Error<T> : IHasValue
    {
        public static readonly Error<T> None = default;

        internal readonly T _value;
        internal readonly ByteBool _hasValue;

        internal Error([NotNull] T value)
        {
            _value = value;

            var isValueType = true;
            ValidateValueType(ref isValueType);

            if (isValueType)
            {
                _hasValue = true;
            }
            else
            {
                var result = true;
                ValidateNotNull(value, ref result);

                _hasValue = result;
            }
        }

        public bool HasValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out bool hasValue, out T value)
        {
            hasValue = _hasValue;
            value = _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<T> other)
            => _hasValue == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj switch {
                Error<T> other => DefaultEquals(this, other),
                Bool<T> other => Equals(other),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashValue.Combine(_hasValue, _value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueOrThrow()
        {
            ThrowIfHasNoValue(_hasValue);
            return _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out T value)
        {
            value = _hasValue ? _value : default;
            return _hasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueOrDefault(T defaultValue = default)
            => _hasValue ? _value : defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _hasValue
            ? string.Format("{0}({1})", ErrorExtensions.ERROR_VALUE_STRING, _value)
            : ErrorExtensions.ERROR_NONE_STRING;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Error<T>(T value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Error<T>(Error _)
            => None;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Option<T>(Error<T> error)
            => Option.SomeIf(error.TryGetValue(out var value), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Error<T> left, in Bool<T> right)
            => left._hasValue == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Error<T> left, in Bool<T> right)
            => left._hasValue != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Bool<T> left, in Error<T> right)
            => left == right._hasValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Bool<T> left, in Error<T> right)
            => left != right._hasValue;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool DefaultEquals(in Error<T> a, in Error<T> b)
            => a._hasValue == b._hasValue && EqualityComparer<T>.Default.Equals(a._value, b._value);

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ValidateValueType(ref bool result)
        {
            result = typeof(T).IsValueType;
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ValidateNotNull(T value, ref bool result)
        {
            result = value is UnityEngine.Object unityObject
                ? (ByteBool)(unityObject == true)
                : value is not null;
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfHasNoValue([DoesNotReturnIf(false)] bool hasValue)
        {
            if (hasValue == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"The instance of Error<{typeof(T)}> has no value");
        }
    }

    public static class ErrorExtensions
    {
        internal const string ERROR_VALUE_STRING = "Error+Value";
        internal const string ERROR_NONE_STRING = "Error+None";

        public static bool TryFormat<T>(
              in this Error<T> self
            , Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
            where T : ISpanFormattable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool False(out int value)
            {
                value = 0;
                return false;
            }

            if (self.TryGetValue(out var value) == false)
            {
                var optionNoneSpan = ERROR_NONE_STRING.AsSpan();

                if (destination.Length < optionNoneSpan.Length)
                {
                    return False(out charsWritten);
                }

                optionNoneSpan.CopyTo(destination);
                charsWritten = optionNoneSpan.Length;
                return true;
            }

            var optionValueSpan = ERROR_VALUE_STRING.AsSpan();

            if (destination.Length < optionValueSpan.Length + 1 + 1) // '(' and ')'
            {
                return False(out charsWritten);
            }

            optionValueSpan.CopyTo(destination);
            destination[optionValueSpan.Length] = '(';

            var prefixChars = optionValueSpan.Length + 1;
            destination = destination[prefixChars..];

            var valueDestination = destination[..^1];

            if (value.TryFormat(valueDestination, out var valueCharsWritten, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[valueCharsWritten..];
            destination[0] = ')';
            charsWritten = prefixChars + valueCharsWritten + 1;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? AsNullable<T>(in this Error<T> self)
            where T : struct
        {
            return self._hasValue ? self._value : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueOrNull<T>(in this Error<T> self)
            where T : class
        {
            return self._hasValue ? self._value : null;
        }
    }

    public readonly struct ErrorEqualityComparer<T> : IEqualityComparer<Error<T>>
        where T : IEquatable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Error<T> x, Error<T> y)
            => Error.Equals(in x, in y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Error<T> obj)
            => obj.GetHashCode();
    }
}
