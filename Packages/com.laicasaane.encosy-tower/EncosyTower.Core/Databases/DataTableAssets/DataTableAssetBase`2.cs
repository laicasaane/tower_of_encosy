using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Data;
using EncosyTower.Debugging;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Databases
{
    public abstract class DataTableAssetBase<TDataId, TData> : DataTableAssetBase
        where TData : IData, IDataWithId<TDataId>
    {
        [SerializeField]
        private TData[] _entries = new TData[0];

        public ReadOnlyMemory<TData> Entries
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entries;
        }

        internal protected Memory<TData> EntriesInternal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entries;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(_entries);

        protected abstract TDataId GetId(in TData entry);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Initialize(ref TData entry) { }

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

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorCannotCast(object obj, UnityEngine.Object context)
        {
            DevLoggerAPI.LogError(context,
                obj == null
                    ? $"Cannot cast null into {typeof(TData[])}"
                    : $"Cannot cast {obj.GetType()} into {typeof(TData[])}"
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
