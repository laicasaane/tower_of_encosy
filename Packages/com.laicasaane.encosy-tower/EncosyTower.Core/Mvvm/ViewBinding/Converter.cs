using System;
using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding
{
    [Serializable]
    public struct Converter
    {
        [field: SerializeReference]
        public IAdapter Adapter { get; set; }

        public readonly Variant Convert(in Variant value)
            => Adapter?.Convert(value) ?? value;
    }
}
