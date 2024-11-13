#if UNITY_LOCALIZATION

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Localization
{
    // ReSharper disable once InconsistentNaming
    public readonly struct L10nLanguage<TEnum> : IEquatable<L10nLanguage<TEnum>>, IEquatable<L10nLanguage>
        where TEnum : unmanaged, Enum
    {
        public readonly static L10nLanguage<TEnum> Default = new(0, L10nLanguage.DEFAULT_LOCALE_CODE);

        public readonly L10nLanguage Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nLanguage(ushort value, [NotNull] string code)
        {
            Value = new(value, code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nLanguage(L10nLanguage value)
        {
            Value = value;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToLocaleCode()
            => Value.ToLocaleCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(L10nLanguage<TEnum> other)
            => Value.Equals(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(L10nLanguage other)
            => Value.Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is L10nLanguage<TEnum> otherT
            ? Value.Equals(otherT.Value)
            : obj is L10nLanguage other && Value.Equals(other)
            ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator L10nLanguage(L10nLanguage<TEnum> value)
            => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator L10nLanguage<TEnum>(L10nLanguage value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L10nLanguage<TEnum> left, L10nLanguage<TEnum> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L10nLanguage<TEnum> left, L10nLanguage<TEnum> right)
            => !left.Equals(right);
    }

    // ReSharper disable once InconsistentNaming
    partial struct L10nLanguage
    {
        [Serializable]
        public struct Serializable<TEnum> : ITryConvert<L10nLanguage<TEnum>>
            , IEquatable<Serializable<TEnum>>
            where TEnum : unmanaged, Enum
        {
            [SerializeField]
            private ushort _value;

            [SerializeField]
            private string _code;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(ushort value, string code)
            {
                _value = value;
                _code = code;
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => string.IsNullOrEmpty(_code) == false;
            }

            public ushort Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _value;
            }

            public string Code
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _code;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryConvert(out L10nLanguage<TEnum> result)
            {
                result = new(_value, _code);
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable<TEnum> other)
                => _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable<TEnum> other && _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _value.GetHashCode();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<TEnum>(Serializable value)
                => new(value.Value, value.Code);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(Serializable<TEnum> value)
                => new(value.Value, value.Code);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator L10nLanguage<TEnum>(Serializable<TEnum> value)
                => new(value._value, value._code);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<TEnum>(L10nLanguage<TEnum> value)
                => new(value.Value.Value, value.Value.Code);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable<TEnum> left, Serializable<TEnum> right)
                => left._value == right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable<TEnum> left, Serializable<TEnum> right)
                => left._value != right._value;
        }
    }
}

#endif
