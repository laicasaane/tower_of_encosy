using System;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Adapters.Unity
{
    [Serializable]
    [Label("Scriptable Adapter", "Default")]
    public sealed class ScriptableAdapter : IAdapter
    {
        [SerializeField, HideInInspector]
        private ScriptableAdapterAsset _asset;

        public Union Convert(in Union union)
        {
            if (_asset)
            {
                return _asset.Convert(union);
            }

            return union;
        }
    }
}
