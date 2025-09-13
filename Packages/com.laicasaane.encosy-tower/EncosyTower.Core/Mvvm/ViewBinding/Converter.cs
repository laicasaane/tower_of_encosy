using System;
using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding
{
    [Serializable]
    public sealed class Converter
    {
        [field: SerializeReference]
        public IAdapter Adapter { get; set; }

        public Variant Convert(in Variant value)
            => Adapter?.Convert(value) ?? value;
    }
}
