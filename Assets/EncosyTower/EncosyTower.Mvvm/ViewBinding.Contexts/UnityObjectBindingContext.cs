using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Contexts
{
    [Label("Unity Object")]
    [Serializable]
    public sealed class UnityObjectBindingContext : IBindingContext
    {
        [SerializeField] private UnityEngine.Object _object;

        public bool TryGetContext(out IObservableObject result)
        {
            if (_object.IsInvalid() || _object is not IObservableObject obj)
            {
                result = null;
                return false;
            }

            result = obj;
            return true;
        }

        public bool TryGetContextType(out Type result)
        {
            if (_object.IsInvalid())
            {
                result = default;
                return false;
            }

            result = _object.GetType();
            return true;
        }
    }
}
