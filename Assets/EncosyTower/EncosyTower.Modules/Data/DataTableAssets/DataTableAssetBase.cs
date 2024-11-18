using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.Modules.Data
{
    public abstract class DataTableAssetBase<TDataId, TData> : DataTableAsset
        where TData : IData, IDataWithId<TDataId>
    {
        [SerializeField, FormerlySerializedAs("_rows")]
        private TData[] _entries;

        public ReadOnlyMemory<TData> Entries
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entries;
        }

        protected abstract TDataId GetId(in TData data);

        internal sealed override void SetEntries(object obj)
        {
            if (obj is TData[] entries)
            {
                _entries = entries;
            }
            else
            {
                _entries = Array.Empty<TData>();
                ErrorCannotCast(obj, this);
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorCannotCast(object obj, UnityEngine.Object context)
        {
            DevLoggerAPI.LogError(context,
                obj == null
                    ? $"Cannot cast null into {typeof(TData[])}"
                    : $"Cannot cast {obj.GetType()} into {typeof(TData[])}"
            );
        }
    }
}
