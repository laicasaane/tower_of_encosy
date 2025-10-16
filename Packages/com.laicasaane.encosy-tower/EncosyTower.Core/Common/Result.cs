using System;
using System.Runtime.CompilerServices;

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

    public readonly struct Result<TValue> : IEquatable<Result<TValue>>, IResult<TValue, Error>
    {
        public readonly Option<TValue> Value;
        public readonly Error Error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TValue value)
        {
            Value = value;
            Error = Error.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(Error error)
        {
            Value = Option.None;
            Error = error;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.HasValue || Error.IsValid;
        }

        public bool IsSuccess
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.HasValue;
        }

        public bool IsError
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsSuccess;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Option<TValue> value, out Error error)
        {
            value = Value;
            error = Error;
        }

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
        public bool Equals(Result<TValue> other)
            => Value.Equals(other.Value) && Error.Equals(other.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Result<TValue, Error> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Value, Error);

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
        public static bool operator ==(Result<TValue> left, Result<TValue> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Result<TValue> left, Result<TValue> right)
            => !left.Equals(right);
    }

    public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>, IResult<TValue, TError>
    {
        public readonly Option<TValue> Value;
        public readonly Option<TError> Error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TValue value)
        {
            Value = Option.Some(value);
            Error = Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TError error)
        {
            Value = Option.None;
            Error = Option.Some(error);
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.HasValue || Error.HasValue;
        }

        public bool IsSuccess
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.HasValue;
        }

        public bool IsError
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsSuccess;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Option<TValue> value, out Option<TError> error)
        {
            value = Value;
            error = Error;
        }

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
        public bool Equals(Result<TValue, TError> other)
            => Value.Equals(other.Value) && Error.Equals(other.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Result<TValue, TError> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashValue.Combine(Value, Error);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right)
            => !left.Equals(right);
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

            if (destination.Length < resultStringSpan.Length + 1)
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
                if (value.TryFormat(destination, out var valueCharsWritten, format, provider) == false)
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

            if (destination.Length < 1)
            {
                return False(out charsWritten);
            }

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
    }
}
