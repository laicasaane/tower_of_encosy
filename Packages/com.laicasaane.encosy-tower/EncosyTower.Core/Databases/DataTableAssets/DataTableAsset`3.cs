using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Data;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Databases
{
    public abstract class DataTableAsset<TDataId, TData, TConvertedId> : DataTableAssetBase<TDataId, TData>
        where TData : IData, IDataWithId<TDataId>
    {
        protected readonly Dictionary<TConvertedId, int> IdToIndexMap = new();

        internal protected override void Initialize()
        {
            var map = IdToIndexMap;
            var entries = GetEntries().AsSpan();

            map.Clear();
            map.EnsureCapacity(entries.Length);

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

        public virtual bool TryGetEntry(TConvertedId id, out TData entry)
        {
            var result = IdToIndexMap.TryGetValue(id, out var index);
            entry = result ? Entries.Span[index] : default;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual DataEntry<TData> GetEntry(TConvertedId id)
        {
            return IdToIndexMap.TryGetValue(id, out var index)
                ? DataEntry.GetEntryAt(Entries, index)
                : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual DataEntryRef<TData> GetEntryByRef(TConvertedId id)
        {
            return IdToIndexMap.TryGetValue(id, out var index)
                ? DataEntryRef.GetEntryAt(Entries, index)
                : default;
        }

        protected abstract TConvertedId ConvertId(TDataId value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string ToString(TConvertedId value)
            => value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string ToString(TDataId value)
            => value.ToString();

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ErrorDuplicateId(
              TConvertedId convertedId
            , TDataId id
            , int index
            , [NotNull] DataTableAsset<TDataId, TData, TConvertedId> context
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
    }
}
