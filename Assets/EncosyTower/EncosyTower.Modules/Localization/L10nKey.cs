#if UNITY_LOCALIZATION

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Serialization;

namespace EncosyTower.Modules.Localization
{
    // ReSharper disable once InconsistentNaming

    public readonly partial struct L10nKey : IEquatable<L10nKey>
    {
        public readonly TableReference Table;
        public readonly TableEntryReference Entry;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public L10nKey(string table, string entry)
        {
            Table = table;
            Entry = entry;
        }

        public bool IsValid
            => Table.ReferenceType == TableReference.Type.Name
            && string.IsNullOrEmpty(Table.TableCollectionName) == false
            && Entry.ReferenceType == TableEntryReference.Type.Name
            && string.IsNullOrEmpty(Entry.Key) == false
            ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(L10nKey other)
            => Table.Equals(other.Table) && Entry.Equals(other.Entry);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is L10nKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Table.GetHashCode(), Entry.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"{Table.TableCollectionName},{Entry.Key}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L10nKey left, L10nKey right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L10nKey left, L10nKey right)
            => !left.Equals(right);

        [Serializable]
        public struct Serializable : ITryConvert<L10nKey>
            , IEquatable<Serializable>
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
            public readonly bool TryConvert(out L10nKey result)
            {
                result = new(_table, _entry);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Serializable other)
                => _table.Equals(other._table, StringComparison.Ordinal)
                && _entry.Equals(other._entry, StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
                => obj is Serializable other && Equals(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
                => HashCode.Combine(_table, _entry);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => $"{_table},{_entry}";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator L10nKey(Serializable value)
                => new(value._table, value._entry);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(L10nKey value)
                => new(value.Table.TableCollectionName, value.Entry.Key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left.Equals(right);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => !left.Equals(right);
        }
    }
}

#endif
