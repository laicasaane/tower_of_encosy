using System;
using EncosyTower.Modules.Mvvm.ComponentModel;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Unity
{
    [Label("Unity Object")]
    [Serializable]
    public sealed class UnityObjectBindingContext : IBindingContext
    {
        [SerializeField] private UnityEngine.Object _object;

        public bool TryGetContext(out IObservableObject result)
        {
            result = _object as IObservableObject;
            return result != null;
        }

        public bool TryGetContextType(out Type result)
        {
            result = _object ? _object.GetType() : null;
            return result != null;
        }
    }
}
