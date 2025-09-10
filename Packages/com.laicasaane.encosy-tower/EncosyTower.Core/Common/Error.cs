using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public interface IError
    {
        bool IsValid { get; }
    }

    public readonly struct Error : IEquatable<Error>, IError
    {
        public static readonly Error None = default;

        public static readonly Error Default = new(string.Empty);

        public readonly string Message;
        public readonly Exception Exception;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error([NotNull] string message)
        {
            Message = message ?? string.Empty;
            Exception = UndefinedException.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error([NotNull] Exception exception)
        {
            Exception = exception ?? UndefinedException.Default;
            Message = exception?.Message ?? string.Empty;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Message.IsNotEmpty() || Exception is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Error other)
            => string.Equals(Message, other.Message, StringComparison.Ordinal)
                && Exception?.Equals(other.Exception) == true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Error other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Message, Exception);

        public override string ToString()
        {
            if (Message.IsNotEmpty())
            {
                return Message;
            }

            if (Exception is not null && Exception != UndefinedException.Default)
            {
                return Exception.Message;
            }

            return "Unknown error.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Error(string message)
            => new(message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Error(Exception exception)
            => new(exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Error left, Error right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Error left, Error right)
            => !left.Equals(right);
    }

    public readonly struct Error<TError> : IEquatable<Error<TError>>, IError
    {
        public static readonly Error<TError> Default = new(Error.Default);

        public readonly Option<TError> Value;
        public readonly Error InnerError;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error([NotNull] TError value)
        {
            Value = value;
            InnerError = UndefinedException.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error([NotNull] Error innerError)
        {
            Value = Option.None;
            InnerError = innerError;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.HasValue || InnerError.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Error<TError> other)
            => Value.Equals(other.Value) && InnerError.Equals(other.InnerError);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Error<TError> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Value, InnerError);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => Value.TryGetValue(out var value)
                ? value.ToString()
                : InnerError.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Error<TError>(TError value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Error<TError>(Error error)
            => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Error<TError> left, Error<TError> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Error<TError> left, Error<TError> right)
            => !left.Equals(right);
    }
}
