using System;
using EncosyTower.Modules;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    [Serializable]
    [Label("Lerp Euler Angles", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(Vector3), order: 0)]
    public sealed class LerpEulerAnglesAdapter : IAdapter
    {
        [SerializeField] private Vector3 _from = Vector3.zero;
        [SerializeField] private Vector3 _to = Vector3.zero;

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out float result))
            {
                return new Vector3Union(Vector3.Lerp(_from, _to, result));
            }

            return union;
        }
    }
}
