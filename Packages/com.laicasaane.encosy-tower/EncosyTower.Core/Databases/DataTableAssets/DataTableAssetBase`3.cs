using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Data;
using EncosyTower.Debugging;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Databases
{
    public abstract class DataTableAssetBase<TDataId, TData, TConvertedId> : DataTableAssetBase, IDataTableAsset<TData>
        where TData : IData, IDataWithId<TDataId>
    {
        [SerializeField]
        private TData[] _entries = new TData[0];

        protected readonly ArrayMap<TConvertedId, int> IdToIndexMap = new();

        public ReadOnlyMemory<TData> Entries
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entries;
        }

        internal protected sealed override void Initialize()
        {
            var map = IdToIndexMap;
            map.Clear();
            map.EnsureCapacity(GetEntries().Length);

            OnBeforeInitialize();
            OnInitialize();
            OnAfterInitialize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(_entries);

        public bool TryGetEntry(TConvertedId id, out TData entry)
        {
            var result = IdToIndexMap.TryGetValue(id, out var index);
            entry = result ? Entries.Span[index] : default;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataEntry<TData> GetEntry(TConvertedId id)
        {
            return IdToIndexMap.TryGetValue(id, out var index)
                ? DataEntry.GetEntryAt(Entries, index)
                : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataEntryRef<TData> GetEntryByRef(TConvertedId id)
        {
            return IdToIndexMap.TryGetValue(id, out var index)
                ? DataEntryRef.GetEntryAt(Entries, index)
                : default;
        }

        protected abstract TDataId GetId(in TData entry);

        protected abstract TConvertedId ConvertId(TDataId value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnBeforeInitialize() { }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInitialize()
        {
            var entries = GetEntries().AsSpan();
            var map = IdToIndexMap;

            for (var i = 0; i < entries.Length; i++)
            {
                ref var entry = ref entries[i];
                var id = ConvertId(GetId(entry));

                if (map.TryAdd(id, i) == false)
                {
                    ErrorDuplicateId(id, entry.Id, i, this);
                    continue;
                }

                Initialize(ref entry);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnAfterInitialize() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Initialize(ref TData entry) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string ToString(TConvertedId value)
            => value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string ToString(TDataId value)
            => value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal protected ref TData[] GetEntries()
            => ref _entries;

        internal sealed override void SetEntries(object obj)
        {
            if (obj is TData[] entries)
            {
                _entries = entries;
                return;
            }

            if (_entries == null || _entries.Length != 0)
            {
                _entries = new TData[0];
            }

            ErrorCannotCast(obj, this);
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorCannotCast(object obj, UnityEngine.Object context)
        {
            StaticDevLogger.LogError(context,
                obj == null
                    ? $"Cannot cast null into {typeof(TData[])}"
                    : $"Cannot cast {obj.GetType()} into {typeof(TData[])}"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ErrorDuplicateId(
              TConvertedId convertedId
            , TDataId id
            , int index
            , [NotNull] DataTableAssetBase<TDataId, TData, TConvertedId> context
        )
        {
            StaticDevLogger.LogErrorFormat(
                  context
                , "Id \"{0}\" (converted from \"{1}\") is duplicated at row \"{2}\""
                , context.ToString(convertedId)
                , context.ToString(id)
                , index
            );
        }

        public struct Enumerator
        {
            private readonly TData[] _entries;
            private readonly int _size;
            private int _counter;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(in TData[] entries)
            {
                _counter = 0;
                _size = entries.Length;
                _entries = entries;
            }

            public readonly TData Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    Checks.IsTrue(_counter <= _size, "_counter must be lesser than or equal to _size");
                    return _entries[_counter - 1];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => _counter++ < _size;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
                => _counter = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose() { }
        }
    }
}
