using System;
using EncosyTower.Modules;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Unions;
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

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out bool result))
            {
                return ActualValue(result) ? new ColorUnion(_stop) : new ColorUnion(_start);
            }

            return union;
        }

        private bool ActualValue(bool value) => _invert ? !value : value;
    }
}
