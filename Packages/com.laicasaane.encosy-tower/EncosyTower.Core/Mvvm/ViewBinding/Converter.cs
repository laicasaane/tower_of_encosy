using System;
using EncosyTower.Unions;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding
{
    [Serializable]
    public sealed class Converter
    {
        [field: SerializeReference]
        public IAdapter Adapter { get; set; }

        public Union Convert(in Union value)
            => Adapter?.Convert(value) ?? value;
    }
}
