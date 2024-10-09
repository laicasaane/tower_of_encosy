using System;
using Module.Core.Mvvm.ComponentModel;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Unity
{
    [Serializable]
    public abstract class ObservableContext
    {
        public abstract bool TryGetContext(out IObservableObject result);

        public abstract bool TryGetContextType(out Type result);
    }

    [Label("Unity Object Context")]
    [Serializable]
    public sealed class ObservableUnityObjectContext : ObservableContext
    {
        [SerializeField]
        private UnityEngine.Object _object;

        public override bool TryGetContext(out IObservableObject result)
        {
            result = _object as IObservableObject;
            return _object;
        }

        public override bool TryGetContextType(out Type result)
        {
            if (_object == false)
            {
                result = default;
                return false;
            }

            result = _object.GetType();
            return true;
        }
    }
}
