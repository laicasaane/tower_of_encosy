using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Databases
{
    public static class DataEntry
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataEntry<T> GetEntryAt<T>(ReadOnlyMemory<T> entries, int index)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new(entries.Slice(index, 1));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    public readonly struct DataEntry<T> : IEquatable<DataEntry<T>>
    {
        private readonly ReadOnlyMemory<T> _value;

        [Obsolete(
            "This constructor is not intended to be used directly by user code. " +
            "Please use DataEntry.GetEntryAt instead."
        )]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DataEntry(ReadOnlyMemory<T> value)
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
            return ref _value.Span[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetValueByRef(ref T defaultValue)
        {
            return ref IsValid ? ref _value.Span[0] : ref defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValue()
        {
            ThrowIfInvalid(IsValid);
            return _value.Span[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValue(T defaultValue)
        {
            return IsValid ? _value.Span[0] : defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DataEntry<T> other)
        {
            return _value.Equals(other._value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is DataEntry<T> other && _value.Equals(other._value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return _value.IsEmpty == false
                ? HashCode.Combine(_value, 0, 1)
                : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DataEntry<T> left, DataEntry<T> right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DataEntry<T> left, DataEntry<T> right)
        {
            return !(left == right);
        }

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
