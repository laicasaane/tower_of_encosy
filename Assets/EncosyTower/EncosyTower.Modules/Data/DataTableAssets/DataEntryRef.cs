using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Data
{
    public readonly ref struct DataEntryRef<T>
    {
        private readonly ReadOnlySpan<T> _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataEntryRef(ReadOnlySpan<T> value)
        {
            _value = value;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsEmpty == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetValueByRef()
        {
            ThrowIfInvalid(IsValid);
            return ref _value[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetValueByRef(ref T defaultValue)
        {
            return ref IsValid ? ref _value[0] : ref defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValue()
        {
            ThrowIfInvalid(IsValid);
            return _value[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValue(T defaultValue)
        {
            return IsValid ? _value[0] : defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DataEntryRef<T> other)
        {
            return _value == other._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DataEntryRef<T> left, DataEntryRef<T> right)
        {
            return left._value == right._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DataEntryRef<T> left, DataEntryRef<T> right)
        {
            return left._value != right._value;
        }

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        [Obsolete("Equals() on DataAccessorRef will always throw an exception. Use the equality operator instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            throw new NotSupportedException(
                "Equals() on DataAccessorRef will always throw an exception. Use the equality operator instead."
            );
        }

        [Obsolete("GetHashCode() on DataAccessorRef will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            throw new NotSupportedException(
                "GetHashCode() on DataAccessorRef will always throw an exception."
            );
        }
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw new InvalidOperationException("DataRef is invalid");
            }
        }
    }
}
