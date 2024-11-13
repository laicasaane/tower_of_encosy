#if UNITY_LOCALIZATION

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Localization
{
    // ReSharper disable once InconsistentNaming
    public readonly partial struct L10nLanguage : IEquatable<L10nLanguage>
    {
        public const string DEFAULT_LOCALE_CODE = "en";

        public readonly static L10nLanguage Default = new(0, DEFAULT_LOCALE_CODE);

        public readonly ushort Value;
        public readonly string Code;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nLanguage(ushort value, [NotNull] string code)
        {
            Value = value;
            Code = code;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.IsNullOrEmpty(Code) == false;
        }

        public string ToLocaleCode()
            => IsValid ? Code : DEFAULT_LOCALE_CODE;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(L10nLanguage other)
            => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is L10nLanguage other && Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L10nLanguage left, L10nLanguage right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L10nLanguage left, L10nLanguage right)
            => !left.Equals(right);

        [Serializable]
        public struct Serializable : ITryConvert<L10nLanguage>
            , IEquatable<Serializable>
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
            public bool TryConvert(out L10nLanguage result)
            {
                result = new(_value, _code);
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable other && _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _value.GetHashCode();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator L10nLanguage(Serializable value)
                => new(value._value, value._code);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(L10nLanguage value)
                => new(value.Value, value.Code);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left._value == right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => left._value != right._value;
        }
    }
}

#endif
