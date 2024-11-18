using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Logging;
using UnityEngine;

namespace EncosyTower.Modules.Data
{
    public abstract class DataTableAsset<TDataId, TData, TConvertedId> : DataTableAssetBase<TDataId, TData>
        where TData : IData, IDataWithId<TDataId>
    {
        private readonly Dictionary<TConvertedId, int> _idToIndexMap = new();

        protected Dictionary<TConvertedId, int> IdToIndexMap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _idToIndexMap;
        }

        internal protected override void Initialize()
        {
            var map = _idToIndexMap;
            var entries = Entries.Span;

            map.Clear();
            map.EnsureCapacity(entries.Length);

            for (var i = 0; i < entries.Length; i++)
            {
                var id = Convert(GetId(entries[i]));

                if (map.TryAdd(id, i))
                {
                    continue;
                }

                ErrorDuplicateId(id, i, this);
            }
        }

        public virtual bool TryGetEntry(TConvertedId id, out TData entry)
        {
            var result = _idToIndexMap.TryGetValue(id, out var index);
            entry = result ? Entries.Span[index] : default;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual DataEntry<TData> GetEntry(TConvertedId id)
        {
            return _idToIndexMap.TryGetValue(id, out var index) ? new(Entries.Slice(index, 1)) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual DataEntryRef<TData> GetEntryByRef(TConvertedId id)
        {
            return _idToIndexMap.TryGetValue(id, out var index) ? new(Entries.Span.Slice(index, 1)) : default;
        }

        protected abstract TConvertedId Convert(TDataId value);

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
