#if UNITY_LOCALIZATION

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Serialization;

namespace EncosyTower.Modules.Localization
{
    // ReSharper disable once InconsistentNaming

    public readonly struct L10nKey<T> : IEquatable<L10nKey<T>>, IEquatable<L10nKey>
    {
        public readonly L10nKey Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nKey(string table, string entry)
        {
            Value = new(table, entry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nKey(L10nKey value)
        {
            Value = value;
        }

        public TableReference Table
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.Table;
        }

        public TableEntryReference Entry
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.Entry;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(L10nKey<T> other)
            => Value.Equals(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(L10nKey other)
            => Value.Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is L10nKey<T> otherT
                ? Value.Equals(otherT.Value)
                : obj is L10nKey other && Value.Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => Value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator L10nKey(L10nKey<T> value)
            => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator L10nKey<T>(L10nKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L10nKey<T> left, L10nKey<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L10nKey<T> left, L10nKey<T> right)
            => !left.Equals(right);
    }

    // ReSharper disable once InconsistentNaming
    partial struct L10nKey
    {
        [Serializable]
        public struct Serializable<T> : ITryConvert<L10nKey<T>>
            , IEquatable<Serializable<T>>
        {
            [SerializeField]
            private string _table;

            [SerializeField, FormerlySerializedAs("_key")]
            private string _entry;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(string table, string entry)
            {
                _table = table;
                _entry = entry;
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => string.IsNullOrEmpty(_table) == false && string.IsNullOrEmpty(_entry) == false;
            }

            public TableReference Table
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _table;
            }

            public TableEntryReference Entry
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _entry;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out L10nKey<T> result)
            {
                result = new(_table, _entry);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Serializable<T> other)
                => _table.Equals(other._table, StringComparison.Ordinal)
                && _entry.Equals(other._entry, StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
                => obj is Serializable<T> other && Equals(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
                => HashCode.Combine(_table, _entry);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => $"{_table},{_entry}";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(Serializable value)
                => new(value.Table.TableCollectionName, value.Entry.Key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(Serializable<T> value)
                => new(value._table, value._entry);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator L10nKey<T>(Serializable<T> value)
                => new(value._table, value._entry);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(L10nKey<T> value)
                => new(value.Table.TableCollectionName, value.Entry.Key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable<T> left, Serializable<T> right)
                => left.Equals(right);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable<T> left, Serializable<T> right)
                => !left.Equals(right);
        }
    }
}

#endif
