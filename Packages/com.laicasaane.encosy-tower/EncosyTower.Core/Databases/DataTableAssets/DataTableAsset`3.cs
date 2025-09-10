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
            var entries = EntriesInternal.Span;

            map.Clear();
            map.EnsureCapacity(entries.Length);

            for (var i = 0; i < entries.Length; i++)
            {
                ref var entry = ref entries[i];
                var id = ConvertId(GetId(entry));

                if (map.TryAdd(id, i) == false)
                {
                    ErrorDuplicateId(id, i, this);
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

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ErrorDuplicateId(
              TConvertedId id
            , int index
            , [NotNull] DataTableAsset<TDataId, TData, TConvertedId> context
        )
        {
            DevLoggerAPI.LogErrorFormat(
                  context
                , "Id with value \"{0}\" is duplicated at row \"{1}\""
                , context.ToString(id)
                , index
            );
        }
    }
}
