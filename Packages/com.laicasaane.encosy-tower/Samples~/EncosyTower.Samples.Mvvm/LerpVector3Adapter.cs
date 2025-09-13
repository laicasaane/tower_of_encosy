using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.Variants;
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

        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out float result) == false)
            {
                return variant;
            }

            var v3 = _reversed ? Vector3.Lerp(_to, _from, result) : Vector3.Lerp(_from, _to, result);
            return new Vector3Variant(v3);
        }
    }
}
