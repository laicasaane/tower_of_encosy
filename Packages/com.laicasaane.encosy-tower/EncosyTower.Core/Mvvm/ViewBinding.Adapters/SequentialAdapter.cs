using System;
using System.Runtime.CompilerServices;
using EncosyTower.Annotations;
using EncosyTower.Collections;
using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Sequence of Adapters", "Default")]
    public sealed class SequentialAdapter : IAdapter, ISerializationCallbackReceiver
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

        public Variant Convert(in Variant variant)
        {
            var adapters = _adapters.AsSpan();
            var length = adapters.Length;
            var result = variant;

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
