#if UNITY_LOCALIZATION

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace EncosyTower.Localization
{
    [Serializable]
    public struct L10nKey<T> : IEquatable<L10nKey<T>>, IEquatable<L10nKey>
    {
        [SerializeField] internal TableReference _table;
        [SerializeField] internal TableEntryReference _entry;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nKey(string table, string entry)
        {
            _table = table;
            _entry = entry;
        }

        public bool IsValid
            => _table.ReferenceType == TableReference.Type.Name
            && _table.TableCollectionName.IsNotEmpty()
            && _entry.ReferenceType == TableEntryReference.Type.Name
            && _entry.Key.IsNotEmpty()
            ;

        public readonly TableReference Table
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _table;
        }

        public readonly TableEntryReference Entry
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entry;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(L10nKey<T> other)
            => _table.Equals(other._table) && _entry.Equals(other._entry);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(L10nKey other)
            => _table.Equals(other._table) && _entry.Equals(other._entry);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is L10nKey<T> otherT
                ? Equals(otherT)
                : obj is L10nKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashCode.Combine(_table, _entry);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => $"{_table.TableCollectionName}[{_entry.Key}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator L10nKey(L10nKey<T> value)
            => new(value._table, value._entry);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator L10nKey<T>(L10nKey value)
            => new(value._table, value._entry);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L10nKey<T> left, L10nKey<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L10nKey<T> left, L10nKey<T> right)
            => !left.Equals(right);
    }
}

#endif
