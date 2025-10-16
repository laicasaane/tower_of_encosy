using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Annotations;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Variants;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Sequence of Adapters", "Default")]
    public sealed class SequentialAdapter : IAdapter
    {
        [SerializeField, SerializeReference, HideInInspector]
        [FormerlySerializedAs("_presetAdapters")]
        private List<IAdapter> _adapters;

        public ListFast<IAdapter>.ReadOnly Adapters
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adapters ??= new();
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
    }
}
