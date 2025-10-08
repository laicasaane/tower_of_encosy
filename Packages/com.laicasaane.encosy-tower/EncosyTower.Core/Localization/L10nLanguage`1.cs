#if UNITY_LOCALIZATION

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.Localization
{
    [Serializable]
    public struct L10nLanguage<TEnum> : IEquatable<L10nLanguage<TEnum>>, IEquatable<L10nLanguage>
        where TEnum : unmanaged, Enum
    {
        public readonly static L10nLanguage<TEnum> Default = new(0, L10nLanguage.DEFAULT_LOCALE_CODE);

        [SerializeField] internal ushort _value;
        [SerializeField] internal string _code;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nLanguage(ushort value, [NotNull] string code)
        {
            _value = value;
            _code = code;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _code.IsNotEmpty();
        }

        public readonly ushort Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        public readonly string Code
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _code;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToLocaleCode()
            => IsValid ? _code : L10nLanguage.DEFAULT_LOCALE_CODE;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(L10nLanguage<TEnum> other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(L10nLanguage other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is L10nLanguage<TEnum> otherT
            ? _value.Equals(otherT._value)
            : obj is L10nLanguage other && _value.Equals(other._value)
            ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator L10nLanguage(L10nLanguage<TEnum> value)
            => new(value._value, value._code);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator L10nLanguage<TEnum>(L10nLanguage value)
            => new(value._value, value._code);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L10nLanguage<TEnum> left, L10nLanguage<TEnum> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L10nLanguage<TEnum> left, L10nLanguage<TEnum> right)
            => !left.Equals(right);
    }
}

#endif
