using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    [Serializable]
    [Label("Start/Stop ⇒ Color", "Default")]
    [Adapter(sourceType: typeof(bool), destType: typeof(Color), order: 0)]
    public sealed class StartStopColorAdapter : IAdapter
    {
        [SerializeField] private Color _start = Color.white;
        [SerializeField] private Color _stop = Color.white;
        [SerializeField] private bool _invert = false;

        public Variant Convert(in Variant variant)
        {
            return variant.TryGetValue(out bool result)
                ? ActualValue(result) ? new ColorVariant(_stop) : new ColorVariant(_start)
                : variant;
        }

        private bool ActualValue(bool value) => _invert ? !value : value;
    }
}
