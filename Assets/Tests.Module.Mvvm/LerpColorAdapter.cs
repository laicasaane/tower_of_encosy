using System;
using Module.Core;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;
using UnityEngine;

namespace Tests.Module.Mvvm
{
    [Serializable]
    [Label("Lerp Color", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(Color), order: 0)]
    public sealed class LerpColorAdapter : IAdapter
    {
        [SerializeField] private Color _from = Color.white;
        [SerializeField] private Color _to = Color.white;

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out float result))
            {
                return new ColorUnion(Color.Lerp(_from, _to, result));
            }

            return union;
        }
    }
}
