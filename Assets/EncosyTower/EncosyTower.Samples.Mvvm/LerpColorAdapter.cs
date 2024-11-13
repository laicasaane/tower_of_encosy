using System;
using EncosyTower.Modules;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    [Serializable]
    [Label("Lerp Color", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(Color), order: 0)]
    public sealed class LerpColorAdapter : IAdapter
    {
        [SerializeField] private Color _from = Color.white;
        [SerializeField] private Color _to = Color.white;
        [SerializeField] private bool _reversed;

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out float result) == false)
            {
                return union;
            }

            var color = _reversed ? Color.Lerp(_to, _from, result) : Color.Lerp(_from, _to, result);
            return new ColorUnion(color);
        }
    }
}
