using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Common
{
    public readonly struct Success
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Success Succeed()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Success<TFailure> Fail<TFailure>(TFailure value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(in Success<T> a, in Success<T> b)
            where T : IEquatable<T>
        {
            return a.IsFailure == b.IsFailure && a._failure.Equals(b._failure);
        }
    }

    /// <summary>
    /// Represents a success result that may contain a failure value of type <typeparamref name="TFailure"/>.
    /// </summary>
    /// <typeparam name="TFailure">The type of the failure value.</typeparam>
    public readonly struct Success<TFailure>
    {
        internal readonly TFailure _failure;

        public readonly ByteBool IsFailure;

        internal Success(TFailure failure)
        {
            _failure = failure;
            IsFailure = true;
        }

        public bool IsSuccess
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsFailure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out bool isFailure, out TFailure failure)
        {
            isFailure = IsFailure;
            failure = _failure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<TFailure> other)
            => IsFailure == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj switch {
                Success<TFailure> other => DefaultEquals(this, other),
                Bool<TFailure> other => Equals(other),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(IsFailure, _failure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TFailure GetValueOrThrow()
        {
            ThrowIfHasNoValue(IsFailure);
            return _failure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out TFailure value)
        {
            value = IsFailure ? _failure : default;
            return IsFailure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TFailure GetValueOrDefault(TFailure defaultValue = default)
            => IsFailure ? _failure : defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => IsFailure
            ? string.Format("{0}({1})", SuccessExtensions.SUCCESS_FAILURE_STRING, _failure)
            : SuccessExtensions.SUCCESS_SUCCESS_STRING;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Success<TFailure>(TFailure value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Success<TFailure>(Success _)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Success<TFailure> left, in Bool<TFailure> right)
            => left.IsFailure == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Success<TFailure> left, in Bool<TFailure> right)
            => left.IsFailure != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Bool<TFailure> left, in Success<TFailure> right)
            => left == right.IsFailure;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Bool<TFailure> left, in Success<TFailure> right)
            => left != right.IsFailure;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool DefaultEquals(in Success<TFailure> a, in Success<TFailure> b)
            => a.IsFailure == b.IsFailure && EqualityComparer<TFailure>.Default.Equals(a._failure, b._failure);

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfHasNoValue([DoesNotReturnIf(false)] bool check)
        {
            if (check == false)
            {
                throw new InvalidOperationException($"The instance of Option<{typeof(TFailure)}> has no value");
            }
        }
    }

    public static class SuccessExtensions
    {
        internal const string SUCCESS_FAILURE_STRING = "Success+Failure";
        internal const string SUCCESS_SUCCESS_STRING = "Success+Success";

        public static bool TryFormat<T>(
              in this Success<T> self
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
                var successSpan = SUCCESS_SUCCESS_STRING.AsSpan();

                if (destination.Length < successSpan.Length)
                {
                    return False(out charsWritten);
                }

                successSpan.CopyTo(destination);
                charsWritten = successSpan.Length;
                return true;
            }

            var failureSpan = SUCCESS_FAILURE_STRING.AsSpan();

            if (destination.Length < failureSpan.Length + 1 + 1) // '(' and ')'
            {
                return False(out charsWritten);
            }

            failureSpan.CopyTo(destination);
            destination[failureSpan.Length] = '(';

            var prefixChars = failureSpan.Length + 1;
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
        public static TFailure? AsNullableFailure<TFailure>(in this Success<TFailure> self)
            where TFailure : struct
        {
            return self.IsFailure ? self._failure : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TFailure GetFailureOrNull<TFailure>(in this Success<TFailure> self)
            where TFailure : class
        {
            return self.IsFailure ? self._failure : null;
        }
    }

    public readonly struct SuccessEqualityComparer<TFailure> : IEqualityComparer<Success<TFailure>>
        where TFailure : IEquatable<TFailure>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Success<TFailure> x, Success<TFailure> y)
            => Success.Equals(in x, in y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Success<TFailure> obj)
            => obj.GetHashCode();
    }
}
