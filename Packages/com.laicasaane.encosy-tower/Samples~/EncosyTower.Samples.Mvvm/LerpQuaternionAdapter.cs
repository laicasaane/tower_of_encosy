using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    [Serializable]
    [Label("Lerp Quaternion", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(Quaternion), order: 0)]
    public sealed class LerpQuaternionAdapter : IAdapter
    {
        [SerializeField] private Vector3 _fromEuler;
        [SerializeField] private Vector3 _toEuler;
        [SerializeField] private bool _reversed;

        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out float result) == false)
            {
                return variant;
            }

            var quaternion = _reversed
                ? Quaternion.Euler(Vector3.Lerp(_toEuler, _fromEuler, result))
                : Quaternion.Euler(Vector3.Lerp(_fromEuler, _toEuler, result));

            return new QuaternionVariant(quaternion);
        }
    }
}
