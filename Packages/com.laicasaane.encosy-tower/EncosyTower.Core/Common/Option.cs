using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Common
{
    public readonly struct Option
    {
        public readonly static Option None = default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> Some<T>(T value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(in Option<T> a, in Option<T> b)
            where T : IEquatable<T>
        {
            return a.HasValue == b.HasValue && a._value.Equals(b._value);
        }
    }

    public readonly struct Option<T>
    {
        public readonly static Option<T> None = default;

        internal readonly T _value;

        public readonly ByteBool HasValue;

        internal Option(T value)
        {
            _value = value;

            var isValueType = true;
            ValidateValueType(ref isValueType);

            if (isValueType)
            {
                HasValue = true;
            }
            else
            {
                var result = true;
                ValidateNotNull(value, ref result);

                HasValue = result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out bool hasValue, out T value)
        {
            hasValue = HasValue;
            value = _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool<T> other)
            => HasValue == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj switch {
                Option<T> other => Equals(other),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(HasValue, _value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueOrThrow()
        {
            ThrowIfHasNoValue(HasValue);
            return _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out T value)
        {
            value = HasValue ? _value : default;
            return HasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueOrDefault(T defaultValue = default)
            => HasValue ? _value : defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => HasValue
            ? string.Format("{0}({1})", OptionExtensions.OPTION_VALUE_STRING, _value)
            : OptionExtensions.OPTION_NONE_STRING;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Option<T>(T value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Option<T>(Option _)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Option<T> left, in Bool<T> right)
            => left.HasValue == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Option<T> left, in Bool<T> right)
            => left.HasValue != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Bool<T> left, in Option<T> right)
            => left == right.HasValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Bool<T> left, in Option<T> right)
            => left != right.HasValue;

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
        private static void ThrowIfHasNoValue([DoesNotReturnIf(false)] bool check)
        {
            if (check == false)
            {
                throw new InvalidOperationException($"The instance of Option<{typeof(T)}> has no value");
            }
        }
    }

    public static class OptionExtensions
    {
        internal const string OPTION_VALUE_STRING = "Option+Value";
        internal const string OPTION_NONE_STRING = "Option+None";

        public static bool TryFormat<T>(
              in this Option<T> self
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
                var optionNoneSpan = OPTION_NONE_STRING.AsSpan();

                if (destination.Length < optionNoneSpan.Length)
                {
                    return False(out charsWritten);
                }

                optionNoneSpan.CopyTo(destination);
                charsWritten = optionNoneSpan.Length;
                return true;
            }

            var optionValueSpan = OPTION_VALUE_STRING.AsSpan();

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
    }

    public readonly struct OptionEqualityComparer<T> : IEqualityComparer<Option<T>>
        where T : IEquatable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Option<T> x, Option<T> y)
            => Option.Equals(in x, in y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Option<T> obj)
            => obj.GetHashCode();
    }
}
