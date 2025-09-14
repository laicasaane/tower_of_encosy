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
                return $"Result+Value({value})";
            }
            else if (TryGetError(out var error))
            {
                return $"Result+Error({error})";
            }
            else
            {
                return "Result+Invalid";
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
            Value = new Option<TValue>(value);
            Error = Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TError error)
        {
            Value = Option.None;
            Error = new Option<TError>(error);
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
                return $"Result+Value({value})";
            }
            else if (TryGetError(out var error))
            {
                return $"Result+Error({error})";
            }
            else
            {
                return "Result+Invalid";
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
}
