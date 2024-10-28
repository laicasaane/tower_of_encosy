using System;
using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding
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
