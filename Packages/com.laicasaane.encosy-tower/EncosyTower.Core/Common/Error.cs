using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

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
                || Exception?.Equals(other.Exception) == true;

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
                return Exception.ToString();
            }

            return "Unknown error";
        }

        public bool TryFormat(Span<char> destination, out int charsWritten)
        {
            ReadOnlySpan<char> source;

            if (Message.IsNotEmpty())
            {
                source = Message.AsSpan();
            }
            else if (Exception is not null && Exception != UndefinedException.Default)
            {
                source = Exception.ToString().AsSpan();
            }
            else
            {
                source = Message.AsSpan();
            }

            var minLength = Mathf.Min(destination.Length, source.Length);
            source[..minLength].CopyTo(destination);
            charsWritten = minLength;

            return true;
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
}
