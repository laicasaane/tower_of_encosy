using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Common
{
    public interface IResultSuccess<TValue>
    {
        bool IsValid { get; }

        public bool IsSuccess { get; }

        TValue GetValueOrThrow();

        TValue GetValueOrDefault(TValue defaultOkValue = default);

        bool TryGetValue(out TValue value);
    }

    public interface IResultError<TError>
    {
        bool IsValid { get; }

        public bool IsError { get; }

        TError GetErrorOrThrow();

        TError GetErrorOrDefault(TError defaultErrorValue = default);

        bool TryGetError(out TError value);
    }

    public interface IResult<TValue, TError> : IResultSuccess<TValue>, IResultError<TError>
    {
    }

    public readonly struct Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<TValue>(in Result<TValue> a, in Result<TValue> b)
            where TValue : IEquatable<TValue>
        {
            return Option.Equals(a.Value, b.Value) || a.Error == b.Error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<TValue, TError>(in Result<TValue, TError> a, in Result<TValue, TError> b)
            where TValue : IEquatable<TValue>
            where TError : IEquatable<TError>
        {
            return Option.Equals(a.Value, b.Value) || Option.Equals(a.Error, b.Error);
        }
    }

    public readonly struct Result<TValue> : IResult<TValue, Error>
    {
        public readonly Error Error;

        internal readonly TValue _value;
        internal readonly ByteBool _hasValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TValue value)
        {
            Error = Error.None;
            _value = value;
            _hasValue = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(Error error)
        {
            Error = error;
            _value = default;
            _hasValue = false;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hasValue || Error.IsValid;
        }

        public bool IsSuccess
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsValid && _hasValue;
        }

        public bool IsError
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsValid && Error.IsValid;
        }

        public Option<TValue> Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hasValue ? Option.Some(_value) : Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue> Succeed(TValue value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue> Err(Error error)
            => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Option<TValue> value, out Error error)
        {
            value = Value;
            error = Error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<TValue> other)
            => IsSuccess == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<Error> other)
            => IsError == other;

        public override bool Equals(object obj)
            => obj switch {
                Result<TValue> other => DefaultEquals(this, other),
                Bool<Error> other => IsError == other,
                Bool<TValue> other => IsSuccess == other,
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashValue.Combine(_hasValue, _value, Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValueOrThrow()
            => Value.GetValueOrThrow();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out TValue value)
            => Value.TryGetValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValueOrDefault(TValue defaultValue = default)
            => Value.GetValueOrDefault(defaultValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error GetErrorOrThrow()
            => Error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError(out Error error)
        {
            if (Error.IsValid)
            {
                error = Error;
                return true;
            }
            else
            {
                error = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error GetErrorOrDefault(Error defaultError = default)
            => Error.IsValid ? Error : defaultError;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if (TryGetValue(out var value))
            {
                return string.Format("{0}({1})", ResultExtensions.RESULT_VALUE_STRING, value);
            }
            else if (TryGetError(out var error))
            {
                return string.Format("{0}({1})", ResultExtensions.RESULT_ERROR_STRING, error);
            }
            else
            {
                return ResultExtensions.RESULT_INVALID_STRING;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue>(TValue value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue>(Error error)
            => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue>(Exception error)
            => new(new Error(error));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Result<TValue> left, in Bool<TValue> right)
            => left.IsSuccess == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Result<TValue> left, in Bool<TValue> right)
            => left.IsSuccess != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Bool<TValue> left, in Result<TValue> right)
            => left == right.IsSuccess;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Bool<TValue> left, in Result<TValue> right)
            => left != right.IsSuccess;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Result<TValue> left, in Bool<Error> right)
            => left.IsError == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Result<TValue> left, in Bool<Error> right)
            => left.IsError != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Bool<Error> left, in Result<TValue> right)
            => left == right.IsError;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Bool<Error> left, in Result<TValue> right)
            => left != right.IsError;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool DefaultEquals(in Result<TValue> a, in Result<TValue> b)
            => Option<TValue>.DefaultEquals(a.Value, b.Value) || a.Error.Equals(b.Error);
    }

    public readonly struct Result<TValue, TError> : IResult<TValue, TError>
    {
        internal readonly TValue _value;
        internal readonly TError _error;
        internal readonly ByteBool _hasValue;
        internal readonly ByteBool _hasError;

        static Result()
        {
            ThrowIfSameType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TValue value)
        {
            _value = value;
            _error = default;
            _hasValue = true;
            _hasError = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TError error)
        {
            _value = default;
            _error = error;
            _hasValue = false;
            _hasError = true;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hasValue || _hasError;
        }

        public bool IsSuccess
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsValid && _hasValue;
        }

        public bool IsError
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsValid && _hasError;
        }

        public Option<TValue> Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hasValue ? Option.Some(_value) : Option.None;
        }

        public Option<TError> Error
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hasError ? Option.Some(_error) : Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue, TError> Succeed(TValue value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue, TError> Err(TError error)
            => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Option<TValue> value, out Option<TError> error)
        {
            value = Value;
            error = Error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<TValue> other)
            => IsSuccess == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<TError> other)
            => IsError == other;

        public override bool Equals(object obj)
            => obj switch {
                Result<TValue, TError> other => DefaultEquals(this, other),
                Bool<Error> other => IsError == other,
                Bool<TValue> other => IsSuccess == other,
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashValue.Combine(_hasValue, _value, _hasError, _error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValueOrThrow()
            => Value.GetValueOrThrow();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out TValue value)
            => Value.TryGetValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValueOrDefault(TValue defaultValue = default)
            => Value.GetValueOrDefault(defaultValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TError GetErrorOrThrow()
            => Error.GetValueOrThrow();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError(out TError error)
            => Error.TryGetValue(out error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TError GetErrorOrDefault(TError defaultError = default)
            => Error.GetValueOrDefault(defaultError);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if (TryGetValue(out var value))
            {
                return string.Format("{0}({1})", ResultExtensions.RESULT_VALUE_STRING, value);
            }
            else if (TryGetError(out var error))
            {
                return string.Format("{0}({1})", ResultExtensions.RESULT_ERROR_STRING, error);
            }
            else
            {
                return ResultExtensions.RESULT_INVALID_STRING;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TError>(TValue value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TError>(TError error)
            => new(error);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool DefaultEquals(in Result<TValue, TError> a, in Result<TValue, TError> b)
            => Option<TValue>.DefaultEquals(a.Value, b.Value) || Option<TError>.DefaultEquals(a.Error, b.Error);

        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        private static void ThrowIfSameType()
        {
            if (typeof(TValue) == typeof(TError))
            {
                throw new InvalidOperationException(
                    $"{typeof(Result<TValue, TError>)} is not allowed. Value type and error type must be different."
                );
            }
        }
    }

    public static class ResultExtensions
    {
        internal const string RESULT_VALUE_STRING = "Result+Value";
        internal const string RESULT_ERROR_STRING = "Result+Error";
        internal const string RESULT_INVALID_STRING = "Result+Invalid";

        public static bool TryFormat<T>(
              in this Result<T> self
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

            var resultValue = self.TryGetValue(out var value);
            var resultError = self.TryGetError(out var error);

            if (resultValue == false && resultError == false)
            {
                var resultInvalidSpan = RESULT_INVALID_STRING.AsSpan();

                if (destination.Length < resultInvalidSpan.Length)
                {
                    return False(out charsWritten);
                }

                resultInvalidSpan.CopyTo(destination);
                charsWritten = resultInvalidSpan.Length;
                return true;
            }

            var resultStringSpan = resultValue ? RESULT_VALUE_STRING.AsSpan() : RESULT_ERROR_STRING.AsSpan();

            if (destination.Length < resultStringSpan.Length + 1 + 1) // '(' and ')'
            {
                return False(out charsWritten);
            }

            resultStringSpan.CopyTo(destination);
            destination[resultStringSpan.Length] = '(';

            var prefixChars = resultStringSpan.Length + 1;
            destination = destination[prefixChars..];

            int resultCharsWritten;

            if (resultValue)
            {
                var valueDestination = destination[..^1];

                if (value.TryFormat(valueDestination, out var valueCharsWritten, format, provider) == false)
                {
                    return False(out charsWritten);
                }

                resultCharsWritten = valueCharsWritten;
            }
            else
            {
                var errorDestination = destination[..^1];

                if (error.TryFormat(errorDestination, out var errorCharsWritten) == false)
                {
                    return False(out charsWritten);
                }

                resultCharsWritten = errorCharsWritten;
            }

            destination = destination[resultCharsWritten..];
            destination[0] = ')';
            charsWritten = prefixChars + resultCharsWritten + 1;
            return true;
        }

        public static bool TryFormat<TValue, TError>(
              in this Result<TValue, TError> self
            , Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
            where TValue : ISpanFormattable
            where TError : ISpanFormattable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool False(out int value)
            {
                value = 0;
                return false;
            }

            var resultValue = self.TryGetValue(out var value);
            var resultError = self.TryGetError(out var error);

            if (resultValue == false && resultError == false)
            {
                var resultInvalidSpan = RESULT_INVALID_STRING.AsSpan();

                if (destination.Length < resultInvalidSpan.Length)
                {
                    return False(out charsWritten);
                }

                resultInvalidSpan.CopyTo(destination);
                charsWritten = resultInvalidSpan.Length;
                return true;
            }

            var resultStringSpan = resultValue ? RESULT_VALUE_STRING.AsSpan() : RESULT_ERROR_STRING.AsSpan();

            if (destination.Length < resultStringSpan.Length + 1 + 1) // '(' and ')'
            {
                return False(out charsWritten);
            }

            resultStringSpan.CopyTo(destination);
            destination[resultStringSpan.Length] = '(';

            var prefixChars = resultStringSpan.Length + 1;
            destination = destination[prefixChars..];

            int resultCharsWritten;

            if (resultValue)
            {
                var valueDestination = destination[..^1];

                if (value.TryFormat(valueDestination, out var valueCharsWritten, format, provider) == false)
                {
                    return False(out charsWritten);
                }

                resultCharsWritten = valueCharsWritten;
            }
            else
            {
                var errorDestination = destination[..^1];

                if (error.TryFormat(errorDestination, out var errorCharsWritten, format, provider) == false)
                {
                    return False(out charsWritten);
                }

                resultCharsWritten = errorCharsWritten;
            }

            destination = destination[resultCharsWritten..];
            destination[0] = ')';
            charsWritten = prefixChars + resultCharsWritten + 1;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? AsNullableValue<T>(in this Result<T> self)
            where T : struct
        {
            return self._hasValue ? self._value : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Error? AsNullableError<T>(in this Result<T> self)
        {
            return self.Error.IsValid ? self.Error : default(Error?);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueOrNull<T>(in this Result<T> self)
            where T : class
        {
            return self._hasValue ? self._value : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue? AsNullableValue<TValue, TError>(in this Result<TValue, TError> self)
            where TValue : struct
        {
            return self._hasValue ? self._value : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TError? AsNullableError<TValue, TError>(in this Result<TValue, TError> self)
            where TError : struct
        {
            return self._hasError ? self._error : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GetValueOrNull<TValue, TError>(in this Result<TValue, TError> self)
            where TValue : class
        {
            return self._hasValue ? self._value : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TError GetErrorOrNull<TValue, TError>(in this Result<TValue, TError> self)
            where TError : class
        {
            return self._hasError ? self._error : null;
        }
    }

    public readonly struct ResultEqualityComparer<TValue> : IEqualityComparer<Result<TValue>>
        where TValue : IEquatable<TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Result<TValue> x, Result<TValue> y)
            => Result.Equals(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Result<TValue> obj)
            => obj.GetHashCode();
    }

    public readonly struct ResultEqualityComparer<TValue, TError> : IEqualityComparer<Result<TValue, TError>>
        where TValue : IEquatable<TValue>
        where TError : IEquatable<TError>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Result<TValue, TError> x, Result<TValue, TError> y)
            => Result.Equals(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Result<TValue, TError> obj)
            => obj.GetHashCode();
    }
}
