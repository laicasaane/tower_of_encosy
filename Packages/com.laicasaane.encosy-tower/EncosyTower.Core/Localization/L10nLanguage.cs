#if UNITY_LOCALIZATION

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.Localization
{
    [Serializable]
    public partial struct L10nLanguage : IEquatable<L10nLanguage>
    {
        public const string DEFAULT_LOCALE_CODE = "en";

        public readonly static L10nLanguage Default = new(0, DEFAULT_LOCALE_CODE);

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
            => IsValid ? _code : DEFAULT_LOCALE_CODE;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(L10nLanguage other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj)
            => obj is L10nLanguage other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L10nLanguage left, L10nLanguage right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L10nLanguage left, L10nLanguage right)
            => !left.Equals(right);
    }
}

#endif
