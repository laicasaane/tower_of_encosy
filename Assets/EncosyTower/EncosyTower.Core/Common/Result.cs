using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public interface IResultOk<TOk>
    {
        bool IsValid { get; }

        TOk Ok();

        TOk OkOrDefault(TOk defaultOkValue = default);

        bool TryOk(out TOk value);
    }

    public interface IResultError<TError>
    {
        bool IsValid { get; }

        TError Error();

        TError ErrorOrDefault(TError defaultErrorValue = default);

        bool TryError(out TError value);
    }

    public interface IResult<TOk, TError> : IResultOk<TOk>, IResultError<TError>
    {
    }

    public readonly struct Result<TOk> : IEquatable<Result<TOk>>, IResult<TOk, Error>
    {
        private readonly Option<TOk> _ok;
        private readonly Error _error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TOk ok)
        {
            _ok = new Option<TOk>(ok);
            _error = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(Error error)
        {
            _ok = default;
            _error = error;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ok.HasValue || _error.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Option<TOk> ok, out Error error)
        {
            ok = _ok;
            error = _error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOk Ok()
            => _ok.Value();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryOk(out TOk value)
            => _ok.TryValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOk OkOrDefault(TOk defaultOkValue = default)
            => _ok.ValueOrDefault(defaultOkValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error Error()
            => _error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryError(out Error value)
        {
            if (_error.IsValid)
            {
                value = _error;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Error ErrorOrDefault(Error defaultErrorValue = default)
            => _error.IsValid ? _error : defaultErrorValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Result<TOk> other)
            => _ok.Equals(other._ok) && _error.Equals(other._error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Result<TOk, Error> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_ok, _error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if (TryOk(out var okValue))
            {
                return $"Result+Ok({okValue})";
            }
            else if (TryError(out var errorValue))
            {
                return $"Result+Error({errorValue})";
            }
            else
            {
                return "Result+Invalid";
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TOk>(TOk ok)
            => new(ok);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TOk>(Error error)
            => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TOk>(Exception exception)
            => new(new Error(exception));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Result<TOk> left, Result<TOk> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Result<TOk> left, Result<TOk> right)
            => !left.Equals(right);
    }

    public readonly struct Result<TOk, TError> : IEquatable<Result<TOk, TError>>, IResult<TOk, TError>
    {
        private readonly Option<TOk> _ok;
        private readonly Option<TError> _error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TOk ok)
        {
            _ok = new Option<TOk>(ok);
            _error = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result(TError error)
        {
            _ok = default;
            _error = new Option<TError>(error);
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ok.HasValue || _error.HasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Option<TOk> ok, out Option<TError> error)
        {
            ok = _ok;
            error = _error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOk Ok()
            => _ok.Value();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryOk(out TOk value)
            => _ok.TryValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOk OkOrDefault(TOk defaultOkValue = default)
            => _ok.ValueOrDefault(defaultOkValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TError Error()
            => _error.Value();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryError(out TError value)
            => _error.TryValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TError ErrorOrDefault(TError defaultErrorValue = default)
            => _error.ValueOrDefault(defaultErrorValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Result<TOk, TError> other)
            => _ok.Equals(other._ok) && _error.Equals(other._error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Result<TOk, TError> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_ok, _error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if (TryOk(out var okValue))
            {
                return $"Result+Ok({okValue})";
            }
            else if (TryError(out var errorValue))
            {
                return $"Result+Error({errorValue})";
            }
            else
            {
                return "Result+Invalid";
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TOk, TError>(TOk ok)
            => new(ok);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TOk, TError>(TError error)
            => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Result<TOk, TError> left, Result<TOk, TError> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Result<TOk, TError> left, Result<TOk, TError> right)
            => !left.Equals(right);
    }
}
