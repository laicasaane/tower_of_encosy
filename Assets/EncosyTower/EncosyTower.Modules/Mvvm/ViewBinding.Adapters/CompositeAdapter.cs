using System;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Composite Adapter", "Default")]
    public sealed class CompositeAdapter : IAdapter, ISerializationCallbackReceiver
    {
        [SerializeField]
        [SerializeReference]
        [HideInInspector]
        private IAdapter[] _presetAdapters = new IAdapter[0];

        private readonly FasterList<IAdapter> _adapters = new();

        public ReadOnlyMemory<IAdapter> Adapters
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adapters.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, IAdapter adapter)
            => _adapters.Insert(index, adapter);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
            => _adapters.RemoveAt(index);

        public Union Convert(in Union union)
        {
            var adapters = _adapters.AsSpan();
            var length = adapters.Length;
            var result = union;

            for (var i = 0; i < length; i++)
            {
                result = adapters[i].Convert(result);
            }

            return result;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _presetAdapters = _adapters.ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _adapters.Clear();
            _adapters.AddRange(_presetAdapters.AsSpan());
        }
    }
}
