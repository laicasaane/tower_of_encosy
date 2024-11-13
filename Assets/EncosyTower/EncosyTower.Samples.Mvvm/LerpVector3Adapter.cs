using System;
using EncosyTower.Modules;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    [Serializable]
    [Label("Lerp Vector3", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(Vector3), order: 0)]
    public sealed class LerpVector3Adapter : IAdapter
    {
        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _to;
        [SerializeField] private bool _reversed;

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out float result) == false)
            {
                return union;
            }

            var v3 = _reversed ? Vector3.Lerp(_to, _from, result) : Vector3.Lerp(_from, _to, result);
            return new Vector3Union(v3);
        }
    }
}
