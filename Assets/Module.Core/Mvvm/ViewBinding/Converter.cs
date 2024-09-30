using System;
using Module.Core.Unions;
using UnityEngine;

namespace Module.Core.Mvvm.ViewBinding
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
